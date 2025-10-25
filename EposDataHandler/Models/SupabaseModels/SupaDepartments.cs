using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaDepartments")]
    public class SupaDepartments
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public long? Supa_Id { get; set; }
        [Required]
        [Column("RetailerId")]
        public Guid RetailerId { get; set; }

        [Key]
        [Column("Department_ID")]
        public int Department_ID { get; set; }

        [Required]
        [Column("Department_Name")]
        public string Department_Name { get; set; } = string.Empty;

        [Column("Department_Description")]
        public string Department_Description { get; set; } = string.Empty;

        [Column("Age_Restricted")]
        public bool Age_Restricted { get; set; }

        [Column("Separate_Sales_In_Reports")]
        public bool Separate_Sales_In_Reports { get; set; }

        [Column("Stock_Refill_Print")]
        public bool Stock_Refill_Print { get; set; }

        [Column("Is_Activated")]
        public bool Is_Activated { get; set; }

        [Column("Is_Deleted")]
        public bool Is_Deleted { get; set; }

        [Column("Allow_Staff_Discount")]
        public bool Allow_Staff_Discount { get; set; }

        [Column("Date_Created")]
        public DateTimeOffset Date_Created { get; set; }

        [Column("Last_Modified")]
        public DateTimeOffset Last_Modified { get; set; }
        
        public SupaDepartments()
        {
            // Initialize default values
            Date_Created = DateTimeOffset.UtcNow;
            Last_Modified = DateTimeOffset.UtcNow;
            Is_Activated = true;
            Is_Deleted = false;
            Allow_Staff_Discount = false;
        }
    }
}