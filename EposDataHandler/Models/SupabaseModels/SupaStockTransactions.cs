using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaStockTransactions")]
    public class SupaStockTransactions
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("StockTransactionId")]
        public int StockTransactionId { get; set; }

        [Column("StockTransactionType")]
        public StockTransferType StockTransactionType { get; set; }

        [Column("ProductId")]
        public int ProductId { get; set; }

        [Column("Quantity")]
        public int Quantity { get; set; }

        [Column("TotalAmount", TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [Column("DayLogId")]
        public int DayLogId { get; set; }

        [Column("TransactionDate")]
        public DateTimeOffset TransactionDate { get; set; }

        [Column("DateCreated")]
        public DateTimeOffset DateCreated { get; set; }

        [Column("LastModified")]
        public DateTimeOffset LastModified { get; set; }

        [Column("Shift_Id")]
        public int? Shift_Id { get; set; }

        [Column("Created_By_Id")]
        public int? Created_By_Id { get; set; }

        [Column("Last_Modified_By_Id")]
        public int? Last_Modified_By_Id { get; set; }

        [Column("From_Site_Id")]
        public int? From_Site_Id { get; set; }

        [Column("To_Site_Id")]
        public int? To_Site_Id { get; set; }

        [Column("Till_Id")]
        public int? Till_Id { get; set; }

        [Column("SyncStatus")]
        public SyncStatus SyncStatus { get; set; }

        public SupaStockTransactions()
        {
            // Initialize default values
            DateCreated = DateTimeOffset.UtcNow;
            LastModified = DateTimeOffset.UtcNow;
            TransactionDate = DateTimeOffset.UtcNow;
            SyncStatus = SyncStatus.Pending;
        }
    }
}