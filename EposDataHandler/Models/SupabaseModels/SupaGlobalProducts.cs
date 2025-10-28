using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaProducts")]
    public class SupaGlobalProducts
    {
        [Key]
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; }

        [Column("ProductBarcode")]
        public string ProductBarcode { get; set; }

        [Column("ProductName")]
        public string ProductName { get; set; }

        [Column("ProductUnitsPerCase")]
        public long? ProductUnitsPerCase { get; set; }

        [Column("ProductCostPerCase")]
        public double? ProductCostPerCase { get; set; }

        [Column("ProductVatValue")]
        public double? ProductVatValue { get; set; }

        [Column("ProductDepartmentName")]
        public string? ProductDepartmentName { get; set; }

        [Column("ProductRetailPrice")]
        public double? ProductRetailPrice { get; set; }

        [Column("ProductMeasurement")]
        public int? ProductMeasurement { get; set; }

        [Column("ProductMeasurementType")]
        public string? ProductMeasurementType { get; set; }

        [Column("Active")]
        public bool? Active { get; set; }

        [Column("Deleted")]
        public bool? Deleted { get; set; }
         [Column("LastModified_At")]
        public DateTimeOffset LastModified_At { get; set; } = DateTimeOffset.UtcNow;

        public SupaGlobalProducts()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}