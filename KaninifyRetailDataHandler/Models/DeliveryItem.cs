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
    public class DeliveryItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [ForeignKey(nameof(DeliveryInvoiceId))]
        public int DeliveryInvoiceId { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int QuantityDelivered { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal CostPerCase { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }
        public int? SupplierItemId { get; set; }
        public bool ConfirmedByScanner { get; set; } = false;
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;

        // Navigation properties
        public virtual DeliveryInvoice? DeliveryInvoice { get; set; }
        public virtual Product? Product { get; set; }
        public virtual SupplierItem? SupplierItem { get; set; }

    }
}
