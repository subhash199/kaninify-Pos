using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaVoidedProducts")]
    public class SupaVoidedProducts
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("VoidedProduct_ID")]
        public int VoidedProduct_ID { get; set; }

        [Column("Product_ID")]
        public int Product_ID { get; set; }

        [Column("Voided_Quantity")]
        public int Voided_Quantity { get; set; }

        [Column("Voided_Amount", TypeName = "decimal(18,2)")]
        public decimal Voided_Amount { get; set; }

        [Column("Voided_By_User_ID")]
        public int Voided_By_User_ID { get; set; }

        [Column("Void_Date")]
        public DateTimeOffset Void_Date { get; set; }

        [Column("Additional_Notes")]
        public string? Additional_Notes { get; set; } = string.Empty;

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

        [Column("Daylog_Id")]
        public int? Daylog_Id { get; set; }

        [Column("Shift_Id")]
        public int? Shift_Id { get; set; }
        [Column("SyncStatus")]
        public SyncStatus SyncStatus { get; set; }

        public SupaVoidedProducts()
        {
            // Initialize default values
            Date_Created = DateTimeOffset.UtcNow;
            Last_Modified = DateTimeOffset.UtcNow;
            Void_Date = DateTimeOffset.UtcNow;
            SyncStatus = SyncStatus.Pending;
        }
    }
}