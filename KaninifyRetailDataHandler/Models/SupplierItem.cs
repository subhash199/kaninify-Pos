using DataHandlerLibrary.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkDatabaseLibrary.Models
{
    public class SupplierItem
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    


        // Navigation property to Supplier
        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; }
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }
        public string Supplier_Product_Code { get; set; }
        public string Product_OuterCaseBarcode { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost_Per_Case { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost_Per_Unit { get; set; }
        public int Unit_Per_Case { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Profit_On_Return { get; set; }

        public bool Is_Active { get; set; }
        public bool Is_Deleted { get; set; } = false;
        public DateTime Date_Created { get; set; }
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
    }
}
