using EntityFrameworkDatabaseLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models
{
    public class ProductRefillDTO
    {
        public int Refill_QTY { get; set; }
        public Product Product { get; set; }
        public bool Is_Refill { get; set; } = false;
    }
}
