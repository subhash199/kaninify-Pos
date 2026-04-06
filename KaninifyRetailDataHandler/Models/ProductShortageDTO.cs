using EntityFrameworkDatabaseLibrary.Models;
using System;

namespace DataHandlerLibrary.Models
{
    public class ProductShortageDTO
    {
        public Product Product { get; set; }
        public double AverageDailySales { get; set; }
        public int TargetDaysStock { get; set; }
        public int UnitsNeeded { get; set; }
        public int CasesNeeded { get; set; }
        public int CurrentStock { get; set; }

        public ProductShortageDTO(Product product, double averageDailySales, int targetDaysStock = 14)
        {
            Product = product ?? throw new ArgumentNullException(nameof(product));
            AverageDailySales = averageDailySales;
            TargetDaysStock = targetDaysStock;
            CurrentStock = product.ShelfQuantity + product.StockroomQuantity;
            
            // Calculate units needed for target days of stock
            var targetStock = (int)Math.Ceiling(averageDailySales * targetDaysStock);
            UnitsNeeded = Math.Max(0, targetStock - CurrentStock);
            
            // Calculate cases needed (if product has units per case defined)
            if (product.Product_Unit_Per_Case > 0 && UnitsNeeded > 0)
            {
                CasesNeeded = (int)Math.Ceiling((double)UnitsNeeded / product.Product_Unit_Per_Case);
            }
            else
            {
                CasesNeeded = 0;
            }
        }
    }
}