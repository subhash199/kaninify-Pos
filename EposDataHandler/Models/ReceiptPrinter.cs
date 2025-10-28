using EntityFrameworkDatabaseLibrary.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models
{
    public class ReceiptPrinter
    {
        public long? Supa_Id  { get; set; }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; } // Unique identifier for the printer
    
        public string Printer_Name { get; set; } // Name of the printer
        public string? Printer_IP_Address { get; set; } // IP address of the printer
        public int? Printer_Port_Number { get; set; } // Port number for the printer connection
        public string? Printer_Password { get; set; } // Password for the printer, if required
        public int Paper_Width { get; set; } = 58; // Width of the paper used by the printer
        public bool Print_Receipt { get; set; } = true; // Indicates if the printer is used for printing receipts
        public bool Print_Label { get; set; } = true; // Indicates if the printer is used for printing labels
        public bool Is_Active { get; set; } = true; // Indicates if the printer is currently active
        public bool Is_Deleted { get; set; } = false; // Soft delete flag
        public DateTime Date_Created { get; set; } = DateTime.UtcNow; // Timestamp when the printer was created
        public DateTime Last_Modified { get; set; } = DateTime.UtcNow; // Timestamp when the printer was last modified
        public int? Created_By_Id { get; set; } // ID of the user who created the printer
        public int? Last_Modified_By_Id { get; set; }
        public int? Site_Id { get; set; } // ID of the site to which the printer belongs
        public int? Till_Id { get; set; } // ID of the till associated with the printer
        public bool Is_Primary { get; set; } = false; // Indicates if this is the primary printer for the site
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;
        public virtual PosUser? Created_By { get; set; } // User who created the printer
        public virtual PosUser? Last_Modified_By { get; set; } // User who last modified the printer
        public virtual Site? Site { get; set; } // Site to which the printer belongs
        public virtual Till? Till { get; set; } // Till associated with the printer
    }
}
