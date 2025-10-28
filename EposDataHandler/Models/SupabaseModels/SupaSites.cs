using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaSites")]
    public class SupaSites
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("Site_Id")]
        public int Site_Id { get; set; }

        [Required]
        [Column("Site_BusinessName")]
        public string Site_BusinessName { get; set; }

        [Required]
        [Column("Site_AddressLine1")]
        public string Site_AddressLine1 { get; set; }

        [Required]
        [Column("Site_City")]
        public string Site_City { get; set; }

        [Column("Site_AddressLine2")]
        public string? Site_AddressLine2 { get; set; }

        [Column("Site_County")]
        public string? Site_County { get; set; }

        [Column("Site_Country")]
        public string Site_Country { get; set; }

        [Column("Site_Postcode")]
        public string Site_Postcode { get; set; }

        [Column("Site_ContactNumber")]
        public string? Site_ContactNumber { get; set; }

        [Column("Site_Email")]
        public string? Site_Email { get; set; }

        [Column("Site_VatNumber")]
        public string? Site_VatNumber { get; set; }

        [Column("Date_Created")]
        public DateTimeOffset Date_Created { get; set; }

        [Column("Last_Modified")]
        public DateTimeOffset Last_Modified { get; set; }

        [Column("Created_By_Id")]
        public int? Created_By_Id { get; set; }

        [Column("Last_Modified_By_Id")]
        public int? Last_Modified_By_Id { get; set; }

        [Column("Is_Active")]
        public bool Is_Active { get; set; }

        [Column("Is_Deleted")]
        public bool Is_Deleted { get; set; }

        [Column("Is_Primary")]
        public bool Is_Primary { get; set; }

        [Column("SyncStatus")]
        public SyncStatus SyncStatus { get; set; }

        public SupaSites()
        {
            // Initialize default values
            Date_Created = DateTimeOffset.UtcNow;
            Last_Modified = DateTimeOffset.UtcNow;
            Is_Active = true;
            Is_Deleted = false;
            Is_Primary = false;
            SyncStatus = SyncStatus.Pending;
        }
    }
}