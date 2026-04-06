using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaTransactions")]
    public class SupaTransactions
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("SaleTransaction_ID")]
        public int SaleTransaction_ID { get; set; }

        [Column("SaleTransaction_Total_QTY")]
        public int SaleTransaction_Total_QTY { get; set; }

        [Column("SaleTransaction_Total_Amount", TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Total_Amount { get; set; }

        [Column("SaleTransaction_Total_Paid", TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Total_Paid { get; set; }

        [Column("SaleTransaction_Cash", TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Cash { get; set; }

        [Column("SaleTransaction_Card", TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Card { get; set; }

        [Column("SaleTransaction_Discount", TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Discount { get; set; }

        [Column("SaleTransaction_Promotion", TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Promotion { get; set; }

        [Column("SaleTransaction_Change", TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Change { get; set; }

        [Column("SaleTransaction_Payout", TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Payout { get; set; }

        [Column("SaleTransaction_CashBack", TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_CashBack { get; set; }

        [Column("SaleTransaction_Card_Charges", TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Card_Charges { get; set; }

        [Column("DayLog_Id")]
        public int DayLog_Id { get; set; }

        [Column("Sale_Date")]
        public DateTime Sale_Date { get; set; }

        [Column("Date_Created")]
        public DateTime Date_Created { get; set; }

        [Column("Last_Modified")]
        public DateTime Last_Modified { get; set; }

        [Column("Sale_Start_Date")]
        public DateTime Sale_Start_Date { get; set; }

        [Column("Shift_Id")]
        public int? Shift_Id { get; set; }

        [Column("Created_By_Id")]
        public int Created_By_Id { get; set; }

        [Column("Last_Modified_By_Id")]
        public int Last_Modified_By_Id { get; set; }

        [Column("Site_Id")]
        public int? Site_Id { get; set; }

        [Column("Till_Id")]
        public int? Till_Id { get; set; }

        [Column("Shift_ID")]
        public int? Shift_ID { get; set; }

        [Column("SaleTransaction_Refund", TypeName = "decimal(18,2)")]
        public decimal SaleTransaction_Refund { get; set; }

        [Column("Is_Printed")]
        public bool Is_Printed { get; set; }

        [Column("SyncStatus")]
        public SyncStatus SyncStatus { get; set; }

        public SupaTransactions()
        {
            // Initialize default values
            Date_Created = DateTime.UtcNow;
            Last_Modified = DateTime.UtcNow;
            Is_Printed = false;
            SyncStatus = SyncStatus.Pending;
        }
    }
}