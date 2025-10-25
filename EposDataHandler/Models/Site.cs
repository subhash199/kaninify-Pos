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
    public class Site
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    

        [Required]
        [MaxLength(100)]
        public string Site_BusinessName { get; set; }

        [Required]
        [MaxLength(200)]
        public string Site_AddressLine1 { get; set; }

        [Required]
        [MaxLength(50)]
        public string Site_City { get; set; }
        public string? Site_AddressLine2 { get; set; }
        public string? Site_County { get; set; }
        public string Site_Country { get; set; }
        public string Site_Postcode { get; set; }
        public string? Site_ContactNumber { get; set; }
        public string? Site_Email { get; set; }
        public string? Site_VatNumber { get; set; }
        public bool Is_Primary { get; set; } = false; // Indicates if this is the primary site for the business
        public DateTime Date_Created { get; set; } = DateTime.UtcNow;
        public DateTime Last_Modified { get; set; } = DateTime.UtcNow;
        public int? Created_By_Id { get; set; }
        public int? Last_Modified_By_Id { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
        public bool Is_Active { get; set; } = true; // Indicates if the site is currently active
        public bool Is_Deleted { get; set; } = false; // Soft delete flag


        // Navigation properties
        public virtual PosUser? Created_By { get; set; } // User who created the site
        public virtual PosUser? Last_Modified_By { get; set; } // User who last modified the site
        public virtual ICollection<UserSiteAccess> UserAccesses { get; set; } = new List<UserSiteAccess>();
        public virtual ICollection<PosUser> PrimaryUsers { get; set; } = new List<PosUser>();
        public virtual ICollection<Till> Tills { get; set; } = new List<Till>();
    }
}
