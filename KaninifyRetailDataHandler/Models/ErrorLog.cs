using EntityFrameworkDatabaseLibrary.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataHandlerLibrary.Models
{
    public class ErrorLog
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    

        [Required]
        [StringLength(500)]
        public string Error_Message { get; set; } = string.Empty;
        
        [StringLength(100)]
        public string? Error_Type { get; set; }
        
        [StringLength(200)]
        public string? Source_Method { get; set; }
        
        [StringLength(200)]
        public string? Source_Class { get; set; }
        
        public string? Stack_Trace { get; set; }
        
        public ErrorLogSeverity? Severity_Level { get; set; }
        
        [StringLength(100)]
        public string? User_Action { get; set; } // What the user was trying to do
        
        public DateTime Error_DateTime { get; set; }
        
        public DateTime Date_Created { get; set; }
        
        public int? User_Id { get; set; }
        
        public int? Site_Id { get; set; }
        
        public int? Till_Id { get; set; }
        
        [StringLength(50)]
        public string? Application_Version { get; set; }
        
        public bool Is_Resolved { get; set; } = false;
        
        public DateTime? Resolved_DateTime { get; set; }
        
        public int? Resolved_By_Id { get; set; }
        
        [StringLength(1000)]
        public string? Resolution_Notes { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
        // Navigation properties
        public virtual PosUser? User { get; set; }
        
        public virtual Site? Site { get; set; }
        
        public virtual Till? Till { get; set; }
        
        public virtual PosUser? Resolved_By { get; set; }
    }
}