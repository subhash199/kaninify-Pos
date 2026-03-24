using DataHandlerLibrary.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkDatabaseLibrary.Models
{
    public class VoidedProduct
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(Product))]
        public int Product_ID { get; set; }
        public virtual Product Product { get; set; }

        [Required]
        public int Voided_Quantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Voided_Amount { get; set; }

        [ForeignKey(nameof(VoidedByUser))]
        public int Voided_By_User_ID { get; set; }
        public virtual PosUser VoidedByUser { get; set; }
        public DateTime Void_Date { get; set; } = DateTime.UtcNow;

        [MaxLength(500)]
        public string? Additional_Notes { get; set; }
        public DateTime Date_Created { get; set; } = DateTime.UtcNow;
        public DateTime Last_Modified { get; set; } = DateTime.UtcNow;
        public int? Created_By_Id { get; set; }
        public int? Last_Modified_By_Id { get; set; }
        public int? Site_Id { get; set; }
        public int? Till_Id { get; set; }
        [ForeignKey(nameof(Shift))]
        public int? Shift_Id { get; set; }
        [ForeignKey(nameof(Daylog))]
        public int? Daylog_Id { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
        public virtual PosUser? Created_By { get; set; }
        public virtual PosUser? Last_Modified_By { get; set; }
        public virtual Shift? Shift { get; set; }
        public virtual DayLog? Daylog { get; set; }
        public virtual Site? Site { get; set; }
        public virtual Till? Till { get; set; }
    }
}