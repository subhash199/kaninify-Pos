using DataHandlerLibrary.Interfaces;
using DataHandlerLibrary.Models;
using DataHandlerLibrary.Services;
using EntityFrameworkDatabaseLibrary.Models;

namespace DataHandlerLibrary.Services
{
    public class PrinterManagementService
    {
        private IPrinterService _printerServices;
        private readonly ReceiptPrinterServices _receiptPrinterServices;
        private readonly UserSessionService _userSessionService;
        private readonly ReceiptPrinter _currentPrinter;
        private bool _isInitialized = false;

        public PrinterManagementService(
            IPrinterService printerServices,
            ReceiptPrinterServices receiptPrinterServices,
            UserSessionService userSessionService,
            ReceiptPrinter currentPrinter)
        {
            _printerServices = printerServices;
            _receiptPrinterServices = receiptPrinterServices;
            _userSessionService = userSessionService;
            _currentPrinter = currentPrinter;
        }

        public async Task <IPrinterService> GetPrinterServicesAsync()
        {
            if(!_printerServices.IsInitialized)
            {
                _printerServices.InitializeAsync( await GetCurrentPrinterAsync(), _userSessionService.CurrentSite, _userSessionService.CurrentDayLog);
            }
            
            return _printerServices;
        }

        public async Task<ReceiptPrinter?> GetCurrentPrinterAsync()
        {
            if (!_isInitialized)
            {
                await InitializePrinterAsync();
            }
            return _currentPrinter;
        }

        public async Task InitializePrinterAsync()
        {
            try
            {
                // Get current site from session
                var currentSite = _userSessionService.CurrentSite;
                if (currentSite?.Id == null)
                {
                    await _userSessionService.EnsureSiteAsync();
                    currentSite = _userSessionService.CurrentSite;
                }

                if (currentSite?.Id != null)
                {
                    // Try to get primary printer for the current site
                    var primaryPrinter = await GetPrimaryPrinterForSiteAsync(currentSite.Id);
                    
                    if (primaryPrinter != null)
                    {
                        // Copy properties to the singleton instance
                        CopyPrinterProperties(primaryPrinter, _currentPrinter);
                        _isInitialized = true;
                        return;
                    }

                    // If no primary printer, get any active printer for the site
                    var sitePrinter = await GetActivePrinterForSiteAsync(currentSite.Id);
                    if (sitePrinter != null)
                    {
                        CopyPrinterProperties(sitePrinter, _currentPrinter);
                        _isInitialized = true;
                        return;
                    }
                }

                // Fallback: get any active printer
                var anyActivePrinter = await GetAnyActivePrinterAsync();
                if (anyActivePrinter != null)
                {
                    CopyPrinterProperties(anyActivePrinter, _currentPrinter);
                }

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing printer: {ex.Message}");
                _isInitialized = true; // Mark as initialized even on error to prevent infinite loops
            }
        }

        private async Task<ReceiptPrinter?> GetPrimaryPrinterForSiteAsync(int siteId)
        {
            try
            {
                var allPrinters = await _receiptPrinterServices.GetAllAsync(true);
                return allPrinters.FirstOrDefault(p => p.Site_Id == siteId && p.Is_Primary && p.Is_Active);
            }
            catch
            {
                return null;
            }
        }

        private async Task<ReceiptPrinter?> GetActivePrinterForSiteAsync(int siteId)
        {
            try
            {
                var allPrinters = await _receiptPrinterServices.GetAllAsync(true);
                return allPrinters.FirstOrDefault(p => p.Site_Id == siteId && p.Is_Active);
            }
            catch
            {
                return null;
            }
        }

        private async Task<ReceiptPrinter?> GetAnyActivePrinterAsync()
        {
            try
            {
                var allPrinters = await _receiptPrinterServices.GetAllAsync(true);
                return allPrinters.FirstOrDefault(p => p.Is_Active);
            }
            catch
            {
                return null;
            }
        }

        private void CopyPrinterProperties(ReceiptPrinter source, ReceiptPrinter target)
        {
            target.Id = source.Id;
            target.Printer_Name = source.Printer_Name;
            target.Printer_IP_Address = source.Printer_IP_Address;
            target.Printer_Port_Number = source.Printer_Port_Number;
            target.Printer_Password = source.Printer_Password;
            target.Paper_Width = source.Paper_Width;
            target.Is_Active = source.Is_Active;
            target.Is_Primary = source.Is_Primary;
            target.Site_Id = source.Site_Id;
            target.Till_Id = source.Till_Id;
            target.Date_Created = source.Date_Created;
            target.Last_Modified = source.Last_Modified;
            target.Created_By_Id = source.Created_By_Id;
            target.Last_Modified_By_Id = source.Last_Modified_By_Id;
        }

        public async Task RefreshPrinterAsync()
        {
            _isInitialized = false;
            await InitializePrinterAsync();
        }

        public async Task SetPrinterAsync(ReceiptPrinter printer)
        {
            CopyPrinterProperties(printer, _currentPrinter);
            _isInitialized = true;
        }

        public bool IsInitialized => _isInitialized;
    }
}