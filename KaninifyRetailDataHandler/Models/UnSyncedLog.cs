using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models
{
    public class UnSyncedLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public SyncStatus SyncStatus { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "text")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Operation Operation { get; set; }
        public DateTime? LastSyncedAt { get; set; }
        public string? SyncError { get; set; }
        public int SyncRetryCount { get; set; } = 0;
    }

    
}
