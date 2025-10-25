using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DataHandlerLibrary.Models;

namespace DataHandlerLibrary.Models.SupabaseModels
{
    [Table("SupaVats")]
    public class SupaVats
    {
        [Key]
        [Column("VAT_ID")]
        public int VAT_ID { get; set; }

        [Required]
        [Column("VAT_Name")]
        public string VAT_Name { get; set; } = string.Empty;

        [Column("VAT_Value", TypeName = "decimal(18,2)")]
        public decimal VAT_Value { get; set; }

        [Column("VAT_Description")]
        public string VAT_Description { get; set; } = string.Empty;

        [Column("Date_Created")]
        public DateTimeOffset Date_Created { get; set; }

        [Column("Last_Modified")]
        public DateTimeOffset Last_Modified { get; set; }

        public SupaVats()
        {
            // Initialize default values
            Date_Created = DateTimeOffset.UtcNow;
            Last_Modified = DateTimeOffset.UtcNow;
        }
    }
}