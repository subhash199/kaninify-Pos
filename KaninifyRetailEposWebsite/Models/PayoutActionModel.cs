using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EposRetail.Models
{
    public class PayoutActionModel
    {
        public PayoutType PayoutType { get; set; }
        public decimal Amount { get; set; }
    }

    public enum PayoutType
    {
        Lottery,
        ScratchCard,
        Other
    }
}

