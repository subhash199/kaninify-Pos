using EntityFrameworkDatabaseLibrary.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models
{
    public class DayLog
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
    
        public DateTime DayLog_Start_DateTime { get; set; }
        public DateTime? DayLog_End_DateTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Opening_Cash_Amount { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Closing_Cash_Amount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Cash_Variance { get; set; }
        public DateTime Date_Created { get; set; }
        public DateTime Last_Modified { get; set; }
        public int? Created_By_Id { get; set; }
        public int? Last_Modified_By_Id { get; set; }
        public int? Site_Id { get; set; }
        public int? Till_Id { get; set; }

        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
        
        public virtual PosUser? Created_By { get; set; }
        public virtual PosUser? Last_Modified_By { get; set; }
        public virtual Site? Site { get; set; }
        public virtual Till? Till { get; set; }


    }
}
