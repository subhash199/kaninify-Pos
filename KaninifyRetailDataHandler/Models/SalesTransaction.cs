using DataHandlerLibrary.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace EntityFrameworkDatabaseLibrary.Models
{
    public class SalesTransaction
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    

        public virtual List<SalesItemTransaction>? SalesItemTransactions { get; set; }
        public int SaleTransaction_Total_QTY { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Total_Amount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Total_Paid { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Cash { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Card { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Refund { get; set; }
        [Column(TypeName = "decimal(18,2")]
        public decimal SaleTransaction_Discount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Promotion { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Change { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Payout { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_CashBack { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Card_Charges { get; set; }
        public int DayLog_Id { get; set; }  // Changed from DayLog_ID to DayLog_Id
        public virtual DayLog? DayLog { get; set; }
        public DateTime Sale_Date { get; set; }
        public bool Is_Printed { get; set; } = false;
        public DateTime Date_Created { get; set; } = DateTime.UtcNow;
        public DateTime Last_Modified { get; set; } = DateTime.UtcNow;
        public DateTime Sale_Start_Date { get; set; }

        [ForeignKey(nameof(Shift))]
        public int? Shift_Id { get; set; }
        public int Created_By_Id { get; set; }
        public int Last_Modified_By_Id { get; set; }
        public int? Site_Id { get; set; }
        public int? Till_Id { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
        public virtual PosUser? Created_By { get; set; }
        public virtual PosUser? Last_Modified_By { get; set; }
        public virtual Site? Site { get; set; }
        public virtual Till? Till { get; set; }
        public virtual Shift? Shift { get; set; }

    }
}
