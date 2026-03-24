using EntityFrameworkDatabaseLibrary.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    public class SupaUnknownProduct
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        public int Id { get; set; }
        public Guid RetailerId { get; set; }
        public string ProductBarcode { get; set; }
        public bool IsResolved { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime LastModified { get; set; }
        public int? DaylogId { get; set; }
        public int? ShiftId { get; set; }
        public int? SiteId { get; set; }
        public int? TillId { get; set; }
        public int? CreatedById { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
    }
}
