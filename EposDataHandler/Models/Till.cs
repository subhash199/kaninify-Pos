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
    public class Till
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    

        [Required]
        [MaxLength(50)]
        public string Till_Name { get; set; }

        [MaxLength(15)]
        public string? Till_IP_Address { get; set; }

        [MaxLength(10)]
        public int? Till_Port_Number { get; set; }
        public string Till_Password { get; set; }
        public bool Is_Primary { get; set; } = false; // Indicates if this is the primary till for the site
        public bool Is_Active { get; set; } = true; // Use bool for active status
        public bool Is_Deleted { get; set; } = false;

        public DateTime Date_Created { get; set; } = DateTime.UtcNow;
        public DateTime Last_Modified { get; set; } = DateTime.UtcNow;
        public int? Created_By_Id { get; set; }
        public int? Last_Modified_By_Id { get; set; }
        public int? Site_Id { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;


        // Navigation properties
        public virtual PosUser? Created_By { get; set; }
        public virtual PosUser? Last_Modified_By { get; set; }
        public virtual Site? Site { get; set; }
    }
}
