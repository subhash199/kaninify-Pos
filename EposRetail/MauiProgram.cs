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

        builder.Services.AddSingleton<AESEncryptDecryptServices>();

        // Register DatabaseInitialization with decrypted connection string
        builder.Services.AddScoped<DatabaseInitialization>(provider =>
        {
            var encryptionService = provider.GetRequiredService<AESEncryptDecryptServices>();
            var decryptedConnectionString = encryptionService.Decrypt(encryptedConnectionString, encryptionKey);
            return new DatabaseInitialization(decryptedConnectionString);
        });
        builder.Services.AddScoped<ProductServices>();        // Changed from AddSingleton
        builder.Services.AddScoped<DepartmentServices>();     // Changed from AddSingleton
        builder.Services.AddScoped<VatServices>();            // Changed from AddSingleton
        builder.Services.AddScoped<SalesTransactionServices>(); // Changed from AddSingleton
        builder.Services.AddScoped<SalesItemTransactionServices>(); // Changed from AddSingleton
        builder.Services.AddScoped<GeneralServices>();        // Changed from AddSingleton
        builder.Services.AddScoped<CheckoutService>();
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
        builder.Services.AddSingleton<ReceiptPrinter>();
        builder.Services.AddScoped<PrinterManagementService>();
        builder.Services.AddScoped<StockRefillServices>();
        builder.Services.AddScoped<ErrorLogServices>();
        builder.Services.AddScoped<GlobalErrorLogService>();
        builder.Services.AddScoped<RetailerServices>();
        builder.Services.AddScoped<SupabaseSyncService>();
        builder.Services.AddScoped<UnknownProductServices>();

        // Register enhanced UserSessionService
        builder.Services.AddScoped<UserSessionService>();
        builder.Services.AddSingleton<ReceiptPrinter>();
        builder.Services.AddSingleton<PosUser>();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        // Register configuration for dependency injection
        builder.Services.AddSingleton<IConfiguration>(config);

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
