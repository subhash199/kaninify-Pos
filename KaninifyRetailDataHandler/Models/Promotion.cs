using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using DataHandlerLibrary.Models;

namespace EntityFrameworkDatabaseLibrary.Models
{
    public class Promotion
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
    
        public string Promotion_Name { get; set; }

        [MaxLength(255)]
        public string? Promotion_Description { get; set; }

        //Buy X Get Y Free promotions or Multi-Buy promotions
        public int Buy_Quantity { get; set; } = 1;

        //Buy X Get Y Free promotions
        public int? Free_Quantity { get; set; } = 0;

        //Discount promotions
        public decimal? Discount_Percentage { get; set; } = 0.00m;
        public decimal? Discount_Amount { get; set; } = 0.00m;
        public decimal? Minimum_Spend_Amount { get; set; } = 0.00m;

        public DateTime Start_Date { get; set; }
        public DateTime End_Date { get; set; }
        public PromotionType Promotion_Type { get; set; }
        public bool Is_Active => DateTime.UtcNow >= Start_Date && DateTime.UtcNow <= End_Date && !Is_Deleted;
        public bool Is_Deleted { get; set; } = false;
        public DateTime Created_Date { get; set; } = DateTime.UtcNow;
        public DateTime Last_Modified { get; set; } = DateTime.UtcNow;
        public int? Created_By_Id { get; set; }
        public int? Last_Modified_By_Id { get; set; }
        public int? Site_Id { get; set; }
        public int? Till_Id { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;

        //Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual PosUser? Created_By { get; set; }
        public virtual PosUser? Last_Modified_By { get; set; }
        public virtual Site? Site { get; set; }
        public virtual Till? Till { get; set; }
    }

    public enum PromotionType
    {
        Discount,
        BuyXGetXFree,
        MultiBuy,
        BundleBuy,
    }
}