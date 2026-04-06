using EntityFrameworkDatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models
{
    public class StockTransaction
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public StockTransferType StockTransactionType { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]     
        public decimal TotalAmount { get; set; }
        [Required]
        public int DayLogId { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public int? Shift_Id { get; set; }
        public int? Created_By_Id { get; set; }
        public int? Last_Modified_By_Id { get; set; }
        public int? From_Site_Id { get; set; }
        public int? To_Site_Id { get; set; }
        public int? Till_Id { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
        public virtual Product? Product { get; set; }
        public virtual DayLog? DayLog { get; set; }
        public virtual Shift? Shift { get; set; }
        public virtual PosUser? Created_By { get; set; }
        public virtual PosUser? Last_Modified_By { get; set; }
        public virtual Site? From_Site { get; set; }
        public virtual Site? To_Site { get; set; }
        public virtual Till? Till { get; set; }
    }
}
