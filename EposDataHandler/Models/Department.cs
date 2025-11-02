using DataHandlerLibrary.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace EntityFrameworkDatabaseLibrary.Models
{

    public class Department
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();

        [Required]
        [MaxLength(100)]
        public string Department_Name { get; set; }

        [MaxLength(255)]
        public string? Department_Description { get; set; }
        public bool Age_Restricted { get; set; }
        public bool Separate_Sales_In_Reports { get; set; }
        public bool Is_Activated { get; set; } = true;
        public bool Is_Deleted { get; set; } = false;
        public bool Allow_Staff_Discount { get; set; } = false;
        public DateTime Date_Created { get; set; } = DateTime.UtcNow;
        public DateTime Last_Modified { get; set; } = DateTime.UtcNow;
        public int? Created_By_Id { get; set; }
        public int? Last_Modified_By_Id { get; set; }
        public int? Site_Id { get; set; }
        public int? Till_Id { get; set; }
     
        public virtual PosUser? Created_By { get; set; }
        public virtual PosUser? Last_Modified_By { get; set; }
        public virtual Site? Site { get; set; }
        public virtual Till? Till { get; set; }
    }
}
