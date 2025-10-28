using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaDrawerLogs")]
    public class SupaDrawerLogs
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("DrawerLogId")]
        public int DrawerLogId { get; set; }

        [Column("OpenedById")]
        public int OpenedById { get; set; }

        [Column("DrawerOpenDateTime")]
        public DateTimeOffset DrawerOpenDateTime { get; set; }

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

        [Column("DayLog_Id")]
        public int? DayLog_Id { get; set; }

        [Column("Shift_Id")]
        public int? Shift_Id { get; set; }

        [Column("DrawerLogType")]
        public DrawerLogType DrawerLogType { get; set; }

        [Column("SyncStatus")]
        public SyncStatus SyncStatus { get; set; }

        public SupaDrawerLogs()
        {
            SyncStatus = SyncStatus.Pending;
        }
    }
}