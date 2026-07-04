using DataHandlerLibrary.Interfaces;
using DataHandlerLibrary.Models;
using DataHandlerLibrary.Services;
using EntityFrameworkDatabaseLibrary.Models;

namespace DataHandlerLibrary.Services
{
    public class PrinterManagementService
    {
        private readonly IPrinterService _printerServices;
        private readonly ReceiptPrinterServices _receiptPrinterServices;
        private readonly UserSessionService _userSessionService;
        private readonly List<ReceiptPrinter> _currentPrinters;
        private bool _isInitialized = false;
        private IPrinterService? _routingPrinterService;

        public PrinterManagementService(
            IPrinterService printerServices,
            ReceiptPrinterServices receiptPrinterServices,
            UserSessionService userSessionService,
            List<ReceiptPrinter> currentPrinters)
        {
            _printerServices = printerServices;
            _receiptPrinterServices = receiptPrinterServices;
            _userSessionService = userSessionService;
            _currentPrinters = currentPrinters;
        }

        public async Task<IPrinterService> GetPrinterServicesAsync()
        {
            if (_routingPrinterService == null)
            {
                _routingPrinterService = new RoutingPrinterService(this, _printerServices, _userSessionService);
            }

            return _routingPrinterService;
        }

        public async Task<ReceiptPrinter?> GetCurrentPrinterAsync()
        {
            if (!_isInitialized)
            {
                await InitializePrinterAsync();
            }

            return SelectPrimaryReceiptPrinter(_currentPrinters);
        }

        public async Task<List<ReceiptPrinter>> GetActivePrintersAsync()
        {
            if (!_isInitialized)
            {
                await InitializePrinterAsync();
            }

            return _currentPrinters;
        }

        public async Task InitializePrinterAsync()
        {
            try
            {
                await _userSessionService.EnsureCompleteSessionAsync();

                var siteId = _userSessionService.GetCurrentSiteId();
                var tillId = _userSessionService.GetCurrentTillId();

                IEnumerable<ReceiptPrinter> activePrinters;
                if (siteId.HasValue)
                {
                    activePrinters = await _receiptPrinterServices.GetActivePrintersBySiteAsync(siteId.Value);
                }
                else
                {
                    var allPrinters = await _receiptPrinterServices.GetAllAsync(true);
                    activePrinters = allPrinters.Where(p => p.Is_Active);
                }

                if (tillId.HasValue)
                {
                    activePrinters = activePrinters.Where(p => p.Till_Id == null || p.Till_Id == tillId.Value);
                }

                var ordered = activePrinters
                    .Where(p => p.Is_Active && !p.Is_Deleted)
                    .OrderByDescending(p => p.Is_Primary)
                    .ThenByDescending(p => p.Print_Receipt)
                    .ThenByDescending(p => p.Print_Label)
                    .ThenBy(p => p.Id)
                    .ToList();

                _currentPrinters.Clear();
                _currentPrinters.AddRange(ordered);

                _isInitialized = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing printer: {ex.Message}");
                _isInitialized = true; // Mark as initialized even on error to prevent infinite loops
            }
        }

        public async Task RefreshPrinterAsync()
        {
            _isInitialized = false;
            await InitializePrinterAsync();
        }

        public async Task SetPrinterAsync(ReceiptPrinter printer)
        {
            _currentPrinters.Clear();
            _currentPrinters.Add(printer);
            _isInitialized = true;
            await Task.CompletedTask;
        }

        public bool IsInitialized => _isInitialized;

        private static ReceiptPrinter? SelectPrimaryReceiptPrinter(IEnumerable<ReceiptPrinter> printers)
        {
            var list = printers?.Where(p => p.Is_Active && !p.Is_Deleted).ToList() ?? new List<ReceiptPrinter>();
            if (list.Count == 0)
            {
                return null;
            }

            var receiptPrinters = list.Where(p => p.Print_Receipt).ToList();
            if (receiptPrinters.Count > 0)
            {
                return receiptPrinters
                    .OrderByDescending(p => p.Is_Primary)
                    .ThenBy(p => p.Id)
                    .FirstOrDefault();
            }

            return list
                .OrderByDescending(p => p.Is_Primary)
                .ThenBy(p => p.Id)
                .FirstOrDefault();
        }

        private sealed class RoutingPrinterService : IPrinterService
        {
            private readonly PrinterManagementService _printerManagementService;
            private readonly IPrinterService _inner;
            private readonly UserSessionService _userSessionService;

            public RoutingPrinterService(PrinterManagementService printerManagementService, IPrinterService inner, UserSessionService userSessionService)
            {
                _printerManagementService = printerManagementService;
                _inner = inner;
                _userSessionService = userSessionService;
            }

            public bool IsInitialized => true;

            public async Task<bool> InitializeAsync(ReceiptPrinter printerModel, Site site, DayLog dayLog)
            {
                _printerManagementService._currentPrinters.Clear();
                _printerManagementService._currentPrinters.Add(printerModel);
                _printerManagementService._isInitialized = true;
                return await _inner.InitializeAsync(printerModel, site, dayLog);
            }

            public void PrintLabel(List<Product>? products)
            {
                _userSessionService.EnsureCompleteSessionAsync().GetAwaiter().GetResult();
                _printerManagementService.InitializePrinterAsync().GetAwaiter().GetResult();

                var labelPrinters = _printerManagementService._currentPrinters
                    .Where(p => p.Is_Active && !p.Is_Deleted && p.Print_Label)
                    .OrderByDescending(p => p.Is_Primary)
                    .ThenBy(p => p.Id)
                    .ToList();

                if (labelPrinters.Count == 0)
                {
                    labelPrinters = _printerManagementService._currentPrinters
                        .Where(p => p.Is_Active && !p.Is_Deleted)
                        .OrderByDescending(p => p.Is_Primary)
                        .ThenBy(p => p.Id)
                        .ToList();
                }

                foreach (var printer in labelPrinters)
                {
                    _inner.InitializeAsync(printer, _userSessionService.CurrentSite!, _userSessionService.CurrentDayLog!).GetAwaiter().GetResult();
                    _inner.PrintLabel(products);
                }
            }

            public async Task<bool> PrintSalesReceipt(SalesTransaction? transaction, List<SalesItemTransaction>? transactionItems)
            {
                await _userSessionService.EnsureCompleteSessionAsync();
                await _printerManagementService.InitializePrinterAsync();

                var receiptPrinters = _printerManagementService._currentPrinters
                    .Where(p => p.Is_Active && !p.Is_Deleted && p.Print_Receipt)
                    .OrderByDescending(p => p.Is_Primary)
                    .ThenBy(p => p.Id)
                    .ToList();

                if (receiptPrinters.Count == 0)
                {
                    return false;
                }

                foreach (var printer in receiptPrinters)
                {
                    await _inner.InitializeAsync(printer, _userSessionService.CurrentSite!, _userSessionService.CurrentDayLog!);
                    await _inner.PrintSalesReceipt(transaction, transactionItems);
                }

                return true;
            }

            public async Task PrintCustomSalesReport(List<SalesTransaction>? transactions, List<Department>? departments, List<Vat>? vats, List<Payout>? payouts, DateTime startDate, DateTime endDate, List<StockTransaction> stockTransactions, List<VoidedProduct> voidedProducts, decimal floatAmount = 0)
            {
                await PrintForReceiptPrintersAsync(p => p.PrintCustomSalesReport(transactions, departments, vats, payouts, startDate, endDate, stockTransactions, voidedProducts, floatAmount));
            }

            public async Task PrintEndOfDayReport(DayLog? dayLog, List<SalesTransaction>? transactions, List<Department>? departments, List<Vat>? vats, List<Payout>? payouts, List<StockTransaction> stockTransactions, List<VoidedProduct> voidedProducts)
            {
                await PrintForReceiptPrintersAsync(p => p.PrintEndOfDayReport(dayLog, transactions, departments, vats, payouts, stockTransactions, voidedProducts));
            }

            public async Task PrintShiftEndReport(Shift? shiftLog, List<SalesTransaction>? transactions, List<Department>? departments, List<Vat>? vats, List<Payout>? payouts, List<StockTransaction> stockTransactions, List<VoidedProduct> voidedProducts)
            {
                await PrintForReceiptPrintersAsync(p => p.PrintShiftEndReport(shiftLog, transactions, departments, vats, payouts, stockTransactions, voidedProducts));
            }

            public async Task PrintRefillProductsAsync(List<ProductRefillDTO> refillProducts)
            {
                await PrintForReceiptPrintersAsync(p => p.PrintRefillProductsAsync(refillProducts));
            }

            public async Task PrintShortageProductsList(List<ProductShortageDTO> shortageProducts)
            {
                await PrintForReceiptPrintersAsync(p => p.PrintShortageProductsList(shortageProducts));
            }

            public async Task PrintExpiryProductsList(List<Product> expiryProducts)
            {
                await PrintForReceiptPrintersAsync(p => p.PrintExpiryProductsList(expiryProducts));
            }

            public void OpenDrawer()
            {
                _userSessionService.EnsureCompleteSessionAsync().GetAwaiter().GetResult();
                _printerManagementService.InitializePrinterAsync().GetAwaiter().GetResult();

                var drawerPrinter = _printerManagementService._currentPrinters
                    .Where(p => p.Is_Active && !p.Is_Deleted && p.Print_Receipt)
                    .OrderByDescending(p => p.Is_Primary)
                    .ThenBy(p => p.Id)
                    .FirstOrDefault();

                if (drawerPrinter == null)
                {
                    return;
                }

                _inner.InitializeAsync(drawerPrinter, _userSessionService.CurrentSite!, _userSessionService.CurrentDayLog!).GetAwaiter().GetResult();
                _inner.OpenDrawer();
            }

            public byte[] CutPage() => _inner.CutPage();

            private async Task PrintForReceiptPrintersAsync(Func<IPrinterService, Task> action)
            {
                await _userSessionService.EnsureCompleteSessionAsync();
                await _printerManagementService.InitializePrinterAsync();

                var receiptPrinters = _printerManagementService._currentPrinters
                    .Where(p => p.Is_Active && !p.Is_Deleted && p.Print_Receipt)
                    .OrderByDescending(p => p.Is_Primary)
                    .ThenBy(p => p.Id)
                    .ToList();

                foreach (var printer in receiptPrinters)
                {
                    await _inner.InitializeAsync(printer, _userSessionService.CurrentSite!, _userSessionService.CurrentDayLog!);
                    await action(_inner);
                }
            }
        }
    }
}
