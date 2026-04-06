using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    public class SupaSyncedLogs
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public Guid RetailerId { get; set; }
        public string TableName { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Synced;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        public Guid? ModifiedBy { get; set; }
        [JsonPropertyName("JsonRecordIds")]
        public string? JsonRecordIds { get; set; }
        public SyncLocation SyncLocation { get; set; }
        public Operation Operation { get; set; }
    }

    public enum SyncLocation
    {
        Central,
        Local
    }
}
