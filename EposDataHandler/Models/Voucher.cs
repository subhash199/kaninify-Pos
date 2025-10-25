using EntityFrameworkDatabaseLibrary.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models
{
    public class Voucher
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Voucher_Code { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Voucher_Description { get; set; }

        // Value of voucher; percentage if Is_Percentage=true, fixed amount otherwise
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal Voucher_Value { get; set; }

        public bool Is_Percentage { get; set; } = false;

        // Optional usage constraints
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal? Min_Basket_Total { get; set; }

        public DateTime? Valid_From { get; set; }
        public DateTime? Valid_To { get; set; }

        // Lifecycle and audit
        public bool Is_Active { get; set; } = true;
        public bool Is_Deleted { get; set; } = false;
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

        // Navigation: exclusions
        public virtual ICollection<VoucherDepartmentExclusion> ExcludedDepartments { get; set; } = new List<VoucherDepartmentExclusion>();
        public virtual ICollection<VoucherProductExclusion> ExcludedProducts { get; set; } = new List<VoucherProductExclusion>();
    }
}
