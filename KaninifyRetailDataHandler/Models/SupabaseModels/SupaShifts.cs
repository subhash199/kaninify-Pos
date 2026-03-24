using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaShifts")]
    public class SupaShifts
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("Shift_Id")]
        public int Shift_Id { get; set; }

        [Column("DayLog_Id")]
        public int DayLog_Id { get; set; }

        [Column("PosUser_Id")]
        public int PosUser_Id { get; set; }

        [Column("Shift_Start_DateTime")]
        public DateTime Shift_Start_DateTime { get; set; }

        [Column("Shift_End_DateTime")]
        public DateTime? Shift_End_DateTime { get; set; }

        [Column("Opening_Cash_Amount", TypeName = "decimal(18,2)")]
        public decimal? Opening_Cash_Amount { get; set; }

        [Column("Closing_Cash_Amount", TypeName = "decimal(18,2)")]
        public decimal? Closing_Cash_Amount { get; set; }

        [Column("Expected_Cash_Amount", TypeName = "decimal(18,2)")]
        public decimal? Expected_Cash_Amount { get; set; }

        [Column("Cash_Variance", TypeName = "decimal(18,2)")]
        public decimal? Cash_Variance { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; }

        [Column("Shift_Notes")]
        public string? Shift_Notes { get; set; }

        [Column("Closing_Notes")]
        public string? Closing_Notes { get; set; }

        [Column("Date_Created")]
        public DateTime Date_Created { get; set; }

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

        public SupaShifts()
        {
            // Initialize default values
            Date_Created = DateTime.UtcNow;
            Last_Modified = DateTime.UtcNow;
            Is_Active = true;
            SyncStatus = SyncStatus.Pending;
        }
    }
}