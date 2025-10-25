using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DataHandlerLibrary.Models;

namespace EntityFrameworkDatabaseLibrary.Models
{
    public class SalesItemTransaction
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    

        [ForeignKey("SalesTransaction")]
        public int SaleTransaction_ID { get; set; }
        public virtual SalesTransaction? SalesTransaction { get; set; } // Add navigation property

        [ForeignKey("Product")]
        public int Product_ID { get; set; }
        public virtual Product? Product { get; set; }

        [Required]
        public int Product_QTY { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Product_Amount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Product_Total_Amount { get; set; }
        public SalesItemTransactionType? SalesItemTransactionType { get; set; }
        public int? SalesPayout_ID { get; set; }
        public virtual Payout? SalesPayout { get; set; } // Add navigation property if exists
        public int? Promotion_ID { get; set; }
        public virtual Promotion? Promotion { get; set; } // Add navigation property if exists

        [Column(TypeName = "decimal(18,2")]
        public decimal Promotional_Discount_Amount =>
            Promotion_ID.HasValue && Product_Total_Amount > 0
            ? Product_Total_Amount_Before_Discount - Product_Total_Amount
            : 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Product_Total_Amount_Before_Discount { get; set; } = 0m;
        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount_Percent { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Discount_Amount { get; set; } = 0m;
        public bool? Is_Manual_Weight_Entry { get; set; } // Changed from decimal? to bool?
        public DateTime Date_Created { get; set; } = DateTime.UtcNow;
        public DateTime Last_Modified { get; set; } = DateTime.UtcNow;
        public int? Created_By_Id { get; set; }
        public int? Last_Modified_By_Id { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
        public virtual PosUser? Created_By { get; set; }
        public virtual PosUser? Last_Modified_By { get; set; }
     
    }
}
