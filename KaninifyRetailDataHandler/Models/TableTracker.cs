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
    public class TableTracker
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public string RecordName { get; set; }
        public string OldRecord { get; set; }
        public string NewRecord { get; set; }
        public Operation Operation { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public int? CreatedBy { get; set; }
        public int? LastModifiedBy { get; set; }
        public int? TillId { get; set; }
        public int? SiteId { get; set; }

        public virtual PosUser? PosUser { get; set; }
        public virtual Till? Till { get; set; }
        public virtual Site? Site { get; set; }

    }
}
