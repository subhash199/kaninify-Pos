using DataHandlerLibrary.Models;
using DataHandlerLibrary.Services;
using EntityFrameworkDatabaseLibrary.Models;
using EposRetail.Services;

namespace EposRetail.Extensions
{
    public static class PrinterExtensions
    {
        public static async Task<ReceiptPrinter?> GetCurrentPrinterAsync(this IServiceProvider serviceProvider)
        {
            var printerManagement = serviceProvider.GetRequiredService<PrinterManagementService>();
            return await printerManagement.GetCurrentPrinterAsync();
        }

        public static async Task RefreshCurrentPrinterAsync(this IServiceProvider serviceProvider)
        {
            var printerManagement = serviceProvider.GetRequiredService<PrinterManagementService>();
            await printerManagement.RefreshPrinterAsync();
        }
    }
}