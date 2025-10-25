using DataHandlerLibrary.Models.SupabaseModels;
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
    public class SyncedLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public string TableName { get; set; }
        public int RecordId { get; set; }
        public SyncStatus SyncStatus { get; set; }
        [Column(TypeName = "text")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Operation Operation { get; set; }
        [Column(TypeName = "text")]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SyncLocation SyncLocation { get; set; }
        public DateTime? SyncedDateTime { get; set; } = DateTime.UtcNow;
        public DateTime LastModified { get; set; } = DateTime.UtcNow;


    }
}
