using DataHandlerLibrary.Models;
using Microsoft.Identity.Client;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityFrameworkDatabaseLibrary.Models
{
    public class PosUser
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    
        [Required]
        [MaxLength(50)]
        public string First_Name { get; set; }

        [Required]
        [MaxLength(50)]
        public string Last_Name { get; set; }

        [Required]
        public int Passcode { get; set; }
        public PosUserType User_Type { get; set; } = PosUserType.Staff; // Default to Staff

        // Primary site (for staff locked to one site)
        [ForeignKey(nameof(PrimarySite))]
        public int? Primary_Site_Id { get; set; }
        public virtual Site? PrimarySite { get; set; }
        public bool Allowed_Void_Line { get; set; }
        public bool Allowed_Void_Sale { get; set; }
        public bool Allowed_No_Sale { get; set; }
        public bool Allowed_Returns { get; set; }
        public bool Allowed_Payout { get; set; }
        public bool Allowed_Refund { get; set; }
        public bool Allowed_Change_Price { get; set; }
        public bool Allowed_Discount { get; set; }
        public bool Allowed_Override_Price { get; set; }
        public bool Allowed_Manage_Users { get; set; }
        public bool Allowed_Manage_Sites { get; set; }
        public bool Allowed_Manage_Tills { get; set; }
        public bool Allowed_Manage_Products { get; set; }
        public bool Allowed_Manage_Suppliers { get; set; }
        public bool Allowed_Manage_StockTransfer { get; set; }
        public bool Allowed_Manage_Vat { get; set; }
        public bool Allowed_Manage_Departments { get; set; }
        public bool Allowed_Manage_Orders { get; set; }
        public bool Allowed_Manage_Reports { get; set; }
        public bool Allowed_Manage_Settings { get; set; }
        public bool Allowed_Manage_Tax_Rates { get; set; }
        public bool Allowed_Manage_Promotions { get; set; }
        public bool Allowed_Manage_VoidedProducts { get; set; }
        public bool Allowed_Day_End { get; set; }

        // Use bool for activation and deletion flags
        public bool Is_Activated { get; set; }
        public bool Is_Deleted { get; set; }
        public DateTime Date_Created { get; set; } = DateTime.UtcNow;
        public DateTime Last_Modified { get; set; } = DateTime.UtcNow;
        public int? Created_By_Id { get; set; }
        public int? Last_Modified_By_Id { get; set; }
        public int? Site_Id { get; set; }
        public int? Till_Id { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;

        //Navigation properties
        public virtual PosUser? Created_By { get; set; }
        public virtual PosUser? Last_Modified_By { get; set; }
        public virtual Site? Site { get; set; }
        public virtual Till? Till { get; set; }
        public virtual ICollection<Shift> Shifts { get; set; } = new List<Shift>();
        public virtual ICollection<UserSiteAccess> SiteAccesses { get; set; } = new List<UserSiteAccess>();
    }
}
