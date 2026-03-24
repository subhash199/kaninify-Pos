using DataHandlerLibrary.Models;
using EntityFrameworkDatabaseLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EposRetail.Models
{
    public class SalesBasket
    {
        public SalesTransaction Transaction { get; set; }
        public List<SalesItemTransaction> SalesItemsList { get; set; }
    }
}
