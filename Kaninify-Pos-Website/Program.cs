using DataHandlerLibrary.Interfaces;
using DataHandlerLibrary.Models;
using DataHandlerLibrary.Services;
using EntityFrameworkDatabaseLibrary.Data;
using EntityFrameworkDatabaseLibrary.Models;
using EposDataHandler.Services;
using Kaninify_Pos_Website.Components;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var encryptedConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var encryptionKey = builder.Configuration.GetSection("Encryption:Key").Value;
var aes = new AESEncryptDecryptServices();

var decrypedConnectionString = aes.Decrypt(encryptedConnectionString ?? string.Empty, encryptionKey);

builder.Services.AddDbContextFactory<DatabaseInitialization>(options =>
    options.UseNpgsql(decrypedConnectionString));

builder.Services.AddScoped<DatabaseInitialization>(sp =>
sp.GetRequiredService<IDbContextFactory<DatabaseInitialization>>().CreateDbContext());

// Services
builder.Services.AddScoped<ProductServices>();        // Changed from AddSingleton
builder.Services.AddScoped<DepartmentServices>();     // Changed from AddSingleton
builder.Services.AddScoped<VatServices>();            // Changed from AddSingleton
builder.Services.AddScoped<SalesTransactionServices>(); // Changed from AddSingleton
builder.Services.AddScoped<SalesItemTransactionServices>(); // Changed from AddSingleton
builder.Services.AddScoped<GeneralServices>();        // Changed from AddSingleton
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


builder.Services.AddScoped<UserSessionService>();
builder.Services.AddSingleton<ReceiptPrinter>();
builder.Services.AddSingleton<PosUser>();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
