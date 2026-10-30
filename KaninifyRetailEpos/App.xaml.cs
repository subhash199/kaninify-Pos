
using EposRetail.Services;
using Microsoft.Maui.ApplicationModel;
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
            var window = new Window(new MainPage()) { Title = "Kaninify" };

#if !WINDOWS
            // Windows maximization is configured in MauiProgram.
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
            window.Width = displayInfo.Width / displayInfo.Density;
            window.Height = displayInfo.Height / displayInfo.Density;
            window.X = 0;
            window.Y = 0;
#endif

#if WINDOWS
            // Show the customer-facing display on the second monitor once the POS window is up,
            // and close it with the POS window so the app always exits as a whole.
            window.Created += (_, _) => MainThread.BeginInvokeOnMainThread(CustomerDisplayWindow.Open);
            window.Destroying += (_, _) => CustomerDisplayWindow.CloseAll();
#endif

            return window;
        }
    }
}
