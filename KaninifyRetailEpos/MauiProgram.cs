using DataHandlerLibrary.Interfaces;
using DataHandlerLibrary.Models;
using DataHandlerLibrary.Services;
using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using EposDataHandler.Services;
using EposRetail.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Maui.LifecycleEvents;
using System.Reflection;

namespace EposRetail;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // Load configuration from appsettings.json
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream("EposRetail.Appsettings.json");

        var config = new ConfigurationBuilder()
            .AddJsonStream(stream)
            .Build();

        // Get the encrypted connection string and encryption key from configuration
        string encryptedConnectionString = config.GetSection("ConnectionStrings:DefaultConnection").Value;
        string encryptionKey = config.GetSection("Encryption:Key").Value;
        builder.Services.AddSingleton<ScreenInfoService>();

        builder.Services.AddTransient<AESEncryptDecryptServices>();

        // Decrypt once at startup
        var aes = new AESEncryptDecryptServices();
        var decryptedConnectionString = aes.Decrypt(encryptedConnectionString, encryptionKey);

        // Register DbContext factory with Npgsql
        builder.Services.AddDbContextFactory<DatabaseInitialization>(options =>
            options.UseNpgsql(decryptedConnectionString));

        // Provide scoped DbContext instances for existing services via the factory
        builder.Services.AddScoped<DatabaseInitialization>(sp =>
            sp.GetRequiredService<IDbContextFactory<DatabaseInitialization>>().CreateDbContext());

        builder.Services.AddScoped<ProductServices>();        // Changed from AddSingleton
        builder.Services.AddScoped<DepartmentServices>();     // Changed from AddSingleton
        builder.Services.AddScoped<VatServices>();            // Changed from AddSingleton
        builder.Services.AddScoped<SalesTransactionServices>(); // Changed from AddSingleton
        builder.Services.AddScoped<SalesItemTransactionServices>(); // Changed from AddSingleton
        builder.Services.AddScoped<GeneralServices>();        // Changed from AddSingleton
        builder.Services.AddScoped<CheckoutService>();
        builder.Services.AddSingleton<CheckoutStateService>();
        builder.Services.AddSingleton<CustomerDisplayService>();
        builder.Services.AddScoped<PosUserServices>();       // Changed from AddSingleton
        builder.Services.AddScoped<UserSiteAccessServices>(); // Changed from AddSingleton
        builder.Services.AddScoped<SiteServices>();          // Changed from AddSingleton
        builder.Services.AddScoped<UserManagementServices>(); // Changed from AddSingleton
        builder.Services.AddScoped<VoidedProductServices>();
        builder.Services.AddScoped<DayLogServices>();
        builder.Services.AddScoped<PromotionServices>();
        builder.Services.AddScoped<MigrateDataServices>();
        builder.Services.AddScoped<PayoutServices>(); // Ensure DbContext is scoped
        builder.Services.AddScoped<IPrinterService, PrinterServices>(); // Ensure DbContext is scoped
        builder.Services.AddScoped<StockTransactionServices>(); // Ensure DbContext is scoped
        builder.Services.AddScoped<TillServices>(); // Ensure DbContext is scoped
        builder.Services.AddScoped<ShiftServices>(); // Ensure DbContext is scoped
        builder.Services.AddScoped<DrawerLogServices>(); // Ensure DbContext is scoped
        builder.Services.AddScoped<StockOrderGenerationService>(); // Ensure DbContext is scoped
        builder.Services.AddScoped<SupplierServices>(); // Ensure DbContext is scoped
        builder.Services.AddScoped<SupplierItemsServices>(); // Ensure DbContext is scoped
        builder.Services.AddScoped<ReceiptPrinterServices>();
        builder.Services.AddScoped<PrinterManagementService>();
        builder.Services.AddScoped<StockRefillServices>();
        builder.Services.AddScoped<ErrorLogServices>();
        builder.Services.AddScoped<GlobalErrorLogService>();
        builder.Services.AddScoped<RetailerServices>();
        builder.Services.AddScoped<SupabaseSyncService>();
        builder.Services.AddScoped<UnknownProductServices>();
        builder.Services.AddScoped<DepartmentButtonsLayoutStorage>();
        builder.Services.AddScoped<PaymentTerminalSettingsServices>();
        builder.Services.AddScoped<CardTransactionServices>();
        builder.Services.AddScoped<TeyaPosLinkService>();

        // Register enhanced UserSessionService
        builder.Services.AddSingleton<UserSessionService>();
        builder.Services.AddSingleton<List<ReceiptPrinter>>();
        builder.Services.AddSingleton<PosUser>();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if WINDOWS
        // Put the device into a tablet-friendly state on launch. Windows 11 has no
        // tablet-mode toggle, so instead we enable the per-user setting that lets the
        // touch keyboard appear automatically on text fields. Fields that are scanned
        // rather than typed opt out with inputmode="none".
        EnableTouchKeyboardAutoInvoke();
#endif

#if WINDOWS
        // Disable pinch zoom so touch screens cannot scale the POS UI.
        Microsoft.AspNetCore.Components.WebView.Maui.BlazorWebViewHandler.BlazorWebViewMapper.AppendToMapping("DisableTouchZoom",
            (handler, view) =>
            {
                if (handler.PlatformView is not Microsoft.UI.Xaml.Controls.WebView2 webView)
                {
                    return;
                }

                void DisablePinchZoom()
                {
                    try
                    {
                        var coreWebView = webView.CoreWebView2;
                        if (coreWebView is null)
                        {
                            return;
                        }

                        coreWebView.Settings.IsPinchZoomEnabled = false;
                    }
                    catch (Exception)
                    {
                        // IsPinchZoomEnabled is only supported by newer WebView2 runtimes.
                    }
                }

                if (webView.CoreWebView2 is null)
                {
                    webView.CoreWebView2Initialized += (_, _) => DisablePinchZoom();
                }
                else
                {
                    DisablePinchZoom();
                }
            });
#endif

#if WINDOWS
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(windows => windows.OnWindowCreated(window =>
            {
                if (window.AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
                {
                    presenter.Maximize();
                }
            }));
        });
#endif

        // Register configuration for dependency injection
        builder.Services.AddSingleton<IConfiguration>(config);

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

#if WINDOWS
    /// <summary>
    /// Enables the per-user Windows setting that shows the touch keyboard automatically
    /// when a text field is focused. This mirrors Settings &gt; Time &amp; language &gt; Typing &gt;
    /// Touch keyboard &gt; Show the touch keyboard, and is what gives tablet-style keyboard
    /// behaviour without the tablet-mode switch that Windows 11 removed.
    /// </summary>
    private static void EnableTouchKeyboardAutoInvoke()
    {
        try
        {
            using var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\TabletTip\1.7");

            if (key is null)
            {
                return;
            }

            if (key.GetValue("EnableDesktopModeAutoInvoke") is int current && current == 1)
            {
                return;
            }

            key.SetValue("EnableDesktopModeAutoInvoke", 1, Microsoft.Win32.RegistryValueKind.DWord);
        }
        catch (Exception)
        {
            // Registry access can be blocked by policy; the app must still start.
        }
    }
#endif
}
