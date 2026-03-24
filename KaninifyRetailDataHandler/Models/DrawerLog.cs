using EntityFrameworkDatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models
{
    public class DrawerLog
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey(nameof(PosUser))]
    
        public int OpenedById { get; set; }
        public DateTime DrawerOpenDateTime { get; set; } = DateTime.UtcNow;
        public DateTime Date_Created { get; set; } = DateTime.UtcNow;
        public DateTime Last_Modified { get; set; } = DateTime.UtcNow;
        public int? Created_By_Id { get; set; }
        public int? Last_Modified_By_Id { get; set; }
        public int? Site_Id { get; set; }
        public int? Till_Id { get; set; }
        [ForeignKey(nameof(Shift))]
        public int? Shift_Id { get; set; }
        [ForeignKey(nameof(DayLog))]
        public int? DayLog_Id { get; set; }
        public DrawerLogType DrawerLogType { get; set; } = DrawerLogType.Sale;
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
        public virtual PosUser? OpenedBy { get; set; }
        public virtual PosUser? Created_By { get; set; }
        public virtual PosUser? Last_Modified_By { get; set; }
        public virtual Site? Site { get; set; }
        public virtual Till? Till { get; set; }
        public virtual Shift? Shift { get; set; }
        public virtual DayLog? DayLog { get; set; }
    }

    public enum DrawerLogType
    {
        Sale, NoSale, Refund, PayIn, Payout, Float, TillAdjustment, TillTransfer, DayStart, DayEnd, ShiftStart, ShiftEnd
    }   
}
