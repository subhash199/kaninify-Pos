using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaPosUsers")]
    public class SupaPosUsers
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("User_ID")]
        public int User_ID { get; set; }

        [Column("First_Name")]
        public string First_Name { get; set; } = string.Empty;

        [Column("Last_Name")]
        public string Last_Name { get; set; } = string.Empty;

        [Column("Passcode")]
        public int Passcode { get; set; }

        [Column("User_Type")]
        public PosUserType User_Type { get; set; }

        [Column("Primary_Site_Id")]
        public int? Primary_Site_Id { get; set; }

        [Column("Allowed_Void_Line")]
        public bool Allowed_Void_Line { get; set; }

        [Column("Allowed_Void_Sale")]
        public bool Allowed_Void_Sale { get; set; }

        [Column("Allowed_No_Sale")]
        public bool Allowed_No_Sale { get; set; }

        [Column("Allowed_Returns")]
        public bool Allowed_Returns { get; set; }

        [Column("Allowed_Payout")]
        public bool Allowed_Payout { get; set; }

        [Column("Allowed_Refund")]
        public bool Allowed_Refund { get; set; }

        [Column("Allowed_Change_Price")]
        public bool Allowed_Change_Price { get; set; }

        [Column("Allowed_Discount")]
        public bool Allowed_Discount { get; set; }

        [Column("Allowed_Override_Price")]
        public bool Allowed_Override_Price { get; set; }

        [Column("Allowed_Manage_Users")]
        public bool Allowed_Manage_Users { get; set; }

        [Column("Allowed_Manage_Sites")]
        public bool Allowed_Manage_Sites { get; set; }

        [Column("Allowed_Manage_Tills")]
        public bool Allowed_Manage_Tills { get; set; }

        [Column("Allowed_Manage_Products")]
        public bool Allowed_Manage_Products { get; set; }

        [Column("Allowed_Manage_Suppliers")]
        public bool Allowed_Manage_Suppliers { get; set; }

        [Column("Allowed_Manage_StockTransfer")]
        public bool Allowed_Manage_StockTransfer { get; set; }

        [Column("Allowed_Manage_Vat")]
        public bool Allowed_Manage_Vat { get; set; }

        [Column("Allowed_Manage_Departments")]
        public bool Allowed_Manage_Departments { get; set; }

        [Column("Allowed_Manage_Orders")]
        public bool Allowed_Manage_Orders { get; set; }

        [Column("Allowed_Manage_Reports")]
        public bool Allowed_Manage_Reports { get; set; }

        [Column("Allowed_Manage_Settings")]
        public bool Allowed_Manage_Settings { get; set; }

        [Column("Allowed_Manage_Tax_Rates")]
        public bool Allowed_Manage_Tax_Rates { get; set; }

        [Column("Allowed_Manage_Promotions")]
        public bool Allowed_Manage_Promotions { get; set; }

        [Column("Allowed_Manage_VoidedProducts")]
        public bool Allowed_Manage_VoidedProducts { get; set; }

        [Column("Allowed_Day_End")]
        public bool Allowed_Day_End { get; set; }

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
        public SupaPosUsers()
        {
            // Initialize default values
            Date_Created = DateTime.UtcNow;
            Last_Modified = DateTime.UtcNow;
            Is_Activated = true;
            Is_Deleted = false;
            SyncStatus = (int)DataHandlerLibrary.Models.SyncStatus.Pending;
        }
    }
}