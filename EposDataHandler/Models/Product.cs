using DataHandlerLibrary.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkDatabaseLibrary.Models
{
    public class Product
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public Guid? GlobalId { get; set; }
        [Required]
        [MaxLength(100)]
        public string Product_Name { get; set; }

        [MaxLength(255)]
        public string? Product_Description { get; set; }

        [Required]
        public string Product_Barcode { get; set; }
        public string? Product_Case_Barcode { get; set; }
        public int ShelfQuantity { get; set; }
        public int StockroomQuantity { get; set; }
        public int ProductTotalQuantity => ShelfQuantity + StockroomQuantity;
        [ForeignKey(nameof(Department))]
        public int Department_ID { get; set; }
        public virtual Department? Department { get; set; }
        [ForeignKey(nameof(VAT))]
        public int VAT_ID { get; set; }
        public virtual Vat? VAT { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Product_Cost { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Product_Selling_Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Profit_On_Return_Percentage { get; set; }
        public int? Product_Size { get; set; }
        [MaxLength(20)]
        public string? Product_Measurement { get; set; }

        public int? Promotion_Id { get; set; }
        public virtual Promotion? Promotion { get; set; }

        [MaxLength(100)]
        public string? Brand_Name { get; set; }
        public int Product_Min_Order { get; set; }
        public int Product_Low_Stock_Alert_QTY { get; set; }
        public int Product_Min_Stock_Level { get; set; }
        public int Product_Unit_Per_Case { get; set; }
        public decimal Product_Cost_Per_Case { get; set; }
        public DateTime Expiry_Date { get; set; }
        public bool Is_Activated { get; set; }
        public bool Is_Deleted { get; set; }
        public DateTime Priced_Changed_On { get; set; }
        public bool Is_Price_Changed { get; set; }
        public DateTime Date_Created { get; set; } = DateTime.UtcNow;
        public DateTime Last_Modified { get; set; } = DateTime.UtcNow;
        public bool Allow_Discount { get; set; }
        public int? Created_By_Id { get; set; }
        public int? Last_Modified_By_Id { get; set; }
        public int? Site_Id { get; set; }
        public int? Till_Id { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
        public virtual PosUser? Created_By { get; set; }
        public virtual PosUser? Last_Modified_By { get; set; }
        public virtual Site? Site { get; set; }
        public virtual Till? Till { get; set; }
        public virtual ICollection<SupplierItem>? SupplierItems { get; set; } = new List<SupplierItem>();
    }
}
