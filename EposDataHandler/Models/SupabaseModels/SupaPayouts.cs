using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaPayouts")]
    public class SupaPayouts
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("Payout_Id")]
        public int Payout_Id { get; set; }

        [Column("Payout_Description")]
        public string Payout_Description { get; set; } = string.Empty;

        [Column("Is_Active")]
        public bool Is_Active { get; set; }

        [Column("Is_Deleted")]
        public bool Is_Deleted { get; set; }

        [Column("Created_Date")]
        public DateTime Created_Date { get; set; }

        [Column("Last_Modified")]
        public DateTime Last_Modified { get; set; }

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

        public SupaPayouts()
        {
            Created_Date = DateTime.UtcNow;
            Last_Modified = DateTime.UtcNow;
            Is_Active = true;
            Is_Deleted = false;
            SyncStatus = SyncStatus.Pending;
        }
    }
}