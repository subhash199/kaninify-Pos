using System;
using System.Collections.Generic;

namespace DataHandlerLibrary.Models
{
    /// <summary>
    /// Comprehensive sales dashboard data model containing all calculated metrics
    /// </summary>
    public class SalesDashboardData
    {
        // Date Range
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public decimal TotalSales { get; set; }
        public int TotalTransactions { get; set; }
        public decimal AverageTransaction { get; set; }
        public int ItemsSold { get; set; }

        // Financial Metrics
        public decimal NetSales { get; set; }
        public decimal TotalRefunds { get; set; }
        public int RefundCount { get; set; }
        public decimal TotalPayouts { get; set; }
        public decimal VoidedSales { get; set; }
        public int VoidCount { get; set; }

        //Payment Metrics
        public decimal CashPayments { get; set; }
        public decimal CardPayments { get; set; }
        public decimal ChangePayments { get; set; }

        //Payment Count
        public int CashTransactionCount { get; set; }
        public int CardTransactionCount { get; set; }
        public int ChangeTransactionCount { get; set; }
        public decimal OtherServices { get; set; }

        public decimal MorningSales { get; set; }
        public int MorningTransactions { get; set; }
        public decimal AfternoonSales { get; set; }
        public int AfternoonTransactions { get; set; }
        public decimal NightSales { get; set; }
        public int NightTransactions { get; set; }

        public List<EmployeePerformanceData> EmployeePerformance { get; set; } = new();
        public List<ProductSalesData> TopProducts { get; set; } = new();
        public List<ProductSalesData> LeastPopularProducts { get; set; } = new();

        public int ExpiredProductsCount { get; set; }
        public int TheftCount { get; set; }
        public int StockAdjustmentsCount { get; set; }
        public List<StockTransactionData> StockTransactions { get; set; } = new();

        // Transaction Details
        public List<RefundTransactionData> RefundTransactions { get; set; } = new();
        public List<VoidedTransactionData> VoidedTransactions { get; set; } = new();
        public List<PayoutTransactionData> PayoutTransactions { get; set; } = new();
        public List<OtherServicesTransactionData> OtherServicesTransactions { get; set; } = new();

        // Department Sales
        public List<DepartmentSalesData> DepartmentSales { get; set; } = new();

        // Additional data for printing
        public List<PaymentMethodData> PaymentMethods { get; set; } = new();
        public List<VatSummaryData> VatSummary { get; set; } = new();

        // Stock Refill Data
        public int StockRefillCount { get; set; }

        public List<StockRefillData> StockRefillProducts { get; set; } = new();

        public List<MiscProductData> MiscProducts { get; set; } = new();
    }

    /// <summary>
    /// Employee performance metrics
    /// </summary>
    public class EmployeePerformanceData
    {
        public string EmployeeName { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
        public int TransactionCount { get; set; }
        public int ItemsSold { get; set; }
        public TimeSpan AverageServiceTime { get; set; }
        public double AverageItemsPerTransaction { get; set; }
    }

    /// <summary>
    /// Product sales performance data
    /// </summary>
    public class ProductSalesData
    {
        public string ProductName { get; set; } = string.Empty;
        public int QuantitySold { get; set; }
        public decimal Revenue { get; set; }
    }

    /// <summary>
    /// Stock transaction details
    /// </summary>
    public class StockTransactionData
    {
        public DateTime TransactionDate { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string TransactionType { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal ValueImpact { get; set; }
    }

    /// <summary>
    /// Refund transaction details
    /// </summary>
    public class RefundTransactionData
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Voided transaction details
    /// </summary>
    public class VoidedTransactionData
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
    }

    /// <summary>
    /// Payout transaction details
    /// </summary>
    public class PayoutTransactionData
    {
        public DateTime TransactionDate { get; set; }
        public string PayoutId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Recipient { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Other services transaction details
    /// </summary>
    public class OtherServicesTransactionData
    {
        public DateTime TransactionDate { get; set; }
        public string TransactionId { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal Amount { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    /// <summary>
    /// Department sales performance data
    /// </summary>
    public class DepartmentSalesData
    {
        public string DepartmentName { get; set; } = string.Empty;
        public decimal TotalSales { get; set; }
        public int TransactionCount { get; set; }
        public int ItemsSold { get; set; }
        public decimal Percentage { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }

    /// <summary>
    /// Payment method summary data
    /// </summary>
    public class PaymentMethodData
    {
        public string PaymentMethod { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// VAT summary data
    /// </summary>
    public class VatSummaryData
    {
        public string VatDescription { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal VatAmount { get; set; }
        public decimal VatRate { get; set; }
    }

    /// <summary>
    /// Stock Refill Data
    ///  </summary>
    public class StockRefillData
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int RefillQuantity { get; set; }
        public int ShelfQuantity { get; set; }
        public int StockRoomQuantity { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRefilled { get; set; }
        public string Status { get; set; } = "Pending";
    }

    public class MiscProductData
    {
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public DateTime DateTime { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
    }


}