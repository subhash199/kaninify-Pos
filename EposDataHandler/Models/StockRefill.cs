using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataHandlerLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkDatabaseLibrary.Models
{
    public class StockRefill
    {
        public long? Supa_Id { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    

        [ForeignKey("SalesItemTransaction")]
        [Required]
        public int SaleTransaction_Item_ID { get; set; }
        public virtual SalesItemTransaction SalesItemTransaction { get; set; }
        public int? Refilled_By { get; set; }

        [ForeignKey("Refilled_By")]
        public virtual PosUser? Refilled_By_User { get; set; }
        public DateTime? Refilled_Date { get; set; }
        public bool Confirmed_By_Scanner { get; set; } = false;

        // Quantity tracking properties
        [Required]
        public int Refill_Quantity { get; set; }

        [Required]
        public int Quantity_Refilled { get; set; }
        public bool Stock_Refilled { get; set; } = false;

        // Shift and DayLog tracking
        [ForeignKey("Shift")]
        public int? Shift_ID { get; set; }
        public virtual Shift? Shift { get; set; }

        [ForeignKey("DayLog")]
        public int? DayLog_ID { get; set; }
        public virtual DayLog? DayLog { get; set; }

        // Audit trail properties
        [Required]
        [ForeignKey("Created_By_User")]
        public int Created_By_ID { get; set; }
        public virtual PosUser Created_By_User { get; set; }

        [ForeignKey("Last_Modified_By_User")]
        public int? Last_Modified_By_ID { get; set; }
        public virtual PosUser? Last_Modified_By_User { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
        public DateTime Date_Created { get; set; } = DateTime.UtcNow;

        public DateTime Last_Modified { get; set; } = DateTime.UtcNow;

        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;

        // Computed properties for business logic
        [NotMapped]
        public int Remaining_Quantity => Refill_Quantity - Quantity_Refilled;

        [NotMapped]
        public bool Is_Fully_Refilled => Quantity_Refilled >= Refill_Quantity;

        [NotMapped]
        public decimal Refill_Percentage => Refill_Quantity > 0 ? ((decimal)Quantity_Refilled / Refill_Quantity) * 100 : 0;
    }
}