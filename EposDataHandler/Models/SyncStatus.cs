using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models
{
    public enum SyncStatus
    {
        Pending,        // Never synced or has changes
        Synced,         // Successfully synced
        Failed,         // Sync failed
        Conflict,       // Conflict detected
        InProgress      // Currently syncing
    }
}

