using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaRetailerProducts")]
    public class SupaRetailerProducts
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }
        public int Product_Id { get; set; }

        [Required]
        [Column("ProductBarcode")]
        public string ProductBarcode { get; set; }

        [Required]
        [Column("ProductName")]
        public string ProductName { get; set; }
        [Required]
        [Column("DepartmentName")]
        public string DepartmentName { get; set; }
        [Column("ProductMeasurement")]
        public int? ProductMeasurement { get; set; }
        [Column("ProductMeasurementType")]
        public string? ProductMeasurementType { get; set; }

        [Column("VatValue", TypeName = "decimal(18,2)")]
        public decimal VatValue { get; set; }

        [Column("ShelfQuantity")]
        public int ShelfQuantity { get; set; }

        [Column("StockroomQuantity")]
        public int StockroomQuantity { get; set; }

        [Column("Product_Cost", TypeName = "decimal(18,2)")]
        public decimal Product_Cost { get; set; }

        [Column("Product_Selling_Price", TypeName = "decimal(18,2)")]
        public decimal Product_Selling_Price { get; set; }

        [Column("Profit_On_Return_Percentage", TypeName = "decimal(18,2)")]
        public decimal Profit_On_Return_Percentage { get; set; }

        [Column("Promotion_Id")]
        public int? Promotion_Id { get; set; }

        [Column("Product_Unit_Per_Case")]
        public int Product_Unit_Per_Case { get; set; }

        [Column("Product_Cost_Per_Case")]
        public double Product_Cost_Per_Case { get; set; }

        [Column("Expiry_Date")]
        public DateTimeOffset Expiry_Date { get; set; } = DateTimeOffset.UtcNow.AddMonths(1);

        [Column("Is_Activated")]
        public bool Is_Activated { get; set; }

        [Column("Is_Deleted")]
        public bool Is_Deleted { get; set; }

        [Column("Priced_Changed_On")]
        public DateTimeOffset Priced_Changed_On { get; set; } = DateTime.UtcNow;

        [Column("Is_Price_Changed")]
        public bool Is_Price_Changed { get; set; } = false;

        [Column("Date_Created")]
        public DateTimeOffset Date_Created { get; set; }

        [Column("Last_Modified")]
        public DateTimeOffset Last_Modified { get; set; }

        [Column("Allow_Discount")]
        public bool Allow_Discount { get; set; } = false;

        [Column("Created_By_Id")]
        public int? Created_By_Id { get; set; }

        [Column("Last_Modified_By_Id")]
        public int? Last_Modified_By_Id { get; set; }

        [Column("Site_Id")]
        public int? Site_Id { get; set; }

        [Column("Till_Id")]
        public int? Till_Id { get; set; }

        [Key]

        [Column("Product_Global_Id")]
        public Guid Product_Global_Id { get; set; }

        [Column("SyncStatus")]
        public SyncStatus SyncStatus { get; set; }

        public SupaRetailerProducts()
        {
            Date_Created = DateTimeOffset.UtcNow;
            Last_Modified = DateTimeOffset.UtcNow;
            Is_Activated = true;
            Is_Deleted = false;
            Is_Price_Changed = false;
            Allow_Discount = true;
            SyncStatus = SyncStatus.Pending;
        }
    }
}