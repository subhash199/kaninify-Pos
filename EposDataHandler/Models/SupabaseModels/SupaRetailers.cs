using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaRetailers")]
    public class SupaRetailers
    {
        [Key]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Column("RetailName")]
        public string RetailName { get; set; }

        [Column("AccessToken")]
        public string AccessToken { get; set; }

        [Column("RefreshToken")]
        public string RefreshToken { get; set; }

        [Column("Password")]
        public string Password { get; set; } // Hashed password

        [Column("FirstLine_Address")]
        public string FirstLine_Address { get; set; }

        [Column("SecondLine_Address")]
        public string? SecondLine_Address { get; set; }

        [Column("City")]
        public string City { get; set; }

        [Column("County")]
        public string? County { get; set; }

        [Column("Country")]
        public string Country { get; set; }

        [Column("Postcode")]
        public string Postcode { get; set; }

        [Column("Vat_Number")]
        public string? Vat_Number { get; set; }

        [Column("Business_Registration_Number")]
        public string? Business_Registration_Number { get; set; }

        [Column("Business_Type")]
        public string? Business_Type { get; set; }

        [Column("Currency")]
        public string Currency { get; set; }

        [Column("TimeZone")]
        public string? TimeZone { get; set; }

        [Column("Contact_Name")]
        public string? Contact_Name { get; set; }

        [Column("Contact_Position")]
        public string? Contact_Position { get; set; }

        [Column("Email")]
        public string Email { get; set; }

        [Column("Contact_Number")]
        public string Contact_Number { get; set; }

        [Column("Website_Url")]
        public string? Website_Url { get; set; }

        [Column("Logo_Path")]
        public string? Logo_Path { get; set; }

        [Column("Business_Hours")]
        public string? Business_Hours { get; set; }

        [Column("IsActive")]
        public bool IsActive { get; set; }

        [Column("LicenseKey")]
        public string LicenseKey { get; set; }

        [Column("LicenseType")]
        public string? LicenseType { get; set; }

        [Column("LicenseIssueDate")]
        public DateTimeOffset LicenseIssueDate { get; set; }

        [Column("LicenseExpiryDate")]
        public DateTimeOffset LicenseExpiryDate { get; set; }

        [Column("MaxUsers")]
        public int MaxUsers { get; set; }

        [Column("MaxTills")]
        public int MaxTills { get; set; }

        [Column("IsLicenseValid")]
        public bool IsLicenseValid { get; set; }

        [Column("LastLicenseCheck")]
        public DateTimeOffset? LastLicenseCheck { get; set; }

        [Column("SecretKey")]
        public string? SecretKey { get; set; }

        [Column("Date_Created")]
        public DateTimeOffset Date_Created { get; set; }

        [Column("Last_Modified")]
        public DateTimeOffset Last_Modified { get; set; }

        [Column("LastSyncedAt")]
        public DateTimeOffset? LastSyncedAt { get; set; }

        [Column("SyncVersion")]
        public int SyncVersion { get; set; }

        [Column("IsSynced")]
        public bool IsSynced { get; set; }

        [Column("SyncStatus")]
        public SyncStatus SyncStatus { get; set; }

        public string ApiUrl { get; set; }

        public string ApiKey { get; set; }

        public DateTimeOffset? Last_Sign_In_At { get; set; }

        public long TokenExpiryAt? { get; set; }
        public SupaRetailers()
        {
            // Initialize default values
            RetailerId = Guid.NewGuid();
            Date_Created = DateTimeOffset.UtcNow;
            Last_Modified = DateTimeOffset.UtcNow;
            IsActive = true;
            IsLicenseValid = false;
            Currency = "GBP";
            TimeZone = "GMT";
            MaxUsers = 1;
            MaxTills = 1;

            // Initialize sync-related fields
            SyncVersion = 1;
            IsSynced = false;
            SyncStatus = SyncStatus.Pending;
        }
    }
}