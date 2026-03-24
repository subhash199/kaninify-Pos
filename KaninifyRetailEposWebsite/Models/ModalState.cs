using EposRetail.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EposRetail.Models
{
    public class ModalState
    {
        public bool ShowConfirmModal { get; set; } = false;
        public bool ShowPrintModal { get; set; } = false;
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
        public ActionType ActionType { get; set; } = ActionType.None;

        public void ShowConfirmation(string title, string message, ActionType actionType)
        {
            Title = title;
            Message = message;
            ActionType = actionType;
            ShowConfirmModal = true;
        }

        public void ShowPrintDialog(string title, string message, ActionType actionType)
        {
            ActionType = actionType;
            Title = title;
            Message = message;
            ShowPrintModal = true;
        }

        public void CloseAll()
        {
            ShowConfirmModal = false;
            ShowPrintModal = false;
            Title = "";
            Message = "";
            ActionType = ActionType.None;
        }
    }
}
