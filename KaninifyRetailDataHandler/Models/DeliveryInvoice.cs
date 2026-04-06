using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EntityFrameworkDatabaseLibrary.Models;

namespace DataHandlerLibrary.Models
{
    [Index(nameof(SupplierId), nameof(InvoiceId), IsUnique = true)]
    public class DeliveryInvoice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        [Required]
        public DateTime DeliveryDate { get; set; } = DateTime.UtcNow;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal InvoiceTotal { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue)]
        public decimal InvoiceVatTotal { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int TotalItemsDelivered { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
        public int? DayLogId { get; set; }
        public int? Shift_Id { get; set; }
        public int? Created_By_Id { get; set; }
        public int? Last_Modified_By_Id { get; set; }
        public int? Till_Id { get; set; }

        [ForeignKey(nameof(DayLogId))]
        public virtual DayLog? DayLog { get; set; }

        [ForeignKey(nameof(Shift_Id))]
        public virtual Shift? Shift { get; set; }

        public virtual PosUser? Created_By { get; set; }
        public virtual PosUser? Last_Modified_By { get; set; }

        [ForeignKey(nameof(Till_Id))]
        public virtual Till? Till { get; set; }

        // Add: link to Site
        public int? Site_Id { get; set; }
        [ForeignKey(nameof(Site_Id))]
        public virtual Site? Site { get; set; }

        // Add: navigation to delivery items
        public virtual ICollection<DeliveryItem> Items { get; set; } = new List<DeliveryItem>();

        public int? SupplierId { get; set; }
        [ForeignKey(nameof(SupplierId))]
        public virtual Supplier? Supplier { get; set; }
    }
}
