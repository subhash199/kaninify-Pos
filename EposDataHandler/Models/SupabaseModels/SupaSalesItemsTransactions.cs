using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaSalesItemsTransactions")]
    public class SupaSalesItemsTransactions
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("SaleTransaction_Item_ID")]
        public int SaleTransaction_Item_ID { get; set; }

        [Column("SaleTransaction_ID")]
        public int SaleTransaction_ID { get; set; }

        [Column("Product_ID")]
        public int Product_ID { get; set; }

        [Column("Product_QTY")]
        public int Product_QTY { get; set; }

        [Column("Product_Amount", TypeName = "decimal(18,2)")]
        public decimal Product_Amount { get; set; }

        [Column("Product_Total_Amount", TypeName = "decimal(18,2)")]
        public decimal Product_Total_Amount { get; set; }

        [Column("SalesPayout_ID")]
        public int? SalesPayout_ID { get; set; }

        [Column("Promotion_ID")]
        public int? Promotion_ID { get; set; }

        [Column("Discount_Percent", TypeName = "decimal(18,2)")]
        public decimal Discount_Percent { get; set; }

        [Column("Product_Total_Amount_Before_Discount", TypeName = "decimal(18,2)")]
        public decimal Product_Total_Amount_Before_Discount { get; set; }

        [Column("Discount_Amount", TypeName = "decimal(18,2)")]
        public decimal Discount_Amount { get; set; }

        [Column("Is_Manual_Weight_Entry")]
        public bool? Is_Manual_Weight_Entry { get; set; }

        [Column("Date_Created")]
        public DateTimeOffset Date_Created { get; set; }

        [Column("Last_Modified")]
        public DateTimeOffset Last_Modified { get; set; }

        [Column("SalesItemTransactionType")]
        public SalesItemTransactionType SalesItemTransactionType { get; set; }

        [Column("Created_By_Id")]
        public int? Created_By_Id { get; set; }

        [Column("Last_Modified_By_Id")]
        public int? Last_Modified_By_Id { get; set; }

        [Column("SyncStatus")]
        public SyncStatus SyncStatus { get; set; }

        public SupaSalesItemsTransactions()
        {
            // Initialize default values
            Date_Created = DateTimeOffset.UtcNow;
            Last_Modified = DateTimeOffset.UtcNow;
            SyncStatus = SyncStatus.Pending;
        }
    }
}