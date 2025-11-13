using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaSuppliers")]
    public class SupaSuppliers
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("Supplier_ID")]
        public int Supplier_ID { get; set; }

        [Column("Supplier_Name")]
        public string Supplier_Name { get; set; }

        [Column("Supplier_Description")]
        public string? Supplier_Description { get; set; }

        [Column("Supplier_Address")]
        public string? Supplier_Address { get; set; }

        [Column("Supplier_Phone")]
        public string? Supplier_Phone { get; set; }

        [Column("Supplier_Mobile")]
        public string? Supplier_Mobile { get; set; }

        [Column("Supplier_Email")]
        public string? Supplier_Email { get; set; }

        [Column("Supplier_Website")]
        public string? Supplier_Website { get; set; }

        [Column("Supplier_Credit_Limit", TypeName = "decimal(18,2)")]
        public decimal? Supplier_Credit_Limit { get; set; }

        [Column("Is_Activated")]
        public bool Is_Activated { get; set; }

        [Column("Is_Deleted")]
        public bool Is_Deleted { get; set; }

        [Column("Date_Created")]
        public DateTime Date_Created { get; set; }

        [Column("Last_Modified")]
        public DateTime Last_Modified { get; set; }

        [Column("Created_By_Id")]
        public int? Created_By_Id { get; set; }

        [Column("Last_Modified_By_Id")]
        public int? Last_Modified_By_Id { get; set; }

        [Column("Site_Id")]
        public int? Site_Id { get; set; }

        [Column("Till_Id")]
        public int? Till_Id { get; set; }

        [Column("SyncStatus")]
        public SyncStatus SyncStatus { get; set; }

        public SupaSuppliers()
        {
            // Initialize default values
            Date_Created = DateTime.UtcNow;
            Last_Modified = DateTime.UtcNow;
            Is_Activated = true;
            Is_Deleted = false;
            SyncStatus = SyncStatus.Pending;
        }
    }
}