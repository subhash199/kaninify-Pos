using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EposRetail.Models
{
    public class ModalSettings
    {
        public bool ShowModal { get; set; } = false;
        public string ModalTitle { get; set; } = "Notice";
        public string ModalMessage { get; set; } = "This is a message.";
        public ModalEnum ModalEnum { get; set; } = ModalEnum.Default;
        public string SecondaryIconLink { get; set; } = string.Empty;
    }
    public enum ModalEnum
    {
        ProductActivate,
        ProductNotFound,
        GenericProduct,
        Error,
        Default,
        Confirmation,
        Refund,
        Transaction,
        ChangeDue
    }

}
