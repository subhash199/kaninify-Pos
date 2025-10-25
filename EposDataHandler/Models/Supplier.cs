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
    public class Supplier
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    

        public virtual ICollection<SupplierItem> SupplierItems { get; set; }
        public string Supplier_Name { get; set; }
        public string? Supplier_Description { get; set; }
        public string? Supplier_Address { get; set; }
        public string? Supplier_Phone { get; set; }
        public string? Supplier_Mobile { get; set; }
        public string? Supplier_Email { get; set; }
        public string? Supplier_Website { get; set; }
        public decimal? Supplier_Credit_Limit { get; set; }
        public bool Is_Activated { get; set; }
        public bool Is_Deleted { get; set; }
        public DateTime Date_Created { get; set; }
        public DateTime Last_Modified { get; set; }
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
