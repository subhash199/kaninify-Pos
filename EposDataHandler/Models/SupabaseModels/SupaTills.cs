using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaTills")]
    public class SupaTills
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("Till_Id")]
        public int Till_Id { get; set; }

        [Column("Till_Name")]
        public string Till_Name { get; set; }

        [Column("Till_IP_Address")]
        public string? Till_IP_Address { get; set; }

        [Column("Till_Port_Number")]
        public int? Till_Port_Number { get; set; }

        [Column("Till_Password")]
        public string Till_Password { get; set; }

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

        [Column("Is_Primary")]
        public bool Is_Primary { get; set; }
        [Column("SyncStatus")]
        public SyncStatus SyncStatus { get; set; }

        public SupaTills()
        {
            // Initialize default values
            Date_Created = DateTimeOffset.UtcNow;
            Last_Modified = DateTimeOffset.UtcNow;
            Is_Active = true;
            Is_Deleted = false;
            Is_Primary = false;
            SyncStatus = SyncStatus.Pending;
        }
    }
}