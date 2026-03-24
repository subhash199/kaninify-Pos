using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models
{
    public enum StockTransferType
    {
        StockIn,
        StockOut,
        Transfer,
        Adjustment,
        Damaged,
        Expired,
        Theft,
        Delivery
    }
}
