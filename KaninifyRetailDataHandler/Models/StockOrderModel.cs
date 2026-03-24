using DataHandlerLibrary.Models;
using EntityFrameworkDatabaseLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EposDataHandler.Models
{
    public class StockOrderModel
    {
        
        public Product Product { get; set; } = new Product();
        public int ProductId { get; set; }
    

        public string ProductName { get; set; } = string.Empty;
        public string ProductBarcode { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public double CurrentStock { get; set; }
        public int YearlySales { get; set; }
        public double DailyAverage { get; set; }
        public double SevenDayForecast { get; set; }
        public int RequiredCases { get; set; }
        public int CaseSize { get; set; }
        public decimal EstimatedCost { get; set; }
        public string Priority { get; set; } = "Low";
        public int? SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public DateTime? LastOrderDate { get; set; }
        public DateTime? LastSyncedAt { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
        public string? SyncError { get; set; }
        public int SyncRetryCount { get; set; } = 0;
        public long SyncVersion { get; set; } = 1;
    }
}
