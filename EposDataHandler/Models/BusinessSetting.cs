using DataHandlerLibrary.Models.Enum;
using EntityFrameworkDatabaseLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models
{
    public class BusinessSetting
    {
        public int Id { get; set; }
        public RefillStageEnum RefillEnforcement_Enabled { get; set; } = RefillStageEnum.Both;
        public bool AllowShiftEndWithPendingRefill { get; set; } = false;
        public bool AllowDayEndWithPendingRefills { get; set; } = false;
        public bool PrintSalesReceiptForEverySale { get; set; } = false;
        public bool PrintRefundReceiptForEveryRefund { get; set; } = false;
        public bool PrintPayoutReceiptForEveryPayout { get; set; } = false;
        public DateTime Date_Created { get; set; } = DateTime.UtcNow;
        public DateTime Last_Modified { get; set; } = DateTime.UtcNow;
        public int? Created_By_Id { get; set; }
        public int? Last_Modified_By_Id { get; set; }
        public int? Site_Id { get; set; }
        public int? Till_Id { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
        public virtual PosUser? Created_By { get; set; }
        public virtual PosUser? Last_Modified_By { get; set; }
        public virtual Site? Site { get; set; }
        public virtual Till? Till { get; set; }
    }
}
