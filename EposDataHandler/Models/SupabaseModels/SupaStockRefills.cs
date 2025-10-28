using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaStockRefills")]
    public class SupaStockRefills
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("StockRefill_ID")]
        public int StockRefill_ID { get; set; }

        [Column("SaleTransaction_Item_ID")]
        public int SaleTransaction_Item_ID { get; set; }

        [Column("Refilled_By")]
        public int? Refilled_By { get; set; }

        [Column("Refilled_Date")]
        public DateTimeOffset? Refilled_Date { get; set; }

        [Column("Confirmed_By_Scanner")]
        public bool Confirmed_By_Scanner { get; set; }

        [Column("Refill_Quantity")]
        public int Refill_Quantity { get; set; }

        [Column("Quantity_Refilled")]
        public int Quantity_Refilled { get; set; }

        [Column("Stock_Refilled")]
        public bool Stock_Refilled { get; set; }

        [Column("Shift_ID")]
        public int? Shift_ID { get; set; }

        [Column("DayLog_ID")]
        public int? DayLog_ID { get; set; }

        [Column("Created_By_ID")]
        public int Created_By_ID { get; set; }

        [Column("Last_Modified_By_ID")]
        public int? Last_Modified_By_ID { get; set; }

        [Column("Notes")]
        public string? Notes { get; set; }

        [Column("Date_Created")]
        public DateTimeOffset Date_Created { get; set; }

        [Column("Last_Modified")]
        public DateTimeOffset Last_Modified { get; set; }
        [Column("SyncStatus")]
        public SyncStatus SyncStatus { get; set; }

        public SupaStockRefills()
        {
            // Initialize default values
            Date_Created = DateTimeOffset.UtcNow;
            Last_Modified = DateTimeOffset.UtcNow;
            Confirmed_By_Scanner = false;
            Stock_Refilled = false;
            SyncStatus = SyncStatus.Pending;
        }
    }
}