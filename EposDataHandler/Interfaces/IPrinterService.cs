using DataHandlerLibrary.Models;
using EntityFrameworkDatabaseLibrary.Models;

namespace DataHandlerLibrary.Interfaces
{
    public interface IPrinterService
    {
        /// <summary>
        /// Initializes the printer service with the required configuration
        /// </summary>
        /// <param name="printerModel">The printer model configuration</param>
        /// <param name="site">The site information</param>
        /// <param name="dayLog">The day log information</param>
        /// <returns>True if initialization was successful</returns>
        Task<bool> InitializeAsync(ReceiptPrinter printerModel, Site site, DayLog dayLog);

        /// <summary>
        /// Checks if the printer service is initialized and ready to use
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Prints labels for the specified products
        /// </summary>
        /// <param name="products">List of products to print labels for</param>
        /// 

        public Task PrintShortageProductsList(List<ProductShortageDTO> shortageProducts);

        public Task PrintExpiryProductsList(List<Product> expiryProducts);

        public Task PrintRefillProductsAsync(List<ProductRefillDTO> refillProducts);
        void PrintLabel(List<Product>? products);

        public Task PrintEndOfDayReport(DayLog? dayLog, List<SalesTransaction>? transactions,
          List<Department>? departments, List<Vat>? vats, List<Payout>? payouts,
          List<StockTransaction> stockTransactions,
          List<VoidedProduct> voidedProducts);

        public Task PrintShiftEndReport(Shift? shiftLog, List<SalesTransaction>? transactions,
          List<Department>? departments, List<Vat>? vats, List<Payout>? payouts,
          List<StockTransaction> stockTransactions,
          List<VoidedProduct> voidedProducts);
        public Task PrintCustomSalesReport(List<SalesTransaction>? transactions, List<Department>? departments,
          List<Vat>? vats, List<Payout>? payouts, DateTime startDate, DateTime endDate
          , List<StockTransaction> stockTransactions,
          List<VoidedProduct> voidedProducts, decimal floatAmount = 0);

        public Task PrintSalesReceipt(SalesTransaction? transaction, List<SalesItemTransaction>? transactionItems);

        /// <summary>
        /// Opens the cash drawer
        /// </summary>
        void OpenDrawer();

        /// <summary>
        /// Generates cut page command bytes
        /// </summary>
        /// <returns>Byte array for cutting the page</returns>
        byte[] CutPage();
    }
}