using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaSupplierItems")]
    public class SupaSupplierItems
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("SupplierItemId")]
        public int SupplierItemId { get; set; }

        [Column("SupplierId")]
        public int SupplierId { get; set; }

        [Column("ProductId")]
        public int ProductId { get; set; }

        [Column("Supplier_Product_Code")]
        public string Supplier_Product_Code { get; set; }

        [Column("Product_OuterCaseBarcode")]
        public string Product_OuterCaseBarcode { get; set; }

        [Column("Cost_Per_Case", TypeName = "decimal(18,2)")]
        public decimal Cost_Per_Case { get; set; }

        [Column("Cost_Per_Unit", TypeName = "decimal(18,2)")]
        public decimal Cost_Per_Unit { get; set; }

        [Column("Unit_Per_Case")]
        public int Unit_Per_Case { get; set; }

        [Column("Profit_On_Return", TypeName = "decimal(18,2)")]
        public decimal Profit_On_Return { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; }

        [Column("Is_Deleted")]
        public bool Is_Deleted { get; set; }

        [Column("Date_Created")]
        public DateTimeOffset Date_Created { get; set; }

        [Column("Last_Modified")]
        public DateTimeOffset Last_Modified { get; set; }

        [Column("Created_By_Id")]
        public int? Created_By_Id { get; set; }

        [Column("Last_Modified_By_Id")]
        public int? Last_Modified_By_Id { get; set; }

        [Column("Site_Id")]
        public int? Site_Id { get; set; }

        [Column("Till_Id")]
        public int? Till_Id { get; set; }

        [Column("SyncStatus")]
        public SyncStatus SyncStatus { get; set; }

        public SupaSupplierItems()
        {
            // Initialize default values
            Date_Created = DateTimeOffset.UtcNow;
            Last_Modified = DateTimeOffset.UtcNow;
            Is_Active = true;
            Is_Deleted = false;
            SyncStatus = SyncStatus.Pending;
        }
    }
}