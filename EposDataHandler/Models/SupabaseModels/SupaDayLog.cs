using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaDayLogs")]
    public class SupaDayLogs
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("DayLog_Id")]
        public int DayLog_Id { get; set; }

        [Column("DayLog_Start_DateTime")]
        public DateTimeOffset DayLog_Start_DateTime { get; set; } = DateTime.UtcNow;

        [Column("DayLog_End_DateTime")]
        public DateTimeOffset? DayLog_End_DateTime { get; set; }

        [Column("Opening_Cash_Amount")]
        public decimal Opening_Cash_Amount { get; set; }

        [Column("Closing_Cash_Amount")]
        public decimal Closing_Cash_Amount { get; set; }

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

        [Column("Cash_Variance")]
        public decimal Cash_Variance { get; set; }

        [Column("SyncStatus")]
        public SyncStatus? SyncStatus { get; set; }

        public SupaDayLogs()
        {
            Date_Created = DateTimeOffset.UtcNow;
            Last_Modified = DateTimeOffset.UtcNow;
            SyncStatus = Models.SyncStatus.Pending;
        }
    }
}