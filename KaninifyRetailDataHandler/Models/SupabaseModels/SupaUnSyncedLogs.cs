using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    public class SupaUnSyncedLogs
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public long? Supa_Id { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public Guid RetailerId { get; set; }
        public string TableName { get; set; }
        public long RecordId { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public Operation Operation { get; set; }

    }
}
