using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaUserSiteAccesses")]
    public class SupaUserSiteAccesses
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("UserSiteAccess_ID")]
        public int UserSiteAccess_ID { get; set; }

        [Column("User_Id")]
        public int User_Id { get; set; }

        [Column("Site_Id")]
        public int Site_Id { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; }

        [Column("Is_Deleted")]
        public bool Is_Deleted { get; set; }

        [Column("Date_Granted")]
        public DateTime Date_Granted { get; set; }

        [Column("Date_Revoked")]
        public DateTime? Date_Revoked { get; set; }

        [Column("Date_Created")]
        public DateTime Date_Created { get; set; }

        [Column("Last_Modified")]
        public DateTime Last_Modified { get; set; }

        [Column("Created_By_Id")]
        public int? Created_By_Id { get; set; }

        [Column("Last_Modified_By_Id")]
        public int? Last_Modified_By_Id { get; set; }

        [Column("Till_Id")]
        public int? Till_Id { get; set; }

        [Column("SyncStatus")]
        public SyncStatus SyncStatus { get; set; }

        public SupaUserSiteAccesses()
        {
            // Initialize default values
            Date_Created = DateTime.UtcNow;
            Last_Modified = DateTime.UtcNow;
            Date_Granted = DateTime.UtcNow;
            Is_Active = true;
            Is_Deleted = false;
            SyncStatus = SyncStatus.Pending;
        }
    }
}