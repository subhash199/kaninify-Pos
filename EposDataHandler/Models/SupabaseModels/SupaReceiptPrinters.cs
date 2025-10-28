using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaReceiptPrinters")]
    public class SupaReceiptPrinters
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("Printer_Id")]
        public int Printer_Id { get; set; }

        [Column("Printer_Name")]
        public string Printer_Name { get; set; }

        [Column("Printer_IP_Address")]
        public string? Printer_IP_Address { get; set; }

        [Column("Printer_Port_Number")]
        public int? Printer_Port_Number { get; set; }

        [Column("Printer_Password")]
        public string? Printer_Password { get; set; }

        [Column("Paper_Width")]
        public int Paper_Width { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; }

        [Column("Is_Deleted")]
        public bool Is_Deleted { get; set; }

        [Column("Date_Created")]
        public DateTimeOffset Date_Created { get; set; }

        [Column("Last_Modified")]
        public DateTimeOffset Last_Modified { get; set; }

        [Column("Created_By_Id")]
        public int? Created_By_Id { get; set; }

        [Column("Last_Modified_By_Id")]
        public int? Last_Modified_By_Id { get; set; }

        [Column("Site_Id")]
        public int? Site_Id { get; set; }

        [Column("Till_Id")]
        public int? Till_Id { get; set; }

        [Column("Is_Primary")]
        public bool Is_Primary { get; set; }

        [Column("SyncStatus")]
        public SyncStatus SyncStatus { get; set; }

        [Column("Print_Receipt")]
        public bool Print_Receipt { get; set; }

        [Column("Print_Label")]
        public bool Print_Label { get; set; }

        public SupaReceiptPrinters()
        {
            // Initialize default values
            Date_Created = DateTimeOffset.UtcNow;
            Last_Modified = DateTimeOffset.UtcNow;
            Is_Active = true;
            Is_Deleted = false;
            Is_Primary = false;
            Print_Receipt = true;
            Print_Label = true;
            SyncStatus = SyncStatus.Pending;
        }
    }
}