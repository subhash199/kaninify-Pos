using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;
using EntityFrameworkDatabaseLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaPromotions")]
    public class SupaPromotions
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("Promotion_ID")]
        public int Promotion_ID { get; set; }

        [Column("Promotion_Name")]
        public string Promotion_Name { get; set; } = string.Empty;

        [Column("Promotion_Description")]
        public string? Promotion_Description { get; set; }

        [Column("Buy_Quantity")]
        public int Buy_Quantity { get; set; }

        [Column("Free_Quantity")]
        public int? Free_Quantity { get; set; }

        [Column("Discount_Percentage", TypeName = "decimal(18,2)")]
        public decimal? Discount_Percentage { get; set; }

        [Column("Discount_Amount", TypeName = "decimal(18,2)")]
        public decimal? Discount_Amount { get; set; }

        [Column("Minimum_Spend_Amount", TypeName = "decimal(18,2)")]
        public decimal? Minimum_Spend_Amount { get; set; }

        [Column("Start_Date")]
        public DateTimeOffset Start_Date { get; set; } = DateTime.UtcNow;

        [Column("End_Date")]
        public DateTimeOffset End_Date { get; set; } = DateTime.UtcNow;

        [Column("Promotion_Type")]
        public PromotionType Promotion_Type { get; set; }

        [Column("Is_Deleted")]
        public bool Is_Deleted { get; set; }

        [Column("Created_Date")]
        public DateTimeOffset Created_Date { get; set; }

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

        [Column("SyncStatus")]
        public SyncStatus SyncStatus { get; set; }

        public SupaPromotions()
        {
            // Initialize default values
            Created_Date = DateTimeOffset.UtcNow;
            Last_Modified = DateTimeOffset.UtcNow;
            Is_Deleted = false;
            SyncStatus = SyncStatus.Pending;
        }
    }
}