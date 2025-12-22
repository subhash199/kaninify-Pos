using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlerLibrary.Models.PaymentsModels.Card
{
    public class TeyaInfo
    {
        public string ExePath { get; set; }
        public string CerFilePath { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public bool IsProduction { get; set; }
        public string AppId { get; set; }
        public string Appversion { get; set; }
        public string AppCurrency { get; set; } = "GBP";
    }
}
