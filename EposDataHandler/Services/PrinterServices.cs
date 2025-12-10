using DataHandlerLibrary.Interfaces;
using DataHandlerLibrary.Models;
using EntityFrameworkDatabaseLibrary.Models;
using EposDataHandler.Models;
using ESC_POS_USB_NET.Enums;
using ESC_POS_USB_NET.EpsonCommands;
using ESC_POS_USB_NET.Printer;
using Microsoft.Extensions.Logging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using ZXing;
using ZXing.Windows.Compatibility;

namespace DataHandlerLibrary.Services
{
    public class PrinterServices : IPrinterService
    {
        static PrinterServices()
        {
            // Register code page provider to support IBM860 and other legacy encodings
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        }

        private readonly StringBuilder receiptBuilder = new StringBuilder();
        private ESC_POS_USB_NET.Printer.Printer? _printer;
        private readonly ILogger<PrinterServices>? _logger;

        private ReceiptPrinter? _printerModel;
        private Site? _site;

        private int maxChar = 40;


        // Add these new global variables
        private int qtyWidth;
        private int nameWidth;
        private int priceWidth;

        private bool _isInitialized = false;

        /// <summary>
        /// Gets whether the printer service is initialized and ready to use
        /// </summary>
        public bool IsInitialized => _isInitialized;

        /// <summary>
        /// Constructor for dependency injection
        /// </summary>
        /// <param name="logger">Optional logger instance</param>
        public PrinterServices(ILogger<PrinterServices>? logger = null)
        {
            _logger = logger;
        }

        /// <summary>
        /// Initializes the printer service with the required configuration
        /// </summary>
        /// <param name="printerModel">The printer model configuration</param>
        /// <param name="site">The site information</param>
        /// <param name="dayLog">The day log information</param>
        /// <returns>True if initialization was successful</returns>
        public async Task<bool> InitializeAsync(ReceiptPrinter printerModel, Site site, DayLog dayLog)
        {
            try
            {
                _printerModel = printerModel ?? throw new ArgumentNullException(nameof(printerModel), "Printer model cannot be null");
                _site = site ?? throw new ArgumentNullException(nameof(site), "Site information cannot be null");
                InitializePrinter();
                InitializeLayout();
                _isInitialized = true;
                _logger?.LogInformation("PrinterServices initialized successfully for printer: {PrinterName}", _printerModel.Printer_Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize PrinterServices");
                _isInitialized = false;
                return false;
            }
        }


        private void InitializePrinter()
        {
            if (_printerModel == null)
            {
                throw new InvalidOperationException("Printer model is not set. Call InitializeAsync first.");
            }

            if (string.IsNullOrWhiteSpace(_printerModel.Printer_Name))
            {
                throw new ArgumentException("Printer name cannot be null or empty", nameof(_printerModel.Printer_Name));
            }

            try
            {
                _printer = new ESC_POS_USB_NET.Printer.Printer(_printerModel.Printer_Name);

                // Set character width based on paper size
                if (_printerModel.Paper_Width <= 58)
                {
                    // 58mm thermal paper - 32 characters per line (normal font)
                    maxChar = 32;
                }
                else if (_printerModel.Paper_Width >= 80)
                {
                    // 80mm thermal paper - 48 characters per line (normal font)
                    maxChar = 48;
                }
                else
                {
                    // Default fallback for other sizes
                    maxChar = 32;
                }

                _logger?.LogInformation("Printer initialized: {PrinterName}, Paper Width: {PaperWidth}mm, Max Characters: {MaxChar}",
                    _printerModel.Printer_Name, _printerModel.Paper_Width, maxChar);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to initialize printer: {PrinterName}", _printerModel.Printer_Name);
                throw new InvalidOperationException($"Failed to initialize printer '{_printerModel.Printer_Name}'", ex);
            }
        }

        private void InitializeLayout()
        {
            if (maxChar <= 0)
            {
                throw new InvalidOperationException("Invalid maximum character width");
            }

            // Calculate layout based on paper width
            if (maxChar == 32) // 58mm paper
            {
                // Set column widths for sales receipt
                qtyWidth = 3;
                nameWidth = 20;
                priceWidth = 7;
            }
            else if (maxChar == 48) // 80mm paper
            {
                // Set column widths for sales receipt
                qtyWidth = 4;
                nameWidth = 32;
                priceWidth = 10;
            }
            else
            {
                // Set column widths for sales receipt (fallback)
                qtyWidth = Math.Max(1, maxChar / 12);
                priceWidth = Math.Max(1, maxChar / 6);
                nameWidth = Math.Max(1, maxChar - qtyWidth - priceWidth - 2); // 2 spaces
            }

            _logger?.LogDebug("Layout initialized - MaxChar: {MaxChar}, qtyWidth: {qtyWidth}, nameWidth: {nameWidth}, QtyWidth: {QtyWidth}, NameWidth: {NameWidth}, PriceWidth: {PriceWidth}",
                maxChar, qtyWidth, nameWidth, priceWidth);
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized || _printer == null || _printerModel == null || _site == null)
            {
                throw new InvalidOperationException("Printer service is not properly initialized. Call InitializeAsync first.");
            }
            else
            {
                _logger?.LogDebug("Printer service is properly initialized");
                _printer.Clear();
                receiptBuilder.Clear();
            }
        }

        private void AddBusinessAddress(Site? site = null)
        {
            try
            {
                // Use provided site or fall back to instance site
                var siteToUse = site ?? _site;

                if (siteToUse == null)
                {
                    _logger?.LogWarning("Site information is null, using default business address");
                    receiptBuilder.AppendLine("Business Name Not Available");
                    receiptBuilder.AppendLine(new string('-', maxChar));
                    return;
                }

                receiptBuilder.AppendLine(siteToUse.Site_BusinessName ?? "Business Name Not Available");

                if (!string.IsNullOrWhiteSpace(siteToUse.Site_AddressLine1))
                    receiptBuilder.AppendLine(siteToUse.Site_AddressLine1);

                if (!string.IsNullOrWhiteSpace(siteToUse.Site_City))
                    receiptBuilder.AppendLine(siteToUse.Site_City);

                if (!string.IsNullOrWhiteSpace(siteToUse.Site_Postcode))
                    receiptBuilder.AppendLine(siteToUse.Site_Postcode);

                if (!string.IsNullOrWhiteSpace(siteToUse.Site_VatNumber))
                    receiptBuilder.AppendLine($"VAT: {siteToUse.Site_VatNumber}");
                else if (!string.IsNullOrWhiteSpace(siteToUse.Site_ContactNumber))
                    receiptBuilder.AppendLine($"Contact: {siteToUse.Site_ContactNumber}");

                receiptBuilder.AppendLine(new string('-', maxChar));
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error adding business address to receipt");
                receiptBuilder.AppendLine("Business Address Error");
                receiptBuilder.AppendLine(new string('-', maxChar));
            }
        }

        private void AddThankYouToReceipt()
        {
            receiptBuilder.AppendLine(new string('-', maxChar));
            receiptBuilder.AppendLine("Thank you for shopping with us!");
            receiptBuilder.AppendLine(new string('-', maxChar));
        }

        public void PrintLabel(List<Product>? products)
        {
            try
            {
                EnsureInitialized();

                if (products == null || !products.Any())
                {
                    _logger?.LogWarning("No products provided for label printing");
                    return;
                }

                _logger?.LogInformation("Starting to print {Count} product labels", products.Count);
                _printer!.InitializePrint();

                for (int i = 0; i < products.Count; i++)
                {
                    var product = products[i];
                    if (product == null)
                    {
                        _logger?.LogWarning("Skipping null product at index {Index}", i);
                        continue;
                    }

                    try
                    {
                        PrintSingleLabel(product, i + 1, products.Count);
                    }
                    catch (Exception ex)
                    {
                        _logger?.LogError(ex, "Failed to print label for product {ProductName} at index {Index}",
                            product.Product_Name ?? "Unknown", i);
                        // Continue with next product instead of failing completely
                    }
                }

                _logger?.LogInformation("Completed printing product labels");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Critical error during label printing");
                throw new InvalidOperationException("Failed to print labels", ex);
            }
        }

        private void PrintSingleLabel(Product product, int currentIndex, int totalCount)
        {
            if (_printer == null)
                throw new InvalidOperationException("Printer not initialized");

            try
            {
                if (product == null)
                {
                    _logger?.LogWarning("Product is null, cannot print label");
                    return;
                }
                var bmp = GenerateLabel(product.Product_Selling_Price.ToString(), TruncateString(product.Product_Name, 32), product.Product_Barcode, DateTime.Now.ToString("MM/dd/yyyy"), _printerModel?.Paper_Width <= 58 ? 384 : 576);
                PrintLabel(_printerModel.Printer_Name, bmp);
                RawPrinterHelper.SendBytesToPrinter(_printerModel.Printer_Name, CutPage());

                //    _printer.AlignCenter();

                //    // Price Formatting (Bold and Big)
                //    _printer.ExpandedMode(PrinterModeState.On);
                //    _printer.DoubleWidth3();
                //    _printer.BoldMode(PrinterModeState.On);

                //    var price = product.Product_Selling_Price;
                //    _printer.Append($"£{price:F2}");

                //    _printer.BoldMode(PrinterModeState.Off);
                //    _printer.ExpandedMode(PrinterModeState.Off);

                //    // Product Name
                //    _printer.DoubleWidth2();
                //    var productName = product.Product_Name ?? "Unknown Product";
                //    var truncatedName = productName.Length > 20 ? productName.Substring(0, 20) : productName;
                //    _printer.Append(truncatedName);
                //    _printer.NormalWidth();
                //    _printer.ExpandedMode(PrinterModeState.Off);

                //    // Barcode Formatting (Wide and Scannable)
                //    if (!string.IsNullOrWhiteSpace(product.Product_Barcode))
                //    {
                //        _printer.AlignCenter();
                //        _printer.SetLineHeight(40);

                //        try
                //        {
                //            _printer.Ean13(product.Product_Barcode);
                //            _printer.Append(product.Product_Barcode);
                //        }
                //        catch (Exception ex)
                //        {
                //            _logger?.LogWarning(ex, "Failed to print barcode {Barcode} for product {ProductName}",
                //                product.Product_Barcode, productName);
                //            _printer.Append($"Barcode: {product.Product_Barcode}");
                //        }
                //    }
                //    else
                //    {
                //        _logger?.LogWarning("No barcode available for product {ProductName}", productName);
                //        _printer.Append("No Barcode Available");
                //    }

                //    _printer.NewLine();
                //    _printer.NewLine();
                //    _printer.FullPaperCut();
                //    _printer.PrintDocument();
                //    _printer.Clear();

                //    _logger?.LogDebug("Successfully printed label {Current}/{Total} for product {ProductName}",
                //        currentIndex, totalCount, productName);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error printing single label for product {ProductName}",
                    product.Product_Name ?? "Unknown");
                throw;
            }
        }





        private void PrintSalesReport(List<SalesTransaction> transactions, List<Department> departments,
            List<Vat> vats, List<Payout> payouts, List<StockTransaction> stockTransactions, List<VoidedProduct> voidedProducts, decimal floatAmount = 0)
        {
            try
            {
                if (_printer == null)
                    throw new InvalidOperationException("Printer not initialized");

                decimal payoutAmount = 0;

                var refundList = new List<SalesItemTransaction>();
                var netSalesItems = new List<SalesItemTransaction>();
                var otherSalesItems = new List<SalesItemTransaction>();
                var payoutItems = new List<SalesItemTransaction>();
                var unscannedSalesItems = new List<SalesItemTransaction>();
                var serviceItems = new List<SalesItemTransaction>();
                var promotionItems = new List<SalesItemTransaction>();
                var discountItems = new List<SalesItemTransaction>();

                // Safely get departments that should be printed separately
                var separatePrintDepartments = departments?.Where(d => d?.Separate_Sales_In_Reports == true)?.ToList()
                    ?? new List<Department>();

                _logger?.LogDebug("Processing {TransactionCount} transactions for sales report", transactions?.Count ?? 0);

                // Process transactions safely
                if (transactions != null)
                {
                    foreach (var transaction in transactions)
                    {
                        if (transaction == null) continue;

                        try
                        {
                            if (transaction.SalesItemTransactions?.Any() == true)
                            {
                                foreach (var item in transaction.SalesItemTransactions)
                                {
                                    if (item == null) continue;

                                    if (item.SalesItemTransactionType == SalesItemTransactionType.Payout)
                                    {
                                        payoutItems.Add(item);
                                    }
                                    else if (item.SalesItemTransactionType == SalesItemTransactionType.Refund)
                                    {
                                        refundList.Add(item);
                                    }
                                    else if (item.SalesItemTransactionType == SalesItemTransactionType.Misc)
                                    {
                                        unscannedSalesItems.Add(item);
                                    }
                                    else if (item.SalesItemTransactionType == SalesItemTransactionType.Service)
                                    {
                                        serviceItems.Add(item);
                                    }
                                    else if (item.SalesItemTransactionType == SalesItemTransactionType.Promotion)
                                    {
                                        promotionItems.Add(item);
                                    }
                                    else if (item.SalesItemTransactionType == SalesItemTransactionType.Discount)
                                    {
                                        discountItems.Add(item);
                                    }
                                    else if (separatePrintDepartments.Any(d => d?.Id == item.Product?.Department_ID))
                                    {
                                        otherSalesItems.Add(item);
                                    }
                                    else
                                    {
                                        netSalesItems.Add(item);
                                    }
                                }
                            }

                        }
                        catch (Exception ex)
                        {
                            _logger?.LogWarning(ex, "Error processing transaction {TransactionId}", transaction.Id);
                            // Continue processing other transactions
                        }
                    }
                }

                // Calculate sales summaries with null safety and alphabetical ordering
                var netSalesList = netSalesItems
                    .Where(sale => sale?.SalesPayout_ID == null)
                    .GroupBy(sale => sale?.Product?.Department_ID ?? -1)
                    .Select(group => new
                    {
                        qty = group.Sum(i => i?.Product_QTY ?? 0),
                        departmentId = group.Key,
                        departmentName = group.Key == -1
                            ? "Default"
                            : departments?.FirstOrDefault(x => x?.Id == group.Key)?.Department_Name ?? "Unknown",
                        total = group.Sum(sale => sale?.Product_Total_Amount ?? 0)
                    })
                    .OrderBy(x => x.departmentName)
                    .ToList();

                var otherSalesList = otherSalesItems
                    .Where(sale => sale?.SalesPayout_ID == null)
                    .GroupBy(sale => sale?.Product?.Department_ID ?? -1)
                    .Select(group => new
                    {
                        qty = group.Sum(i => i?.Product_QTY ?? 0),
                        departmentId = group.Key,
                        departmentName = group.Key == -1
                            ? "Default"
                            : departments?.FirstOrDefault(x => x?.Id == group.Key)?.Department_Name ?? "Unknown",
                        total = group.Sum(sale => sale?.Product_Total_Amount ?? 0)
                    })
                    .OrderBy(x => x.departmentName)
                    .ToList();

                // Calculate unscanned sales details
                var unscannedSalesList = unscannedSalesItems
                    .Where(sale => sale?.SalesPayout_ID == null)
                    .GroupBy(sale => sale?.Product?.Department_ID ?? -1)
                    .Select(group => new
                    {
                        qty = group.Sum(i => i?.Product_QTY ?? 0),
                        departmentId = group.Key,
                        departmentName = group.Key == -1
                            ? "Default"
                            : departments?.FirstOrDefault(x => x?.Id == group.Key)?.Department_Name ?? "Unknown",
                        total = group.Sum(sale => sale?.Product_Total_Amount ?? 0)
                    })
                    .OrderBy(x => x.departmentName)
                    .ToList();

                // Calculate promotion sales details
                var promotionSalesList = promotionItems
                    .Where(sale => sale?.SalesPayout_ID == null)
                    .GroupBy(sale => sale?.Product?.Department_ID ?? -1)
                    .Select(group => new
                    {
                        qty = group.Sum(i => i?.Product_QTY ?? 0),
                        departmentId = group.Key,
                        departmentName = group.Key == -1
                            ? "Default"
                            : departments?.FirstOrDefault(x => x?.Id == group.Key)?.Department_Name ?? "Unknown",
                        total = group.Sum(sale => sale?.Product_Total_Amount ?? 0)
                    })
                    .OrderBy(x => x.departmentName)
                    .ToList();

                // Calculate discount sales details
                var discountSalesList = discountItems
                    .Where(sale => sale?.SalesPayout_ID == null)
                    .GroupBy(sale => sale?.Product?.Department_ID ?? -1)
                    .Select(group => new
                    {
                        qty = group.Sum(i => i?.Product_QTY ?? 0),
                        departmentId = group.Key,
                        departmentName = group.Key == -1
                            ? "Default"
                            : departments?.FirstOrDefault(x => x?.Id == group.Key)?.Department_Name ?? "Unknown",
                        total = group.Sum(sale => sale?.Product_Total_Amount ?? 0)
                    })
                    .OrderBy(x => x.departmentName)
                    .ToList();

                // Calculate refund details with transaction information
                var refundDetails = refundList
                    .GroupBy(r => new { r.Product?.Department_ID, r.Product?.Product_Name })
                    .Select(group => new
                    {
                        qty = group.Sum(i => i?.Product_QTY ?? 0),
                        productName = group.Key.Product_Name ?? "Unknown Product",
                        departmentName = departments?.FirstOrDefault(d => d?.Id == group.Key.Department_ID)?.Department_Name ?? "Unknown",
                        total = group.Sum(r => r?.Product_Total_Amount ?? 0)
                    })
                    .OrderBy(x => x.departmentName)
                    .ThenBy(x => x.productName)
                    .ToList();

                // Calculate stock transaction details
                var stockTransactionDetails = stockTransactions
                    .GroupBy(st => st?.StockTransactionType)
                    .Select(group => new
                    {
                        transactionType = group.Key?.ToString() ?? "Unknown",
                        count = group.Count(),
                        totalAmount = group.Sum(st => st?.TotalAmount ?? 0)
                    })
                    .OrderBy(x => x.transactionType)
                    .ToList();

                // Calculate voided transactions by department
                var voidedTransactionDetails = voidedProducts
                    .GroupBy(vp => vp?.Product?.Department_ID)
                    .Select(group => new
                    {
                        departmentName = departments?.FirstOrDefault(d => d?.Id == group.Key)?.Department_Name ?? "Unknown",
                        count = group.Count(),
                        totalAmount = group.Sum(vp => vp?.Voided_Amount)
                    })
                    .OrderBy(x => x.departmentName)
                    .ToList();

                // Calculate VAT totals with null safety
                var totalVat = netSalesItems
                    .Where(sale => sale?.Product?.VAT_ID != null)
                    .Join(vats ?? new List<Vat>(),
                        sales => sales.Product!.VAT_ID,
                        vat => vat?.Id ?? 0,
                        (sale, vat) => new
                        {
                            Qty = sale?.Product_QTY ?? 0,
                            Vat_Value = vat?.VAT_Value ?? 0,
                            Total_Product_Amount = sale?.Product_Total_Amount ?? 0,
                            vat_Description = vat?.VAT_Description ?? "Unknown",
                            Vat_Amount = (sale?.Product_Total_Amount ?? 0) *
                                ((vat?.VAT_Value ?? 0) == 0 ? 0 : (vat.VAT_Value / 100.0m))
                        })
                    .GroupBy(item => item.Vat_Value)
                    .Select(group => new
                    {
                        Qty = group.Sum(i => i.Qty),
                        Vat_Description = group.FirstOrDefault()?.vat_Description ?? string.Empty,
                        Total_Product_Amount = group.Sum(item => item.Total_Product_Amount),
                        TotalVat = group.Sum(item => item.Vat_Amount)
                    }).ToList();

                // Calculate payout totals with null safety
                var totalPayouts = payoutItems
                    .Where(sales => sales?.SalesPayout_ID != null)
                    .Join(payouts ?? new List<Payout>(),
                        sales => sales.SalesPayout_ID,
                        payout => payout?.Id ?? 0,
                        (sales, payout) => new
                        {
                            Qty = sales?.Product_QTY ?? 0,
                            PayoutDescription = payout?.Payout_Description ?? "Unknown",
                            payoutAmount = sales?.Product_Total_Amount ?? 0
                        })
                    .GroupBy(item => item.PayoutDescription)
                    .Select(group => new
                    {
                        Qty = group.Sum(p => p.Qty),
                        PayoutDescription = group.Key,
                        TotalPayoutAmount = group.Sum(item => item.payoutAmount)
                    }).ToList();

                // Calculate refund and tender totals with null safety
                var safeTransactions = transactions?.Where(t => t != null).ToList() ?? new List<SalesTransaction>();

                var totalCashAmount = new
                {
                    Qty = safeTransactions.Count(x => (x.SaleTransaction_Cash) > 0),
                    Description = "Cash",
                    Total = safeTransactions.Where(x => (x.SaleTransaction_Cash) > 0)
                        .Sum(x => x.SaleTransaction_Cash)
                };

                var totalCardAmount = new
                {
                    Qty = safeTransactions.Count(x => (x.SaleTransaction_Card) > 0),
                    Description = "Card",
                    Total = safeTransactions.Where(x => (x.SaleTransaction_Card) > 0)
                        .Sum(card => card.SaleTransaction_Card)
                };

                var totalChangeAmount = new
                {
                    Qty = safeTransactions.Count(x => (x.SaleTransaction_Change) < 0),
                    Description = "Change",
                    Total = safeTransactions.Where(x => (x.SaleTransaction_Change) < 0)
                        .Sum(x => x.SaleTransaction_Change)
                };

                // Print Department Sales (Alphabetically ordered)
                PrintReportListHeader("Department Sales");
                decimal netTotal = 0;
                foreach (var item in netSalesList)
                {
                    var truncatedDeptName = item.departmentName.Length > nameWidth ? item.departmentName.Substring(0, nameWidth) : item.departmentName;
                    _printer.Append($"{item.qty.ToString().PadRight(qtyWidth)} {truncatedDeptName.PadRight(nameWidth)} {"£" + item.total.ToString().PadRight(priceWidth)}");
                    netTotal += item.total;
                }
                _printer.Append($"{"".PadRight(qtyWidth)} {"Net Total".PadRight(nameWidth)} {"£" + netTotal.ToString().PadRight(priceWidth)}");
                _printer.NewLine();
                _printer.Separator();

                // Print Other Department Sales (Alphabetically ordered)
                if (otherSalesList != null && otherSalesList.Count() > 0)
                {
                    PrintReportListHeader("Other Departments");
                    netTotal = 0;
                    foreach (var item in otherSalesList)
                    {
                        var truncatedDeptName = item.departmentName.Length > nameWidth ? item.departmentName.Substring(0, nameWidth) : item.departmentName;
                        _printer.Append($"{item.qty.ToString().PadRight(qtyWidth)} {truncatedDeptName.PadRight(nameWidth)} {"£" + item.total.ToString().PadRight(priceWidth)}");
                        netTotal += item.total;
                    }
                    _printer.Append($"{"".PadRight(qtyWidth)} {"Net Total".PadRight(nameWidth)} {"£" + netTotal.ToString().PadRight(priceWidth)}");
                    _printer.NewLine();
                    _printer.Separator();
                }

                // Print Unscanned Sales Section
                if (unscannedSalesList != null && unscannedSalesList.Count() > 0)
                {
                    PrintReportListHeader("Unscanned Sales");
                    decimal unscannedTotal = 0;
                    foreach (var item in unscannedSalesList)
                    {
                        var truncatedDeptName = item.departmentName.Length > nameWidth ? item.departmentName.Substring(0, nameWidth) : item.departmentName;
                        _printer.Append($"{item.qty.ToString().PadRight(qtyWidth)} {truncatedDeptName.PadRight(nameWidth)} {"£" + item.total.ToString().PadRight(priceWidth)}");
                        unscannedTotal += item.total;
                    }
                    var truncatedUnscannedLabel = "Unscanned Total".Length > nameWidth ? "Unscanned Total".Substring(0, nameWidth) : "Unscanned Total";
                    _printer.Append($"{"".PadRight(qtyWidth)} {truncatedUnscannedLabel.PadRight(nameWidth)} {"£" + unscannedTotal.ToString().PadRight(priceWidth)}");
                    _printer.NewLine();
                    _printer.Separator();
                }

                // Print Promotion Sales Section
                if (promotionSalesList != null && promotionSalesList.Count() > 0)
                {
                    PrintReportListHeader("Promotion Sales");
                    decimal promotionTotal = 0;
                    foreach (var item in promotionSalesList)
                    {
                        var truncatedDeptName = item.departmentName.Length > nameWidth ? item.departmentName.Substring(0, nameWidth) : item.departmentName;
                        _printer.Append($"{item.qty.ToString().PadRight(qtyWidth)} {truncatedDeptName.PadRight(nameWidth)} {"£" + item.total.ToString().PadRight(priceWidth)}");
                        promotionTotal += item.total;
                    }
                    var truncatedPromotionLabel = "Promotion Total".Length > nameWidth ? "Promotion Total".Substring(0, nameWidth) : "Promotion Total";
                    _printer.Append($"{"".PadRight(qtyWidth)} {truncatedPromotionLabel.PadRight(nameWidth)} {"£" + promotionTotal.ToString().PadRight(priceWidth)}");
                    _printer.NewLine();
                    _printer.Separator();
                }

                // Print Discount Sales Section
                if (discountSalesList != null && discountSalesList.Count() > 0)
                {
                    PrintReportListHeader("Discount Sales");
                    decimal discountTotal = 0;
                    foreach (var item in discountSalesList)
                    {
                        var truncatedDeptName = item.departmentName.Length > nameWidth ? item.departmentName.Substring(0, nameWidth) : item.departmentName;
                        _printer.Append($"{item.qty.ToString().PadRight(qtyWidth)} {truncatedDeptName.PadRight(nameWidth)} {"£" + item.total.ToString().PadRight(priceWidth)}");
                        discountTotal += item.total;
                    }
                    var truncatedDiscountLabel = "Discount Total".Length > nameWidth ? "Discount Total".Substring(0, nameWidth) : "Discount Total";
                    _printer.Append($"{"".PadRight(qtyWidth)} {truncatedDiscountLabel.PadRight(nameWidth)} {"£" + discountTotal.ToString().PadRight(priceWidth)}");
                    _printer.NewLine();
                    _printer.Separator();
                }

                // Print Refund Section
                if (refundDetails != null && refundDetails.Count() > 0)
                {
                    PrintReportListHeader("Refunded Items");
                    decimal refundTotal = 0;
                    foreach (var item in refundDetails)
                    {
                        var truncatedProductName = item.productName.Length > nameWidth ? item.productName.Substring(0, nameWidth) : item.productName;
                        _printer.Append($"{item.qty.ToString().PadRight(qtyWidth)} {truncatedProductName.PadRight(nameWidth)} {"£" + item.total.ToString().PadRight(priceWidth)}");
                        refundTotal += item.total;
                    }
                    var truncatedRefundLabel = "Refund Total".Length > nameWidth ? "Refund Total".Substring(0, nameWidth) : "Refund Total";
                    _printer.Append($"{"".PadRight(qtyWidth)} {truncatedRefundLabel.PadRight(nameWidth)} {"£" + refundTotal.ToString().PadRight(priceWidth)}");
                    _printer.NewLine();
                    _printer.Separator();
                }

                // Print Stock Transactions Section
                if (stockTransactionDetails != null && stockTransactionDetails.Count() > 0)
                {
                    PrintReportListHeader("Stock Transactions");
                    decimal stockTotal = 0;
                    foreach (var item in stockTransactionDetails)
                    {
                        var truncatedTransactionType = item.transactionType.Length > nameWidth ? item.transactionType.Substring(0, nameWidth) : item.transactionType;
                        _printer.Append($"{item.count.ToString().PadRight(qtyWidth)} {truncatedTransactionType.PadRight(nameWidth)} {"£" + item.totalAmount.ToString().PadRight(priceWidth)}");
                        stockTotal += item.totalAmount;
                    }
                    var truncatedStockLabel = "Stock Total".Length > nameWidth ? "Stock Total".Substring(0, nameWidth) : "Stock Total";
                    _printer.Append($"{"".PadRight(qtyWidth)} {truncatedStockLabel.PadRight(nameWidth)} {"£" + stockTotal.ToString().PadRight(priceWidth)}");
                    _printer.NewLine();
                    _printer.Separator();
                }

                // Print Voided Transactions Section
                if (voidedTransactionDetails != null && voidedTransactionDetails.Count() > 0)
                {
                    PrintReportListHeader("Voided Transactions");
                    decimal voidedTotal = 0;
                    foreach (var item in voidedTransactionDetails)
                    {
                        var truncatedDeptName = item.departmentName.Length > nameWidth ? item.departmentName.Substring(0, nameWidth) : item.departmentName;
                        _printer.Append($"{item.count.ToString().PadRight(qtyWidth)} {truncatedDeptName.PadRight(nameWidth)} {"£" + item.totalAmount.ToString().PadRight(priceWidth)}");
                        voidedTotal += item.totalAmount ?? 0;
                    }
                    var truncatedVoidedLabel = "Voided Total".Length > nameWidth ? "Voided Total".Substring(0, nameWidth) : "Voided Total";
                    _printer.Append($"{"".PadRight(qtyWidth)} {truncatedVoidedLabel.PadRight(nameWidth)} {"£" + voidedTotal.ToString().PadRight(priceWidth)}");
                    _printer.NewLine();
                    _printer.Separator();
                }

                // Print Payouts
                if (totalPayouts != null && totalPayouts.Count() > 0)
                {
                    PrintReportListHeader("Payouts");
                    netTotal = 0;
                    foreach (var item in totalPayouts)
                    {
                        var truncatedPayoutDesc = item.PayoutDescription.ToString().Length > nameWidth ? item.PayoutDescription.ToString().Substring(0, nameWidth) : item.PayoutDescription.ToString();
                        _printer.Append($"{item.Qty.ToString().PadRight(qtyWidth)} {truncatedPayoutDesc.PadRight(nameWidth)} {"£" + item.TotalPayoutAmount.ToString().PadRight(priceWidth)}");
                        netTotal += item.TotalPayoutAmount;
                    }
                    payoutAmount = netTotal;
                    _printer.Append($"{totalPayouts.Sum(x => x.Qty).ToString().PadRight(qtyWidth)} {"Net Total".PadRight(nameWidth)} {"£" + netTotal.ToString().PadRight(priceWidth)}");
                    _printer.NewLine();
                    _printer.Separator();
                }

                // Print Tenders
                PrintReportListHeader("Tenders");

                _printer.Append($"{totalCashAmount.Qty.ToString().PadRight(qtyWidth)} {totalCashAmount.Description.ToString().PadRight(nameWidth)} {"£" + totalCashAmount.Total.ToString().PadRight(priceWidth)}");
                _printer.Append($"{totalCardAmount.Qty.ToString().PadRight(qtyWidth)} {totalCardAmount.Description.ToString().PadRight(nameWidth)} {"£" + totalCardAmount.Total.ToString().PadRight(priceWidth)}");
                _printer.Append($"{"".PadRight(qtyWidth)} {"Total Amount".ToString().PadRight(nameWidth)} {"£" + (totalCashAmount.Total + totalCardAmount.Total).ToString().PadRight(priceWidth)}");
                _printer.NewLine();
                _printer.Separator();

                _printer.Append($"{totalChangeAmount.Qty.ToString().PadRight(qtyWidth)} {totalChangeAmount.Description.ToString().PadRight(nameWidth)} {"£" + totalChangeAmount.Total.ToString().PadRight(priceWidth)}");
                _printer.Append($"{"".PadRight(qtyWidth)} {"Pay out".ToString().PadRight(nameWidth)} {"£" + payoutAmount.ToString().PadRight(priceWidth)}");

                // Calculate refund total (negative amounts)
                decimal totalRefundAmount = refundDetails?.Sum(r => r.total) ?? 0;

                // Calculate drawer cash: Cash received + Float - Change given - Payouts + Refunds (since refunds are negative, adding them subtracts from drawer)
                _printer.Append($"{"".PadRight(qtyWidth)} {"Refunds".PadRight(nameWidth)} {"£" + totalRefundAmount.ToString().PadRight(priceWidth)}");
                _printer.Append($"{"".PadRight(qtyWidth)} {"Float Amount".PadRight(nameWidth)} {"£" + floatAmount.ToString().PadRight(priceWidth)}");
                _printer.Append($"{"".PadRight(qtyWidth)} {"Drawer Cash".PadRight(nameWidth)} {"£" + (totalCashAmount.Total
                    + payoutAmount + totalRefundAmount + floatAmount).ToString().PadRight(priceWidth)}");
                _printer.NewLine();
                _printer.Separator();

                // Print VATs
                netTotal = 0;
                PrintReportListHeader("Vats");
                foreach (var item in totalVat)
                {
                    _printer.Append($"{item.Qty.ToString().PadRight(qtyWidth)}{("(" + item.Vat_Description + ")").PadRight(nameWidth)} {("£" + Math.Round(item.TotalVat, 2)).PadRight(priceWidth)}");
                    netTotal += item.TotalVat;
                }
                _printer.Append($"{"".PadRight(qtyWidth)} {"Net Total".ToString().PadRight(nameWidth)} {"£" + Math.Round(netTotal, 2).ToString().PadRight(priceWidth)}");
                _printer.NewLine();
                _printer.Separator();

                // Print Summary
                _printer.Append($"{"".PadRight(qtyWidth)} {"Customer Count".PadRight(nameWidth)} {safeTransactions.Count.ToString().PadRight(priceWidth)}");

                var totalSales = safeTransactions.Sum(x => x.SaleTransaction_Total_Amount);
                var totalPayoutAmount = safeTransactions.Sum(p => p.SaleTransaction_Payout);
                var averageSpent = safeTransactions.Count > 0
                    ? (totalSales + totalPayoutAmount) / safeTransactions.Count
                    : 0;

                _printer.Append($"{"".PadRight(qtyWidth)} {"Average Spent".PadRight(nameWidth)} {"£" + averageSpent.ToString("F2").PadRight(priceWidth)}");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error generating sales report");
                _printer?.Append("Error generating sales report");
                throw;
            }
        }

        private void PrintReportListHeader(string header)
        {
            _printer.AlignCenter();
            _printer.BoldMode(header);
            _printer.AlignLeft();
            _printer.Append($"{"Qty".PadRight(qtyWidth)} {"Description".ToString().PadRight(nameWidth)} {"Total".PadRight(priceWidth)}");
            _printer.Separator();

        }

        //public void PrintStockRefill(List<SalesTransaction> transactions, List<Department> departments)
        //{
        //    _printer.InitializePrint();
        //    _printer.AlignCenter();
        //    _printer.Append("Stock Refill " + DateTime.UtcNow);
        //    _printer.NewLine();
        //    _printer.AlignLeft();

        //    //List<SalesItemTransaction> itemTransactions = transactions.Where(t => t.Is_Refund == false && t.SalesItemTransactions != null).SelectMany(t =>
        //    //t.SalesItemTransactions).ToList().Where(i => i.Stock_Refilled == false && i.Product?.StockroomQuantity > 0).ToList();
        //    List<SalesItemTransaction> itemTransactions = transactions.Where(t => t.SalesItemTransactions != null).SelectMany(t =>
        //    t.SalesItemTransactions).ToList().Where(i => i.Stock_Refilled == false && i.Product?.StockroomQuantity > 0).ToList();

        //    // Group itemTransactions by Product_ID and sum the Product_QTY for each product
        //    var groupedItemTransactions = itemTransactions
        //        .GroupBy(i => i.Product_ID)
        //        .Select(g => new
        //        {
        //            Product_ID = g.Key,
        //            Product = g.First().Product,
        //            Product_QTY = g.Sum(x => x.Product_QTY),
        //            Stock_Refilled = g.All(x => x.Stock_Refilled),
        //            // Add other properties as needed
        //        })
        //        .ToList();

        //    groupedItemTransactions.OrderBy(x => x?.Product?.Department_ID).ThenBy(x => x.Product?.Product_Name);


        //    foreach (var department in departments)
        //    {
        //        int i = 0;
        //        foreach (var item in groupedItemTransactions)
        //        {
        //            if (item?.Product?.Department_ID == department.Department_ID && department.Stock_Refill_Print == true)
        //            {

        //                if (i == 0)
        //                {
        //                    i = 1;
        //                    _printer.Separator();
        //                    _printer.AlignCenter();
        //                    _printer.BoldMode(department.Department_Name);
        //                    _printer.AlignLeft();
        //                    _printer.Append($"{"ReFill".PadRight(priceWidth)} {"Product".PadRight(nameWidth - 3)} {"Available".PadRight(priceWidth)}");
        //                    _printer.Separator();
        //                }
        //                _printer.Append($"{item.Product_QTY.ToString().PadRight(3)} {(item.Product.Product_Name.Length > nameWidth ? item.Product.Product_Name.Substring(0, nameWidth) : item.Product.Product_Name).PadRight(nameWidth + 3)} {item.Product.StockroomQuantity.ToString().PadRight(3)}");


        //            }

        //        }

        //    }
        //    _printer.NewLine();
        //    _printer.NewLine();
        //    _printer.FullPaperCut();
        //    _printer.PrintDocument();

        //}

        public void PrintStockDeliveryList(List<StockOrderModel> stockDelivery, List<Department> departments)
        {

            _printer.InitializePrint();
            _printer.AlignCenter();
            _printer.Append("Stock Delivery " + TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local));
            _printer.NewLine();
            _printer.AlignLeft();

            foreach (var item in departments)
            {
                _printer.Separator();
                _printer.AlignCenter();
                _printer.BoldMode(item.Department_Name);
                _printer.AlignLeft();
                _printer.Append($"{"Product".PadRight(qtyWidth)} {"Qty Available".PadRight(nameWidth)} {"Cases Needed".PadRight(priceWidth)}");
                _printer.Separator();
                var stockDeliveryList = stockDelivery.Where(x => x.Product.Department_ID == item.Id).ToList();
                stockDeliveryList.Sort((x, y) => x.Product.Product_Name.CompareTo(y.Product.Product_Name));
                foreach (var stock in stockDeliveryList)
                {
                    _printer.Append($"{stock.Product.Product_Name.Substring(0, Math.Min(nameWidth, stock.Product.Product_Name.Length)).ToString().PadRight(nameWidth)} {stock.Product.ProductTotalQuantity.ToString().PadRight(priceWidth)} {stock.RequiredCases.ToString().PadRight(priceWidth)}");
                }
            }
            _printer.NewLine();
            _printer.NewLine();
            _printer.FullPaperCut();
            _printer.PrintDocument();
        }


        private void Print()
        {
            try
            {
                if (_printer == null)
                    throw new InvalidOperationException("Printer not initialized");

                _printer.NewLine();
                _printer.FullPaperCut();
                _printer.PrintDocument();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error during print operation");
                throw new InvalidOperationException("Failed to complete print operation", ex);
            }
        }

        public byte[] CutPage()
        {
            try
            {
                var cutCommand = new List<byte>
                {
                    Convert.ToByte(Convert.ToChar(0x1D)),
                    Convert.ToByte('V'),
                    (byte)66,
                    (byte)3
                };
                return cutCommand.ToArray();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error creating cut page command");
                throw new InvalidOperationException("Failed to create cut page command", ex);
            }
        }

        public void OpenDrawer()
        {
            try
            {
                EnsureInitialized();

                if (string.IsNullOrWhiteSpace(_printerModel!.Printer_Name))
                {
                    throw new InvalidOperationException("Printer name is not available");
                }

                _logger?.LogInformation("Opening cash drawer for printer: {PrinterName}", _printerModel.Printer_Name);

                byte[] openDrawerCommand = { 0x1B, 0x70, 0x00, 0x19, 0xFA };

                if (!RawPrinterHelper.SendBytesToPrinter(_printerModel.Printer_Name, openDrawerCommand))
                {
                    throw new InvalidOperationException("Failed to send open drawer command to printer");
                }

                _logger?.LogInformation("Successfully opened cash drawer");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to open cash drawer");
                throw new InvalidOperationException("Failed to open cash drawer", ex);
            }
        }

        /// <summary>
        /// Prints a day-end report
        /// </summary>
        /// <param name="salesData">Sales dashboard data for the day</param>

        /// <summary>
        /// Prints a sales receipt
        /// </summary>
        /// <param name="transaction">Sales transaction</param>
        /// <param name="transactionItems">Sales transaction items</param>
        public Task PrintSalesReceipt(SalesTransaction? transaction, List<SalesItemTransaction>? transactionItems)
        {
            try
            {
                EnsureInitialized();

                if (transaction == null)
                {
                    throw new ArgumentNullException(nameof(transaction), "Transaction cannot be null");
                }

                _logger?.LogInformation("Starting sales receipt printing for transaction {TransactionId}", transaction.Id);

                _printer!.InitializePrint();

                // Clear the receipt builder
                receiptBuilder.Clear();

                // Add business header
                AddBusinessAddress();

                // Add transaction details
                receiptBuilder.AppendLine($"Sales Receipt - Transaction TR{transaction.Id:D6} - {TimeZoneInfo.ConvertTimeFromUtc(transaction.Sale_Start_Date, TimeZoneInfo.Local):dd/MM/yyyy}");
                receiptBuilder.AppendLine($"Time: {TimeZoneInfo.ConvertTimeFromUtc(transaction.Sale_Start_Date, TimeZoneInfo.Local):HH:mm}");
                receiptBuilder.AppendLine(new string('-', maxChar));

                // Add items
                if (transactionItems?.Any() == true)
                {
                    foreach (var item in transactionItems)
                    {
                        if (item?.Product != null)
                        {
                            var qty = item.Product_QTY.ToString().PadRight(qtyWidth);
                            var productName = item.Product.Product_Name ?? "Unknown";
                            var name = productName.Length > nameWidth
                                ? productName.Substring(0, nameWidth)
                                : productName.PadRight(nameWidth);
                            var price = $"£{item.Product_Total_Amount:F2}";

                            receiptBuilder.AppendLine($"{qty} {name} {price.PadLeft(priceWidth)}");

                            if (item.Promotion != null && !string.IsNullOrWhiteSpace(item.Promotion.Promotion_Name))
                            {
                                var promoQty = "".PadRight(qtyWidth);
                                var promoName = "(" + (item.Promotion.Promotion_Name.Length > nameWidth
                                    ? item.Promotion.Promotion_Name.Substring(0, (nameWidth - 2)) + ")"
                                    : (item.Promotion.Promotion_Name + ")").PadRight((nameWidth - 1)));
                                var promoPrice = $"-£{item.Promotional_Discount_Amount:F2}".PadLeft(priceWidth);
                                receiptBuilder.AppendLine($"{promoQty} {promoName} {promoPrice}");
                            }
                        }
                    }
                }

                receiptBuilder.AppendLine(new string('-', maxChar));

                // Add discounts and promotions
                if (transaction.SaleTransaction_Promotion > 0)
                {
                    receiptBuilder.AppendLine($"{"".PadRight(qtyWidth)} {"Promotion:".PadRight(nameWidth)} {$"-£{transaction.SaleTransaction_Promotion:F2}".PadLeft(priceWidth)}");
                }
                if (transaction.SaleTransaction_Discount > 0)
                {
                    receiptBuilder.AppendLine($"{"".PadRight(qtyWidth)} {"Discounts:".PadRight(nameWidth)} {$"-£{transaction.SaleTransaction_Discount:F2}".PadLeft(priceWidth)}");
                }

                // Add total and payment info
                receiptBuilder.AppendLine($"{"".PadRight(qtyWidth)} {"Total:".PadRight(nameWidth)} {$"£{transaction.SaleTransaction_Total_Amount:F2}".PadLeft(priceWidth)}");
                receiptBuilder.AppendLine($"{"".PadRight(qtyWidth)} {"Payment:".PadRight(nameWidth)} {(transaction.SaleTransaction_Card > 0 ? "Card" : "Cash").PadLeft(priceWidth)}");

                receiptBuilder.AppendLine();
                receiptBuilder.AppendLine("Thank you for your purchase!".PadLeft((maxChar + "Thank you for your purchase!".Length) / 2));

                // Append the built receipt to printer and print
                _printer.Append(receiptBuilder.ToString());
                _printer.NewLine();
                _printer.NewLine();
                _printer.FullPaperCut();
                _printer.PrintDocument();

                receiptBuilder.Clear();
                _logger?.LogInformation("Successfully printed sales receipt");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to print sales receipt");
                throw new InvalidOperationException("Failed to print sales receipt", ex);
            }
        }

        public Task PrintCustomSalesReport(List<SalesTransaction>? transactions, List<Department>? departments, List<Vat>? vats, List<Payout>? payouts, DateTime startDate, DateTime endDate, List<StockTransaction> stockTransactions, List<VoidedProduct> voidedProducts, decimal floatAmount = 0)
        {
            try
            {
                EnsureInitialized();

                if (startDate > endDate)
                {
                    throw new ArgumentException("Start date cannot be later than end date");
                }

                _logger?.LogInformation("Starting custom sales report generation for period {StartDate} to {EndDate}",
                    startDate, endDate);

                _printer!.InitializePrint();
                _printer.AlignCenter();
                _printer.Append("Sales Report");
                _printer.NewLine();

                var culture = new System.Globalization.CultureInfo("en-GB");
                _printer.Append($"{TimeZoneInfo.ConvertTimeFromUtc(startDate, TimeZoneInfo.Local).ToString("d", culture)} - {TimeZoneInfo.ConvertTimeFromUtc(endDate, TimeZoneInfo.Local).ToString("d", culture)}");

                _printer.AlignLeft();
                _printer.Separator();
                _printer.NewLine();

                PrintSalesReport(transactions ?? new List<SalesTransaction>(),
                    departments ?? new List<Department>(),
                    vats ?? new List<Vat>(),
                    payouts ?? new List<Payout>(),
                    stockTransactions,
                    voidedProducts,
                    floatAmount);

                Print();

                _logger?.LogInformation("Successfully completed custom sales report");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to print custom sales report for period {StartDate} to {EndDate}",
                    startDate, endDate);
                throw new InvalidOperationException("Failed to print custom sales report", ex);
            }
        }

        public Task PrintEndOfDayReport(DayLog? dayLog, List<SalesTransaction>? transactions, List<Department>? departments, List<Vat>? vats, List<Payout>? payouts, List<StockTransaction> stockTransactions, List<VoidedProduct> voidedProducts)
        {
            try
            {
                EnsureInitialized();

                _logger?.LogInformation("Starting {ReportType} report generation", dayLog.DayLog_End_DateTime != null ? "Day End" : "X-Read");

                _printer!.InitializePrint();
                _printer.AlignCenter();
                _printer.Append(dayLog.DayLog_End_DateTime != null ? "Day End Report" : "X-Read");
                _printer.NewLine();

                var startDate = TimeZoneInfo.ConvertTimeFromUtc(dayLog!.DayLog_Start_DateTime, TimeZoneInfo.Local).ToString("yyyy-MM-dd HH:mm") ?? "Unknown";
                var endDate = dayLog!.DayLog_End_DateTime.HasValue
                    ? TimeZoneInfo.ConvertTimeFromUtc(dayLog.DayLog_End_DateTime.Value, TimeZoneInfo.Local).ToString("yyyy-MM-dd HH:mm")
                    : "Ongoing";
                _printer.Append($"{startDate} - {endDate}");
                _printer.Append($"Opening Balance: {dayLog.Opening_Cash_Amount}");
                _printer.Append($"Closing Balance: {dayLog.Closing_Cash_Amount}");

                _printer.AlignLeft();
                _printer.Separator();
                _printer.NewLine();

                PrintSalesReport(transactions ?? new List<SalesTransaction>(),
                    departments ?? new List<Department>(),
                    vats ?? new List<Vat>(),
                    payouts ?? new List<Payout>(),
                    stockTransactions,
                    voidedProducts,
                    dayLog?.Opening_Cash_Amount ?? 0);

                Print();

                _logger?.LogInformation("Successfully completed {ReportType} report", dayLog.DayLog_End_DateTime != null ? "Day End" : "X-Read");
                return Task.FromResult(true);

            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to print {ReportType} report", dayLog.DayLog_End_DateTime != null ? "Day End" : "X-Read");
                throw new InvalidOperationException($"Failed to print {(dayLog.DayLog_End_DateTime != null ? "Day End" : "X-Read")} report", ex);
            }
        }

        public Task PrintShiftEndReport(Shift? shiftLog, List<SalesTransaction>? transactions, List<Department>? departments, List<Vat>? vats, List<Payout>? payouts, List<StockTransaction> stockTransactions, List<VoidedProduct> voidedProducts)
        {

            try
            {
                EnsureInitialized();

                _logger?.LogInformation("Starting {ReportType} report generation", shiftLog.Shift_End_DateTime != null ? "Shift End" : "Shift Read");

                _printer!.InitializePrint();
                _printer.AlignCenter();
                _printer.Append(shiftLog.Shift_End_DateTime != null ? "Shift End Report" : "Shift Read");
                _printer.NewLine();

                var startDate = TimeZoneInfo.ConvertTimeFromUtc(shiftLog!.Shift_Start_DateTime, TimeZoneInfo.Local).ToString("yyyy-MM-dd HH:mm") ?? "Unknown";
                var endDate = shiftLog!.Shift_End_DateTime.HasValue
                    ? TimeZoneInfo.ConvertTimeFromUtc(shiftLog.Shift_End_DateTime.Value, TimeZoneInfo.Local).ToString("yyyy-MM-dd HH:mm")
                    : "Ongoing";
                _printer.Append($"{startDate} - {endDate}");
                _printer.Append($"Opening Balance: {shiftLog.Opening_Cash_Amount}");
                _printer.Append($"Closing Balance: {shiftLog.Closing_Cash_Amount}");

                _printer.AlignLeft();
                _printer.Separator();
                _printer.NewLine();

                PrintSalesReport(transactions ?? new List<SalesTransaction>(),
                    departments ?? new List<Department>(),
                    vats ?? new List<Vat>(),
                    payouts ?? new List<Payout>(),
                    stockTransactions,
                    voidedProducts,
                    shiftLog?.Opening_Cash_Amount ?? 0);

                Print();

                _logger?.LogInformation("Successfully completed {ReportType} report", shiftLog.Shift_End_DateTime != null ? "Shift End" : "Shift Read");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to print {ReportType} report", shiftLog.Shift_End_DateTime != null ? "Shift End" : "Shift Read");
                throw new InvalidOperationException($"Failed to print {(shiftLog.Shift_End_DateTime != null ? "Shift End" : "Shift Read")} report", ex);
            }

        }

        public Task PrintRefillProductsAsync(List<ProductRefillDTO> refillProducts)
        {
            try
            {
                EnsureInitialized();

                if (refillProducts == null || !refillProducts.Any())
                {
                    _logger?.LogWarning("No refill products provided for printing");
                    return Task.FromResult(true);
                }

                _logger?.LogInformation("Starting to print refill products list with {Count} items", refillProducts.Count);
                _printer!.InitializePrint();

                // Header
                AddBusinessAddress();
                receiptBuilder.AppendLine("REFILL PRODUCTS REPORT");
                receiptBuilder.AppendLine($"Date: {DateTime.Now:dd/MM/yyyy HH:mm}");
                receiptBuilder.AppendLine(new string('-', maxChar));

                // Column headers
                receiptBuilder.AppendLine("Product Name".PadRight((nameWidth - 3)) + "Qty".PadLeft((qtyWidth + 3)) + "Price".PadLeft(priceWidth));
                receiptBuilder.AppendLine(new string('-', maxChar));

                decimal totalValue = 0;
                int totalItems = 0;

                foreach (var item in refillProducts.OrderBy(p => p.Product?.Product_Name))
                {
                    if (item?.Product == null) continue;

                    var productName = TruncateString(item.Product.Product_Name == null ? "Unknown" : item.Product.Product_Selling_Price.ToString() + " " + item.Product.Product_Name, (nameWidth - 3));
                    var qty = item.Refill_QTY.ToString();
                    var price = $"£{item.Product.Product_Selling_Price:F2}";

                    receiptBuilder.AppendLine(productName.PadRight((nameWidth - 3)) + qty.PadLeft((qtyWidth + 3)) + price.PadLeft(priceWidth));

                    totalValue += item.Product.Product_Selling_Price * item.Refill_QTY;
                    totalItems += item.Refill_QTY;
                }

                // Summary
                receiptBuilder.AppendLine(new string('-', maxChar));
                receiptBuilder.AppendLine($"Total Items: {totalItems}");
                receiptBuilder.AppendLine($"Total Value: £{totalValue:F2}");
                receiptBuilder.AppendLine(new string('-', maxChar));

                _printer.Append(receiptBuilder.ToString());
                _printer.NewLine();
                _printer.NewLine();
                _printer.FullPaperCut();
                _printer.PrintDocument();

                receiptBuilder.Clear();
                _logger?.LogInformation("Successfully printed refill products report");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error printing refill products list");
                throw new InvalidOperationException("Failed to print refill products list", ex);
            }
        }

        public Task PrintShortageProductsList(List<ProductShortageDTO> shortageProducts)
        {
            try
            {
                EnsureInitialized();

                if (shortageProducts == null || !shortageProducts.Any())
                {
                    _logger?.LogWarning("No shortage products provided for printing");
                    return Task.FromResult(true);
                }

                _logger?.LogInformation("Starting to print shortage products list with {Count} items", shortageProducts.Count);
                _printer!.InitializePrint();

                // Header
                AddBusinessAddress();
                receiptBuilder.AppendLine("SHORTAGE PRODUCTS REPORT");
                receiptBuilder.AppendLine($"Date: {DateTime.Now:dd/MM/yyyy HH:mm}");
                receiptBuilder.AppendLine(new string('-', maxChar));

                int totalUnitsNeeded = 0;
                int totalCasesNeeded = 0;
                int totalProducts = 0;

                // Group by department and sort departments alphabetically
                var departmentGroups = shortageProducts
                    .Where(p => p?.Product != null)
                    .GroupBy(p => new
                    {
                        DeptId = p.Product.Department_ID,
                        DeptName = p.Product.Department?.Department_Name ?? "Unknown Department"
                    })
                    .OrderBy(g => g.Key.DeptName)
                    .ToList();

                foreach (var deptGroup in departmentGroups)
                {
                    // Department header
                    receiptBuilder.AppendLine();
                    receiptBuilder.AppendLine($"DEPARTMENT: {deptGroup.Key.DeptName.ToUpper()}");
                    receiptBuilder.AppendLine(new string('=', maxChar));

                    // Column headers for this department
                    receiptBuilder.AppendLine("Product Name".PadRight((nameWidth - 3)) + "Units".PadRight((qtyWidth + 3)) + "Cases".PadRight(priceWidth));
                    receiptBuilder.AppendLine(new string('-', maxChar));

                    int deptUnitsNeeded = 0;
                    int deptCasesNeeded = 0;

                    // Sort products alphabetically within department
                    var sortedProducts = deptGroup.OrderBy(p => p.Product.Product_Name).ToList();

                    foreach (var item in sortedProducts)
                    {
                        var productName = TruncateString(item.Product.Product_Name == null ? "Unknown" : item.Product.Product_Selling_Price + " " + item.Product.Product_Name, (nameWidth - 3));
                        var unitsNeeded = item.UnitsNeeded.ToString();
                        var casesNeeded = item.CasesNeeded.ToString();

                        receiptBuilder.AppendLine(productName.PadRight((nameWidth - 3)) + unitsNeeded.PadRight((qtyWidth + 3)) + casesNeeded.PadRight(priceWidth));

                        deptUnitsNeeded += item.UnitsNeeded;
                        deptCasesNeeded += item.CasesNeeded;
                        totalProducts++;
                    }

                    // Department summary
                    receiptBuilder.AppendLine(new string('-', maxChar));
                    receiptBuilder.AppendLine($"Dept Total - Products: {sortedProducts.Count}, Units: {deptUnitsNeeded}, Cases: {deptCasesNeeded}");

                    totalUnitsNeeded += deptUnitsNeeded;
                    totalCasesNeeded += deptCasesNeeded;
                }

                // Overall summary
                receiptBuilder.AppendLine();
                receiptBuilder.AppendLine(new string('=', maxChar));
                receiptBuilder.AppendLine("OVERALL SUMMARY");
                receiptBuilder.AppendLine(new string('=', maxChar));
                receiptBuilder.AppendLine($"Total Departments: {departmentGroups.Count}");
                receiptBuilder.AppendLine($"Total Products: {totalProducts}");
                receiptBuilder.AppendLine($"Total Units Needed: {totalUnitsNeeded}");
                receiptBuilder.AppendLine($"Total Cases Needed: {totalCasesNeeded}");
                receiptBuilder.AppendLine(new string('=', maxChar));

                _printer.Append(receiptBuilder.ToString());
                _printer.NewLine();
                _printer.NewLine();
                _printer.FullPaperCut();
                _printer.PrintDocument();

                receiptBuilder.Clear();
                _logger?.LogInformation("Successfully printed shortage products report");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error printing shortage products list");
                return Task.FromResult(false);
                throw new InvalidOperationException("Failed to print shortage products list", ex);
            }
        }

        public Task PrintExpiryProductsList(List<Product> expiryProducts)
        {
            try
            {
                EnsureInitialized();

                if (expiryProducts == null || !expiryProducts.Any())
                {
                    _logger?.LogWarning("No expiry products provided for printing");
                    return Task.FromResult(true);
                }

                _logger?.LogInformation("Starting to print expiry products list with {Count} items", expiryProducts.Count);
                _printer!.InitializePrint();

                // Header
                AddBusinessAddress();
                receiptBuilder.AppendLine("EXPIRY PRODUCTS REPORT");
                receiptBuilder.AppendLine($"Date: {DateTime.Now:dd/MM/yyyy HH:mm}");
                receiptBuilder.AppendLine(new string('-', maxChar));

                // Column headers
                receiptBuilder.AppendLine("Product Name".PadRight(nameWidth - 5) + "Expiry".PadRight(qtyWidth + 3) + " " + "Qty".PadRight(priceWidth));
                receiptBuilder.AppendLine(new string('-', maxChar));

                decimal totalValue = 0;
                int totalItems = 0;

                foreach (var product in expiryProducts.OrderBy(p => p.Expiry_Date).ThenBy(p => p.Product_Name))
                {
                    if (product == null) continue;

                    var productName = TruncateString(product.Product_Name == null ? "Unknown" : product.Product_Selling_Price.ToString() + " " + product.Product_Name, nameWidth - 5);
                    var expiryDate = product.Expiry_Date.ToString("dd/MM/yy") ?? "N/A";
                    var totalQty = (product.ShelfQuantity + product.StockroomQuantity).ToString();
                    var price = $"£{product.Product_Selling_Price:F2}";

                    receiptBuilder.AppendLine(productName.PadRight(nameWidth - 5) + expiryDate.PadRight(qtyWidth + 5) + " " + totalQty.PadRight(priceWidth));

                    totalValue += product.Product_Selling_Price * (product.ShelfQuantity + product.StockroomQuantity);
                    totalItems += (product.ShelfQuantity + product.StockroomQuantity);
                }

                // Summary
                receiptBuilder.AppendLine(new string('-', maxChar));
                receiptBuilder.AppendLine($"Total Items: {totalItems}");
                receiptBuilder.AppendLine($"Total Value: £{totalValue:F2}");
                receiptBuilder.AppendLine(new string('-', maxChar));

                _printer.Append(receiptBuilder.ToString());
                _printer.NewLine();
                _printer.NewLine();
                _printer.FullPaperCut();
                _printer.PrintDocument();

                receiptBuilder.Clear();
                _logger?.LogInformation("Successfully printed expiry products report");
                return Task.FromResult(true);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error printing expiry products list");
                throw new InvalidOperationException("Failed to print expiry products list", ex);
            }

        }

        /// <summary>
        /// Helper method to truncate strings to fit within specified width
        /// </summary>
        /// <param name="text">Text to truncate</param>
        /// <param name="maxWidth">Maximum width allowed</param>
        /// <returns>Truncated string</returns>
        private string TruncateString(string text, int maxWidth)
        {
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            if (text.Length <= maxWidth)
                return text;

            return text.Substring(0, Math.Max(0, maxWidth - 3)) + "...";
        }
        public Bitmap GenerateLabel(
    string price,
    string productName,
    string barcodeNumber,
    string date,
    int printerWidthPx)     // 58mm = 384px, 80mm = 576px
        {
            // Create canvas
            Bitmap label = new Bitmap(printerWidthPx, 300);
            Graphics g = Graphics.FromImage(label);
            g.Clear(Color.White);

            // Fonts
            Font bigFont = new Font("Arial", 32, FontStyle.Bold);
            Font smallFont = new Font("Arial", 12);
            Font mediumFont = new Font("Arial", 16);

            // Draw Price centered
            g.DrawString(price, bigFont, Brushes.Black,
                new RectangleF(0, 0, printerWidthPx, 50),
                new StringFormat { Alignment = StringAlignment.Center });

            int barcodeAreaWidth = 220;
            int leftAreaX = 10;
            int leftAreaWidth = printerWidthPx - barcodeAreaWidth - 20;
            var sfLeft = new StringFormat { Alignment = StringAlignment.Near, Trimming = StringTrimming.None };
            var nameSize = g.MeasureString(productName, mediumFont, new SizeF(leftAreaWidth, 1000), sfLeft);
            g.DrawString(productName, mediumFont, Brushes.Black,
                new RectangleF(leftAreaX, 60, leftAreaWidth, nameSize.Height), sfLeft);

            Bitmap barcodeBmp;
            var digits = new string((barcodeNumber ?? string.Empty).Where(char.IsDigit).ToArray());
            if (digits.Length == 12 || digits.Length == 13)
            {
                var ean13 = digits.Length == 12 ? digits + CalculateEan13CheckDigit(digits) : digits;
                var writer = new ZXing.Windows.Compatibility.BarcodeWriter()
                {
                    Format = ZXing.BarcodeFormat.EAN_13,
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Height = 80,
                        Width = 220,
                        Margin = 0,
                        PureBarcode = true
                    }
                };
                barcodeBmp = writer.Write(ean13);
            }
            else if (digits.Length == 7 || digits.Length == 8)
            {
                var ean8 = digits.Length == 7 ? digits + CalculateEan8CheckDigit(digits) : digits;
                var writer = new ZXing.Windows.Compatibility.BarcodeWriter()
                {
                    Format = ZXing.BarcodeFormat.EAN_8,
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Height = 80,
                        Width = 220,
                        Margin = 0,
                        PureBarcode = true
                    }
                };
                barcodeBmp = writer.Write(ean8);
            }
            else
            {
                var writer = new ZXing.Windows.Compatibility.BarcodeWriter()
                {
                    Format = ZXing.BarcodeFormat.CODE_128,
                    Options = new ZXing.Common.EncodingOptions
                    {
                        Height = 80,
                        Width = 220,
                        Margin = 0,
                        PureBarcode = true
                    }
                };
                barcodeBmp = writer.Write(barcodeNumber ?? string.Empty);
            }

            // Draw barcode on right side
            g.DrawImage(barcodeBmp, printerWidthPx - barcodeBmp.Width - 10, 60);

            // Draw barcode text below
            var rightAreaX = printerWidthPx - barcodeBmp.Width - 10;
            var rightAreaWidth = barcodeBmp.Width;
            var sfRight = new StringFormat { Alignment = StringAlignment.Far };
            g.DrawString(barcodeNumber, smallFont, Brushes.Black,
                new RectangleF(rightAreaX, 145, rightAreaWidth, smallFont.Height), sfRight);

            // Draw date
            g.DrawString(date, smallFont, Brushes.Black,
                new RectangleF(rightAreaX, 165, rightAreaWidth, smallFont.Height), sfRight);

            g.Dispose();
            return label;
        }

        private static string CalculateEan13CheckDigit(string twelveDigits)
        {
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = twelveDigits[i] - '0';
                sum += (i % 2 == 0) ? digit : digit * 3;
            }
            int mod = sum % 10;
            int check = (10 - mod) % 10;
            return check.ToString();
        }

        private static string CalculateEan8CheckDigit(string sevenDigits)
        {
            int sum = 0;
            for (int i = 0; i < 7; i++)
            {
                int digit = sevenDigits[i] - '0';
                // Weights: 3,1,3,1,3,1,3 from left to right
                sum += digit * ((i % 2 == 0) ? 3 : 1);
            }
            int mod = sum % 10;
            int check = (10 - mod) % 10;
            return check.ToString();
        }
        public void PrintLabel(string printerName, Bitmap bm)
        {
            int width = bm.Width;
            int height = bm.Height;
            int widthBytes = (int)Math.Ceiling(width / 8.0);

            byte xL = (byte)(widthBytes & 0xFF);
            byte xH = (byte)((widthBytes >> 8) & 0xFF);
            byte yL = (byte)(height & 0xFF);
            byte yH = (byte)((height >> 8) & 0xFF);

            byte[] raster = ConvertBitmapToRaster(bm);
            byte[] header = new byte[] { 0x1D, 0x76, 0x30, 0x00, xL, xH, yL, yH };
            byte[] final = new byte[header.Length + raster.Length];
            Buffer.BlockCopy(header, 0, final, 0, header.Length);
            Buffer.BlockCopy(raster, 0, final, header.Length, raster.Length);

            RawPrinterHelper.SendBytesToPrinter(printerName, final);
        }

        private static byte[] ConvertBitmapToRaster(Bitmap bm)
        {
            int width = bm.Width;
            int height = bm.Height;
            int widthBytes = (int)Math.Ceiling(width / 8.0);
            byte[] data = new byte[widthBytes * height];

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * widthBytes;
                int bitPos = 7;
                byte current = 0;
                int byteIndex = rowStart;

                for (int x = 0; x < width; x++)
                {
                    var c = bm.GetPixel(x, y);
                    int luminance = (int)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
                    bool isBlack = luminance < 128;
                    if (isBlack)
                    {
                        current |= (byte)(1 << bitPos);
                    }
                    bitPos--;
                    if (bitPos < 0)
                    {
                        data[byteIndex++] = current;
                        current = 0;
                        bitPos = 7;
                    }
                }
                if (bitPos != 7)
                {
                    data[byteIndex] = current;
                }
            }
            return data;
        }

    }

}





public class RawPrinterHelper
{
    // Structure and API declarations:
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class DOCINFOA
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string pDocName;
        [MarshalAs(UnmanagedType.LPStr)]
        public string pOutputFile;
        [MarshalAs(UnmanagedType.LPStr)]
        public string pDataType;
    }

    [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

    [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool ClosePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

    [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool EndDocPrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool StartPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool EndPagePrinter(IntPtr hPrinter);

    [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

    /// <summary>
    /// Sends raw bytes to the specified printer.
    /// </summary>
    /// <param name="szPrinterName">Name of the printer</param>
    /// <param name="pBytes">Pointer to the bytes to send</param>
    /// <param name="dwCount">Number of bytes to send</param>
    /// <returns>True on success, false on failure</returns>
    public static bool SendBytesToPrinter(string szPrinterName, IntPtr pBytes, Int32 dwCount)
    {
        if (string.IsNullOrWhiteSpace(szPrinterName))
            return false;

        if (pBytes == IntPtr.Zero || dwCount <= 0)
            return false;

        IntPtr hPrinter = IntPtr.Zero;
        bool bSuccess = false;

        try
        {
            var di = new DOCINFOA
            {
                pDocName = "Raw Print Document",
                pDataType = "RAW"
            };

            // Open the printer
            if (!OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
            {
                return false;
            }

            // Start a document
            if (!StartDocPrinter(hPrinter, 1, di))
            {
                return false;
            }

            try
            {
                // Start a page
                if (!StartPagePrinter(hPrinter))
                {
                    return false;
                }

                try
                {
                    // Write the bytes
                    bSuccess = WritePrinter(hPrinter, pBytes, dwCount, out int dwWritten);

                    // Verify all bytes were written
                    if (bSuccess && dwWritten != dwCount)
                    {
                        bSuccess = false;
                    }
                }
                finally
                {
                    EndPagePrinter(hPrinter);
                }
            }
            finally
            {
                EndDocPrinter(hPrinter);
            }
        }
        finally
        {
            if (hPrinter != IntPtr.Zero)
            {
                ClosePrinter(hPrinter);
            }
        }

        return bSuccess;
    }

    /// <summary>
    /// Sends a byte array to the specified printer.
    /// </summary>
    /// <param name="szPrinterName">Name of the printer</param>
    /// <param name="pBytes">Byte array to send</param>
    /// <returns>True on success, false on failure</returns>
    public static bool SendBytesToPrinter(string szPrinterName, byte[] pBytes)
    {
        if (string.IsNullOrWhiteSpace(szPrinterName) || pBytes == null || pBytes.Length == 0)
            return false;

        IntPtr pUnmanagedBytes = IntPtr.Zero;

        try
        {
            // Allocate unmanaged memory for the bytes
            pUnmanagedBytes = Marshal.AllocCoTaskMem(pBytes.Length);

            // Copy the managed byte array into the unmanaged array
            Marshal.Copy(pBytes, 0, pUnmanagedBytes, pBytes.Length);

            // Send the unmanaged bytes to the printer
            return SendBytesToPrinter(szPrinterName, pUnmanagedBytes, pBytes.Length);
        }
        finally
        {
            // Always free the unmanaged memory
            if (pUnmanagedBytes != IntPtr.Zero)
            {
                Marshal.FreeCoTaskMem(pUnmanagedBytes);
            }
        }
    }

}

