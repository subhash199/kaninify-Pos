using EposRetail.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Devices;

namespace EposRetail.Services
{
    public class ScreenInfoService
    {
        public ScreenModel GetScreenSize()
        {
            ScreenModel screenModel = new ScreenModel();
            screenModel.DisplayInfo = DeviceDisplay.MainDisplayInfo;
            screenModel.Height = DeviceDisplay.MainDisplayInfo.Height;
            screenModel.Width = DeviceDisplay.MainDisplayInfo.Width;
            screenModel.Density = DeviceDisplay.MainDisplayInfo.Density.ToString();
            return screenModel;
        }
    }
}
