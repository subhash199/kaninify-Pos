using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EposRetail.Models
{
    public class MessageModel
    {
        public bool IsVisible { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string primaryButtonText { get; set; } = "OK";
        public string? secondaryButtonText { get; set; } 
    }
}
