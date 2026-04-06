using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models
{
    public class Retailer
    {
        [Key]
        public Guid RetailerId { get; set; }
        public string RetailName { get; set; }
        public string AccessToken { get; set; }
        public int? TokenExpiryAt { get; set; }
        public string RefreshToken { get; set; }
        public string ApiKey { get; set; }
        public string ApiUrl { get; set; }
        public string Password { get; set; } // Hashed password
        public string FirstLine_Address { get; set; }
        public string? SecondLine_Address { get; set; }
        public string City { get; set; }
        public string? County { get; set; }
        public string Country { get; set; }
        public string Postcode { get; set; }
        public string? Vat_Number { get; set; }
        public string? Business_Registration_Number { get; set; }
        public string? Business_Type { get; set; }
        public string Currency { get; set; } = "GBP";
        public string? TimeZone { get; set; } = "GMT";
        public string? Contact_Name { get; set; }
        public string? Contact_Position { get; set; }
        public string? Email { get; set; }
        public string Contact_Number { get; set; }
        public string? Website_Url { get; set; }
        public string? Logo_Path { get; set; }
        public string? Business_Hours { get; set; }
        public bool IsActive { get; set; } = true;

        // License Management
        public string LicenseKey { get; set; }
        public string? LicenseType { get; set; } // "Trial", "Basic", "Premium", "Enterprise"
        public DateTime LicenseIssueDate { get; set; }
        public DateTime LicenseExpiryDate { get; set; }
        public int MaxUsers { get; set; } = 1;
        public int MaxTills { get; set; } = 1;
        public bool IsLicenseValid { get; set; } = false;
        public DateTime? LastLicenseCheck { get; set; }
        public string? SecretKey { get; set; }
        public DateTime? Last_Sign_In_At { get; set; }
        public DateTime Date_Created { get; set; }
        public DateTime Last_Modified { get; set; }

        // Sync-related properties
        public DateTime? LastSyncedAt { get; set; }
        public SyncStatus SyncStatus { get; set; }
        public int SyncVersion { get; set; } = 1;
        public bool IsSynced { get; set; } = false;
    }
}
