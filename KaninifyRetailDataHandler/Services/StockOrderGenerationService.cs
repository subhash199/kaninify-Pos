using DataHandlerLibrary.Services;
using EposDataHandler.Models;
using EposDataHandler.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EposDataHandler.Services
{
    public class StockOrderGenerationService
    {
        private readonly SalesItemTransactionServices _salesItemTransactionServices;
        private readonly ProductServices _productServices;
        private readonly DepartmentServices _departmentServices;
        private readonly SupplierServices _supplierServices;

        public StockOrderGenerationService(
            SalesItemTransactionServices salesItemTransactionServices,
            ProductServices productServices,
            DepartmentServices departmentServices,
            SupplierServices supplierServices)
        {
            _salesItemTransactionServices = salesItemTransactionServices;
            _productServices = productServices;
            _departmentServices = departmentServices;
            _supplierServices = supplierServices;
        }

        public async Task<StockOrderReport> GetDetailedStockOrderReportAsync()
        {
            try
            {
                var report = new StockOrderReport
                {
                    GeneratedDate = DateTime.Now,
                    AnalysisPeriodDays = 365,
                    ForecastDays = 7,
                    StockOrders = new List<StockOrderModel>()
                };

                // Get all active products
                var products = await _productServices.GetAllAsync(false);
                var activeProducts = products.Where(p => p.Is_Activated && !p.Is_Deleted).ToList();
                
                // Get departments to exclude (payouts, services, generic)
                var departments = await _departmentServices.GetAllAsync(false);
                
                // Get suppliers for supplier name lookup
                var suppliers = await _supplierServices.GetAllAsync(false);
                var excludedDepartmentIds = departments
                    .Where(d => d.Separate_Sales_In_Reports == true)
                    .Select(d => d.Id)
                    .ToList();

                     // Get department names for barcode filtering
                var departmentNames = departments.Select(d => d.Department_Name.ToLower()).ToList();

                // Calculate date range for analysis (last 365 days)
                var endDate = DateTime.Now.Date;
                var startDate = endDate.AddDays(-365);

                // Get sales data for the period
                var salesTransactions = (await _salesItemTransactionServices.GetByConditionAsync(s => s.SalesTransaction.Sale_Date >= startDate.ToUniversalTime() &&
                               s.SalesTransaction.Sale_Date <= endDate.ToUniversalTime() && s.SalesPayout_ID == null && !excludedDepartmentIds.Contains(s.Product.Department_ID), true)).ToList();


                foreach (var product in activeProducts)
                {
                    // Skip products from excluded departments
                    if (excludedDepartmentIds.Contains(product.Department_ID))
                        continue;

                    // Skip products where barcode is actually a department name
                    if (!string.IsNullOrEmpty(product.Product_Barcode) && 
                        departmentNames.Contains(product.Product_Barcode.ToLower()))
                        continue;

                    // Get sales data for this product
                    var productSales = salesTransactions
                        .Where(s => s.Product_ID == product.Id)
                        .ToList();

                    if (!productSales.Any())
                        continue; // Skip products with no sales history

                    // Calculate sales metrics
                    var totalQuantitySold = Math.Max(0, productSales.Sum(s => s.Product_QTY));
                    var totalSalesAmount = Math.Max(0, productSales.Sum(s => s.Product_Amount));
                    var dailyAverageSales = Math.Max(0, totalQuantitySold / 365.0);
                    var forecastFor7Days = Math.Max(0, dailyAverageSales * 7);

                    // Calculate current stock
                    var currentStock = (product.ShelfQuantity) + (product.StockroomQuantity);

                    // Determine if restocking is needed
                    var stockNeeded = Math.Max(0, forecastFor7Days - currentStock);

                    if (stockNeeded > 0)
                    {
                        var caseSize = Math.Max(1, product.Product_Unit_Per_Case); // Ensure case size is at least 1
                        var requiredCases = Math.Max(0, Math.Ceiling(stockNeeded / caseSize));
                        
                        // Safe cost calculation with bounds checking and overflow protection
                        decimal estimatedCost = 0;
                        try
                        {
                            var costPerCase = Math.Max(0, Math.Min(product.Product_Cost_Per_Case, 1000000));
                            estimatedCost = (decimal)requiredCases * (decimal)costPerCase;
                        }
                        catch (OverflowException)
                        {
                            estimatedCost = 0; // Fallback to 0 if calculation overflows
                        }

                        var department = departments.FirstOrDefault(d => d.Id == product.Department_ID);
                        var stockOrder = new StockOrderModel
                        {
                            Product = product,
                            ProductId = product.Id,
                            ProductName = product.Product_Name,
                            ProductBarcode = product.Product_Barcode,
                            DepartmentName = department?.Department_Name ?? "Unknown",
                            CurrentStock = currentStock,
                            YearlySales = (int)totalQuantitySold,
                            DailyAverage = dailyAverageSales,
                            SevenDayForecast = forecastFor7Days,
                            RequiredCases = (int)requiredCases,
                            CaseSize = caseSize,
                            EstimatedCost = estimatedCost,
                            Priority = GetPriority(dailyAverageSales, currentStock)
                        };

                        report.StockOrders.Add(stockOrder);
                    }
                }

                // Sort by priority and daily average sales (descending)
                report.StockOrders = report.StockOrders
                    .OrderBy(so => GetPriorityOrder(so.Priority))
                    .ThenByDescending(so => so.DailyAverage)
                    .ToList();

                // Calculate summary statistics
                report.TotalProductsAnalyzed = activeProducts.Count(p => !excludedDepartmentIds.Contains(p.Department_ID));
                report.ProductsRequiringRestock = report.StockOrders.Count;
                report.TotalEstimatedCost = report.StockOrders.Sum(so => so.EstimatedCost);
                report.TotalCasesRequired = report.StockOrders.Sum(so => so.RequiredCases);

                return report;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating stock order report: {ex.Message}", ex);
            }
        }

        private string GetPriority(double dailyAverage, double currentStock)
        {
            if (currentStock <= 0)
                return "High";

            var daysOfStock = currentStock / Math.Max(dailyAverage, 0.1);

            if (daysOfStock <= 2)
                return "High";
            else if (daysOfStock <= 5)
                return "Medium";
            else
                return "Low";
        }

        private int GetPriorityOrder(string priority)
        {
            return priority switch
            {
                "High" => 1,
                "Medium" => 2,
                "Low" => 3,
                _ => 4
            };
        }
    }

    public class StockOrderReport
    {
        public DateTime GeneratedDate { get; set; }
        public int AnalysisPeriodDays { get; set; }
        public int ForecastDays { get; set; }
        public int TotalProductsAnalyzed { get; set; }
        public int ProductsRequiringRestock { get; set; }
        public decimal TotalEstimatedCost { get; set; }
        public int TotalCasesRequired { get; set; }
        public List<StockOrderModel> StockOrders { get; set; } = new List<StockOrderModel>();
    }
}