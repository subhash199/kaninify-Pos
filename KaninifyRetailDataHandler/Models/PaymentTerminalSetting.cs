using EntityFrameworkDatabaseLibrary.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataHandlerLibrary.Models
{
    public class PaymentTerminalSetting
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(40)]
        public string Provider { get; set; } = "Teya";

        [MaxLength(100)]
        public string? Display_Name { get; set; }

        [MaxLength(100)]
        public string DeviceCode {get; set;}

        [MaxLength(200)]
        public string? Merchant_Display_Name { get; set; }

        [MaxLength(100)]
        public string? Store_Id { get; set; }

        [MaxLength(200)]
        public string? Store_Name { get; set; }

        [MaxLength(200)]
        public string? Store_Address_Line_1 {get; set;}
        [MaxLength(200)]
        public string? Store_Address_Line_2 {get; set;}

        [MaxLength(100)]
        public string? Store_City { get; set; }

        [MaxLength(100)]
        public string? Store_Country { get; set; }

        [MaxLength (20)]
        public string? Postcode {get; set; }

        [MaxLength(100)]
        public string? Terminal_Id { get; set; }

        [MaxLength(200)]
        public string? Terminal_Name { get; set; }

        [MaxLength(200)]
        public string? Terminal_Serial_Number { get; set; }

        [MaxLength(120)]
        public string Currency_Code { get; set; } = "GBP";

        [MaxLength(200)]
        public string? Epos_Instance_Id { get; set; }

        public bool Is_Enabled { get; set; } = false;
        public bool Is_Pay_At_Counter_Enabled { get; set; } = true;

        public string? Access_Token { get; set; }
        public string? Refresh_Token { get; set; }
        public DateTime? Access_Token_Expires_At { get; set; }
        public DateTime? Last_Verified_At { get; set; }

        [MaxLength(120)]
        public string? Last_Known_Status { get; set; }

        public DateTime Date_Created { get; set; } = DateTime.UtcNow;
        public DateTime Last_Modified { get; set; } = DateTime.UtcNow;
        public int? Created_By_Id { get; set; }
        public int? Last_Modified_By_Id { get; set; }
        public int? Site_Id { get; set; }
        public int? Till_Id { get; set; }
        public SyncStatus SyncStatus { get; set; } = SyncStatus.Pending;

        public virtual PosUser? Created_By { get; set; }
        public virtual PosUser? Last_Modified_By { get; set; }
        public virtual Site? Site { get; set; }
        public virtual Till? Till { get; set; }
    }
}
