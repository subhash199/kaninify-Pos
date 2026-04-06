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
    public class UserSiteAccess
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }    
    


        public int User_Id { get; set; }
        public virtual PosUser User { get; set; } // Navigation property to PosUser

        public int Site_Id { get; set; }
        public virtual Site? Site { get; set; }

        // Access properties
        public bool Is_Active { get; set; } = true;
        public bool Is_Deleted { get; set; } = false; // Soft delete flag
        public DateTime Date_Granted { get; set; } = DateTime.UtcNow;
        public DateTime? Date_Revoked { get; set; }
        
        // Audit fields
        public DateTime Date_Created { get; set; } = DateTime.UtcNow;
        public DateTime Last_Modified { get; set; } = DateTime.UtcNow;

        public int? Created_By_Id { get; set; }
        public int? Last_Modified_By_Id { get; set; }
        public int? Till_Id { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
        public virtual PosUser? Created_By { get; set; }
        public virtual PosUser? Last_Modified_By { get; set; }  
        public virtual Till? Till { get; set; }

    }
}