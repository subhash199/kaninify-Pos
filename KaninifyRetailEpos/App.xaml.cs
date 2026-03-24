
using EposRetail.Services;
using Microsoft.Maui.Devices;
namespace EposRetail
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new MainPage()) { Title = "EposRetail" };

            // Set window to fullscreen/maximized by using screen dimensions
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            window.Width = displayInfo.Width / displayInfo.Density;
            window.Height = displayInfo.Height / displayInfo.Density;
            window.X = 0;
            window.Y = 0;

            return window;
        }
    }
}
