using DataHandlerLibrary.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.InteropServices.Marshalling;

namespace EntityFrameworkDatabaseLibrary.Models
{
    public class Shift
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    

        [ForeignKey(nameof(DayLog))]
        public int DayLog_Id { get; set; }
        public virtual DayLog DayLog { get; set; }
        public int PosUser_Id { get; set; }
        public virtual PosUser PosUser { get; set; }


        [Required]
        public DateTime Shift_Start_DateTime { get; set; }

        public DateTime? Shift_End_DateTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Opening_Cash_Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Closing_Cash_Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Expected_Cash_Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? Cash_Variance { get; set; }

        public bool Is_Active { get; set; } = true;

        [MaxLength(500)]
        public string? Shift_Notes { get; set; }

        [MaxLength(500)]
        public string? Closing_Notes { get; set; }

        public DateTime Date_Created { get; set; } = DateTime.UtcNow;
        public DateTime Last_Modified { get; set; } = DateTime.UtcNow;
        public int? Created_By_Id { get; set; }
        public int? Last_Modified_By_Id { get; set; }
        public int? Site_Id { get; set; }
        public int? Till_Id { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
        // Navigation properties
        public virtual PosUser? Created_By { get; set; }
        public virtual PosUser? Last_Modified_By { get; set; }
        public virtual Site? Site { get; set; }
        public virtual Till? Till { get; set; }
        public virtual ICollection<SalesTransaction> SalesTransactions { get; set; } = new List<SalesTransaction>();
    }
}