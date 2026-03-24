using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaErrorLogs")]
    public class SupaErrorLogs
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("ErrorLog_Id")]
        public int ErrorLog_Id { get; set; }

        [Column("Error_Message")]
        public string Error_Message { get; set; } = string.Empty;

        [Column("Error_Type")]
        public string? Error_Type { get; set; }

        [Column("Source_Method")]
        public string? Source_Method { get; set; }

        [Column("Source_Class")]
        public string? Source_Class { get; set; }

        [Column("Stack_Trace")]
        public string? Stack_Trace { get; set; }

        [Column("Severity_Level")]
        public int? Severity_Level { get; set; }

        [Column("User_Action")]
        public string? User_Action { get; set; }

        [Column("Error_DateTime")]
        public DateTime Error_DateTime { get; set; }

        [Column("Date_Created")]
        public DateTime Date_Created { get; set; }

        [Column("User_Id")]
        public int? User_Id { get; set; }

        [Column("Site_Id")]
        public int? Site_Id { get; set; }

        [Column("Till_Id")]
        public int? Till_Id { get; set; }

        [Column("Application_Version")]
        public string? Application_Version { get; set; }

        [Column("Is_Resolved")]
        public bool Is_Resolved { get; set; } = false;

        [Column("Resolved_DateTime")]
        public DateTime? Resolved_DateTime { get; set; }

        [Column("Resolved_By_Id")]
        public int? Resolved_By_Id { get; set; }

        [Column("Resolution_Notes")]
        public string? Resolution_Notes { get; set; }

        [Column("SyncStatus")]
        public SyncStatus SyncStatus { get; set; }

        public SupaErrorLogs()
        {
           
            Date_Created = DateTime.UtcNow;
            Error_DateTime = DateTime.UtcNow;
            Is_Resolved = false;
            SyncStatus = SyncStatus.Pending;
        }
    }
}