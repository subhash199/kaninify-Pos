using DataHandlerLibrary.Models;
using DataHandlerLibrary.Services;
using EntityFrameworkDatabaseLibrary.Models;
using EposRetail.Models;
using EposRetail.Services;
using Microsoft.AspNetCore.Components;
using NetTopologySuite.Index.HPRtree;

public class CheckoutService
{
    private readonly StockRefillServices _stockRefillServices;
    private readonly UserSessionService _userSessionService;
    private readonly ProductServices _productServices;
    private readonly SalesTransactionServices _salesTransactionServices;
    private readonly SalesItemTransactionServices _salesItemTransactionServices;
    private readonly GeneralServices _generalServices;
    private readonly PromotionServices _promotionServices;
    private readonly IServiceScopeFactory ServiceScopeFactory;

    public CheckoutService(ProductServices productServices,
                          SalesTransactionServices salesTransactionServices,
                          GeneralServices generalServices,
                          SalesItemTransactionServices salesItemTransactionServices,
                          PromotionServices promotionServices,
                          IServiceScopeFactory serviceScopeFactory,
                          StockRefillServices stockRefillServices,
                          UserSessionService userSessionService)
    {
        _productServices = productServices;
        _salesTransactionServices = salesTransactionServices;
        _generalServices = generalServices;
        _salesItemTransactionServices = salesItemTransactionServices;
        _promotionServices = promotionServices;
        ServiceScopeFactory = serviceScopeFactory;
        _stockRefillServices = stockRefillServices;
        _userSessionService = userSessionService;
    }

    public async Task<Product?> GetProductByBarcodeAsync(string barcode)
    {
        return await _productServices.GetProductByBarcode(barcode, false, true);
    }

    public async Task SaveTransactionAsync(SalesTransaction transaction)
    {
        // Save the main transaction first
        var salesItems = transaction.SalesItemTransactions?.ToList() ?? new List<SalesItemTransaction>();
        transaction.SalesItemTransactions = null; // Detach to avoid circular reference issues
        await _salesTransactionServices.AddAsync(transaction);

        //Save all sales item transactions in bulk if they exist
        if (salesItems?.Any() == true)
        {

            // Set the transaction ID for all sales items
            foreach (var salesItem in salesItems)
            {
                salesItem.SaleTransaction_ID = transaction.Id;
                salesItem.SalesTransaction = null;
                salesItem.Created_By_Id = transaction.Created_By_Id;
                salesItem.Last_Modified_By_Id = transaction.Last_Modified_By_Id;
                salesItem.Product = null; // Detach product to avoid circular reference issues
                salesItem.SalesPayout = null; // Detach payout to avoid circular reference issues
                salesItem.Promotion = null; // Detach promotion to avoid circular reference issues
            }

            // Save all sales items in a single bulk operation
            await _salesItemTransactionServices.AddRangeAsync(salesItems);

            // Create StockRefill objects for each sales item transaction
            await CreateStockRefillRecordsAsync(salesItems, transaction);

            // Update product quantities based on the transaction
            await UpdateProductQuantitiesAsync(salesItems);
        }
    }

    /// <summary>
    /// Updates product quantities based on sales item transactions
    /// Reduces quantities for purchases, adds quantities back for refunds
    /// </summary>
    /// <param name="salesItems">List of sales item transactions</param>
    /// <param name="isRefund">True if this is a refund transaction, false for regular sale</param>
    public async Task UpdateProductQuantitiesAsync(List<SalesItemTransaction> salesItems)
    {
        if (salesItems?.Any() != true) return;

        List<Product> productsToUpdate = new List<Product>();

        foreach (var salesItem in salesItems)
        {
            var product = await _productServices.GetByIdAsync(salesItem.Product_ID);
            if (product == null) continue;

            int quantityChange = salesItem.Product_QTY;

            if (salesItem.SalesItemTransactionType == SalesItemTransactionType.Refund)
            {
                product.ShelfQuantity += quantityChange;
            }
            else
            {
                // For regular sales, reduce the quantity
                // Prioritize taking from shelf first, then stockroom
                int takeFromShelf = Math.Min(quantityChange, product.ShelfQuantity);
                int takeFromStockroom = quantityChange - takeFromShelf;

                product.ShelfQuantity -= takeFromShelf;
                product.StockroomQuantity = Math.Max(0, product.StockroomQuantity - takeFromStockroom);
                product.Promotion = null; // Detach promotion to avoid circular reference issues
                product.Department = null; // Detach department to avoid circular reference issues
            }
            productsToUpdate.Add(product);
            // Update the product in the database
        }
        if (productsToUpdate.Any())
        {
            await _productServices.BulkUpdateAsync(productsToUpdate);
        }
    }

    public decimal CalculateGrandTotal(SalesBasket basket)
    {
        if (basket?.SalesItemsList?.Count == 0) return 0;

        var grandTotal = basket.Transaction.SaleTransaction_Total_Amount;
        var totalPaid = basket.Transaction.SaleTransaction_Cash + basket.Transaction.SaleTransaction_Card;
        return grandTotal - totalPaid;
    }

    public async Task AddProductToBasketAsync(SalesBasket basket, Product product, SalesItemTransactionType transactionType, int? payoutId)
    {
        basket.SalesItemsList ??= new List<SalesItemTransaction>();

        var existingItem = basket.SalesItemsList.FirstOrDefault(x => x.Product_ID == product.Id && x.SalesItemTransactionType == transactionType);

        if (existingItem != null && transactionType != SalesItemTransactionType.Misc
            && transactionType != SalesItemTransactionType.Service && transactionType != SalesItemTransactionType.Payout)
        {
            existingItem.Product_QTY += 1;
        }
        else
        {
            basket.SalesItemsList.Add(new SalesItemTransaction
            {
                Product_ID = product.Id,
                Product = product,
                Product_QTY = 1,
                Product_Amount = transactionType == SalesItemTransactionType.Refund ? -(product.Product_Selling_Price) : product.Product_Selling_Price,
                Product_Total_Amount = transactionType == SalesItemTransactionType.Refund ? -(product.Product_Selling_Price) : product.Product_Selling_Price,
                Product_Total_Amount_Before_Discount = transactionType == SalesItemTransactionType.Refund ? -(product.Product_Selling_Price) : product.Product_Selling_Price,
                SalesPayout_ID = payoutId,
                SalesItemTransactionType = transactionType

            });
        }

        // Apply promotions to the entire basket
        await ApplyPromotionsToBasketAsync(basket);

        basket.Transaction.SaleTransaction_Total_Amount = basket.SalesItemsList.Sum(s => s.Product_Total_Amount);
    }

    /// <summary>
    /// Applies all active promotions to the basket items
    /// </summary>
    public async Task ApplyPromotionsToBasketAsync(SalesBasket basket)
    {
        if (basket?.SalesItemsList?.Any() != true) return;

        // Reset all items to original prices before applying promotions
        foreach (var item in basket.SalesItemsList)
        {
            item.Product_Total_Amount_Before_Discount = item.Product_QTY *
                (item.SalesItemTransactionType == SalesItemTransactionType.Refund ? -(item.Product?.Product_Selling_Price) : item.Product?.Product_Selling_Price) ?? 0;
            item.Product_Total_Amount = item.Product_Total_Amount_Before_Discount;
            item.Product_Amount = (item.SalesItemTransactionType == SalesItemTransactionType.Refund ? -(item.Product?.Product_Selling_Price) :
                item.Product?.Product_Selling_Price) ?? 0;
        }

        // Apply promotions directly from products in the basket
        var processedPromotions = new HashSet<int>();

        // Create a copy of the list to avoid collection modification during enumeration
        var itemsToProcess = basket.SalesItemsList.ToList();

        foreach (var item in itemsToProcess)
        {
            if (item.Product?.Promotion_Id.HasValue == true &&
                !processedPromotions.Contains(item.Product.Promotion_Id.Value))
            {
                var promotion = await _promotionServices.GetByIdAsync(item.Product.Promotion_Id.Value);
                if (promotion != null && IsPromotionActive(promotion))
                {
                    await ApplyPromotionToBasketAsync(basket, promotion);
                    processedPromotions.Add(item.Product.Promotion_Id.Value);
                }
            }
        }
    }

    /// <summary>
    /// Applies a specific promotion to the basket
    /// </summary>
    private async Task ApplyPromotionToBasketAsync(SalesBasket basket, Promotion promotion)
    {
        // Get products in basket that have this promotion assigned
        var eligibleItems = basket.SalesItemsList
            .Where(item => item.Product?.Promotion_Id == promotion.Id && item.SalesItemTransactionType == SalesItemTransactionType.Sale || item.SalesItemTransactionType
            == SalesItemTransactionType.Refund)
            .ToList();

        if (!eligibleItems.Any()) return;

        switch (promotion.Promotion_Type)
        {
            case PromotionType.Discount:
                ApplyDiscountPromotion(basket, eligibleItems, promotion);
                break;

            case PromotionType.BuyXGetXFree:
                ApplyBuyXGetXFreePromotion(basket, eligibleItems, promotion);
                break;

            case PromotionType.MultiBuy:
                ApplyMultiBuyPromotion(eligibleItems, promotion);
                break;

            case PromotionType.BundleBuy:
                await ApplyBundleBuyPromotion(basket, promotion);
                break;

            default:
                break;
        }

        // Make promotion totals negative for refund items
        foreach (var item in eligibleItems)
        {
            if (item.SalesItemTransactionType == SalesItemTransactionType.Refund)
            {
                item.Product_Total_Amount = -Math.Abs(item.Product_Total_Amount);
            }
        }
    }

    /// <summary>
    /// Applies discount promotion (percentage or fixed amount) with minimum spend check
    /// </summary>
    private void ApplyDiscountPromotion(SalesBasket basket, List<SalesItemTransaction> eligibleItems, Promotion promotion)
    {
        // Check minimum spend requirement if specified
        if (promotion.Minimum_Spend_Amount > 0)
        {
            decimal currentTotal = basket.SalesItemsList.Sum(item => item.Product_Total_Amount_Before_Discount);
            if (currentTotal < promotion.Minimum_Spend_Amount)
            {
                return; // Don't apply discount if minimum spend not met
            }
        }

        foreach (var item in eligibleItems)
        {
            item.Promotion = promotion;
            item.Promotion_ID = promotion.Id;

            decimal discountAmount = 0;

            if (promotion.Discount_Percentage > 0)
            {
                // Percentage discount
                discountAmount = (item.Product_Total_Amount_Before_Discount * promotion.Discount_Percentage ?? 0) / 100;
            }
            else if (promotion.Discount_Amount > 0)
            {
                // Fixed amount discount per item
                discountAmount = (promotion.Discount_Amount ?? 0) * item.Product_QTY;
            }

            // Apply discount but ensure total doesn't go below 0
            item.Product_Total_Amount = discountAmount;


        }
    }

    /// <summary>
    /// Applies Buy X Get X Free promotion
    /// Increases the product quantity to include free items but only charges for the purchased quantity
    /// </summary>
    private void ApplyBuyXGetXFreePromotion(SalesBasket basket, List<SalesItemTransaction> eligibleItems, Promotion promotion)
    {
        foreach (var item in eligibleItems)
        {
            item.Promotion = promotion;
            item.Promotion_ID = promotion.Id;
            if (item.Product_QTY >= promotion.Buy_Quantity)
            {
                // Calculate how many complete promotion sets we have
                int promotionSets = item.Product_QTY / (promotion.Buy_Quantity + (promotion.Free_Quantity ?? 0));
                int remainingItems = item.Product_QTY % (promotion.Buy_Quantity + (promotion.Free_Quantity ?? 0));

                // Calculate chargeable items from complete sets
                int chargeableFromSets = promotionSets * promotion.Buy_Quantity;

                // For remaining items, charge for items up to Buy_Quantity, rest are free
                int chargeableFromRemaining = Math.Min(remainingItems, promotion.Buy_Quantity);

                // Total chargeable quantity
                int totalChargeableQuantity = chargeableFromSets + chargeableFromRemaining;

                // Update the total amount (charge only for chargeable items)
                item.Product_Total_Amount = totalChargeableQuantity * item.Product.Product_Selling_Price;

            }

        }
    }



    /// <summary>
    /// Applies MultiBuy promotion (e.g., buy 3 for £10, buy 2 get 1 at 50% off)
    /// Uses Buy_Quantity for the required quantity and Discount_Amount for the special price
    /// </summary>
    private void ApplyMultiBuyPromotion(List<SalesItemTransaction> eligibleItems, Promotion promotion)
    {
        foreach (var item in eligibleItems)
        {
            item.Promotion = promotion;
            item.Promotion_ID = promotion.Id;
            if (item.Product_QTY >= promotion.Buy_Quantity)
            {
                // Calculate how many multi-buy sets the customer gets
                int multiBuySets = item.Product_QTY / promotion.Buy_Quantity;
                int remainingItems = item.Product_QTY % promotion.Buy_Quantity;

                decimal multiBuyTotal = 0;

                if (promotion.Discount_Amount > 0)
                {
                    // Fixed price for the multi-buy set (e.g., 3 for £10)
                    multiBuyTotal = multiBuySets * (promotion.Discount_Amount ?? 0);
                }
                else if (promotion.Discount_Percentage > 0)
                {
                    // Percentage discount on the multi-buy set
                    decimal originalSetPrice = promotion.Buy_Quantity * item.Product.Product_Selling_Price;
                    decimal discountedSetPrice = originalSetPrice * (1 - (promotion.Discount_Percentage ?? 0) / 100);
                    multiBuyTotal = multiBuySets * discountedSetPrice;
                }

                // Add remaining items at regular price
                decimal remainingTotal = remainingItems * item.Product.Product_Selling_Price;

                // Update the total amount
                item.Product_Total_Amount = multiBuyTotal + remainingTotal;
            }
            // If quantity is below minimum, promotion data remains cleared (from reset phase)
        }
    }

    /// <summary>
    /// Applies BundleBuy promotion (buy specific combination of products for a special price)
    /// For the new direct relationship, BundleBuy works on products that share the same promotion
    /// All products with the same promotion_id form a bundle
    /// Supports both multi-product bundles and same-product quantity bundles (e.g., "Any 3 for £2")
    /// </summary>
    private async Task ApplyBundleBuyPromotion(SalesBasket basket, Promotion promotion)
    {
        // Get all products in basket that have this promotion assigned
        var bundleItemsInBasket = basket.SalesItemsList
            .Where(item => item.Product?.Promotion_Id == promotion.Id)
            .ToList();

        if (!bundleItemsInBasket.Any()) return;

        // Check if this is a same-product bundle (quantity-based) or multi-product bundle
        bool isSameProductBundle = bundleItemsInBasket.Count == 1;
        int requiredBundleQuantity = promotion.Buy_Quantity;

        if (isSameProductBundle)
        {
            // Handle same-product bundle (e.g., "Any 3 for £2")
            var item = bundleItemsInBasket.First();

            if (item.Product_QTY < requiredBundleQuantity) return;

            int completeBundles = item.Product_QTY / requiredBundleQuantity;
            int bundleQuantities = completeBundles * requiredBundleQuantity;
            int remainingQuantity = item.Product_QTY - bundleQuantities;

            // Apply bundle pricing
            decimal bundlePrice = promotion.Discount_Amount ?? 0;
            decimal originalUnitPrice = item.Product.Product_Selling_Price;
            decimal originalBundlePrice = originalUnitPrice * requiredBundleQuantity;

            // If no bundle price is set, apply percentage discount
            if (bundlePrice == 0 && promotion.Discount_Percentage > 0)
            {
                bundlePrice = originalBundlePrice * (1 - (promotion.Discount_Percentage ?? 0) / 100);
            }

            // Calculate total: bundle price for bundle quantities + regular price for remaining
            decimal bundleTotal = bundlePrice * completeBundles;
            decimal remainingTotal = remainingQuantity * originalUnitPrice;

            item.Promotion = promotion;
            item.Promotion_ID = promotion.Id;
            item.Product_Total_Amount = bundleTotal + remainingTotal;
        }
        else
        {
            // Handle multi-product bundle (original logic)
            if (bundleItemsInBasket.Count < 2) return; // Multi-product bundle needs at least 2 different products

            // For multi-product bundles, we need at least 1 of each product type
            // Calculate how many complete bundles we can make (minimum quantity of all items)
            int completeBundles = bundleItemsInBasket.Min(item => item.Product_QTY);

            if (completeBundles == 0) return;

            // Apply bundle pricing
            decimal bundlePrice = promotion.Discount_Amount ?? 0;
            decimal originalBundlePrice = bundleItemsInBasket.Sum(item => item.Product.Product_Selling_Price);

            // If no bundle price is set, apply percentage discount to the bundle
            if (bundlePrice == 0 && promotion.Discount_Percentage > 0)
            {
                bundlePrice = originalBundlePrice * (1 - (promotion.Discount_Percentage ?? 0) / 100);
            }

            // Calculate the discount per item in the bundle
            decimal totalDiscount = (originalBundlePrice - bundlePrice) * completeBundles;
            decimal discountPerItem = totalDiscount / bundleItemsInBasket.Count;

            foreach (var basketItem in bundleItemsInBasket)
            {
                basketItem.Promotion = promotion;
                basketItem.Promotion_ID = promotion.Id;
                // Apply discount to bundle quantities only
                int bundleQuantity = completeBundles;
                int remainingQuantity = basketItem.Product_QTY - bundleQuantity;

                // Calculate new total: discounted bundle items + regular price remaining items
                decimal bundleItemTotal = (basketItem.Product.Product_Selling_Price * bundleQuantity) - discountPerItem;
                decimal remainingItemTotal = remainingQuantity * basketItem.Product.Product_Selling_Price;

                basketItem.Product_Total_Amount = bundleItemTotal + remainingItemTotal;
            }
        }
    }

    /// <summary>
    /// Recalculates the basket totals after promotions are applied
    /// </summary>
    public void RecalculateBasketTotals(SalesBasket basket)
    {
        if (basket?.SalesItemsList?.Any() != true) return;

        basket.Transaction.SaleTransaction_Total_Amount = basket.SalesItemsList.Sum(s => s.Product_Total_Amount);
    }

    /// <summary>
    /// Gets the total discount amount applied to the basket
    /// </summary>
    public decimal GetTotalDiscountAmount(SalesBasket basket)
    {
        if (basket?.SalesItemsList?.Any() != true) return 0;

        return basket.SalesItemsList.Sum(item =>
            item.Product_Total_Amount_Before_Discount - item.Product_Total_Amount);
    }

    /// <summary>
    /// Removes a product from the basket and recalculates promotions
    /// </summary>
    public async Task RemoveProductFromBasketAsync(SalesBasket basket, int productId, int quantityToRemove = 1)
    {
        if (basket?.SalesItemsList?.Any() != true) return;

        var itemToRemove = basket.SalesItemsList.FirstOrDefault(x => x.Product_ID == productId);
        if (itemToRemove == null) return;

        if (itemToRemove.Product_QTY <= quantityToRemove)
        {
            // Remove the entire item
            basket.SalesItemsList.Remove(itemToRemove);
        }
        else
        {
            // Reduce quantity
            itemToRemove.Product_QTY -= quantityToRemove;
        }

        // Note: With the new unified approach, free items are included in the main product quantity
        // so no separate free item removal is needed

        // Reapply promotions to the entire basket
        await ApplyPromotionsToBasketAsync(basket);


        foreach (var item in basket.SalesItemsList)
        {
            if (item.SalesItemTransactionType == SalesItemTransactionType.Refund)
            {
                item.Product_Total_Amount = -Math.Abs(item.Product_Total_Amount);
                item.Product_Total_Amount_Before_Discount = -Math.Abs(item.Product_Total_Amount_Before_Discount);
            }

        }

        basket.Transaction.SaleTransaction_Total_Amount = basket.SalesItemsList.Sum(s => s.Product_Total_Amount);
    }

    /// <summary>
    /// Manually refreshes all promotions on the basket (useful when promotions change)
    /// </summary>
    public async Task RefreshPromotionsAsync(SalesBasket basket)
    {
        if (basket?.SalesItemsList?.Any() != true) return;

        await ApplyPromotionsToBasketAsync(basket);


        foreach (var item in basket.SalesItemsList)
        {
            if (item.SalesItemTransactionType == SalesItemTransactionType.Refund)
            {
                item.Product_Total_Amount = -Math.Abs(item.Product_Total_Amount);
                item.Product_Total_Amount_Before_Discount = -Math.Abs(item.Product_Total_Amount_Before_Discount);
            }
        }

        basket.Transaction.SaleTransaction_Total_Amount = basket.SalesItemsList.Sum(s => s.Product_Total_Amount);
    }

    /// <summary>
    /// Gets detailed promotion information applied to the basket
    /// </summary>
    public async Task<List<AppliedPromotionInfo>> GetAppliedPromotionsAsync(SalesBasket basket)
    {
        var appliedPromotions = new List<AppliedPromotionInfo>();

        if (basket?.SalesItemsList?.Any() != true) return appliedPromotions;

        // Group items by their promotion
        var promotionGroups = basket.SalesItemsList
            .Where(item => item.Product?.Promotion_Id.HasValue == true)
            .GroupBy(item => item.Product.Promotion_Id.Value);

        foreach (var group in promotionGroups)
        {
            var promotion = await _promotionServices.GetByIdAsync(group.Key);
            if (promotion != null && IsPromotionActive(promotion))
            {
                var eligibleItems = group.ToList();
                decimal totalDiscount = eligibleItems.Sum(item =>
                    item.Product_Total_Amount_Before_Discount - item.Product_Total_Amount);

                if (totalDiscount > 0)
                {
                    appliedPromotions.Add(new AppliedPromotionInfo
                    {
                        PromotionName = promotion.Promotion_Name,
                        PromotionType = promotion.Promotion_Type,
                        DiscountAmount = totalDiscount,
                        AffectedProducts = eligibleItems.Select(i => i.Product.Product_Name).ToList()
                    });
                }
            }
        }

        return appliedPromotions;
    }

    /// <summary>
    /// Checks if a promotion is currently active
    /// </summary>
    private bool IsPromotionActive(Promotion promotion)
    {
        var now = DateTime.UtcNow;
        return promotion.Is_Active &&
               promotion.Start_Date <= now &&
               promotion.End_Date >= now;
    }
    /// <summary>
    /// Creates StockRefill records for sales item transactions that require refilling
    /// </summary>
    /// <param name="salesItems">List of sales item transactions</param>
    /// <param name="transaction">The parent sales transaction</param>
    private async Task CreateStockRefillRecordsAsync(List<SalesItemTransaction> salesItems, SalesTransaction transaction)
    {
        if (salesItems?.Any() != true) return;

        var stockRefillsToCreate = new List<StockRefill>();
        var currentUserId = transaction.Created_By_Id; // Use the transaction creator as the refill user

        foreach (var salesItem in salesItems)
        {
            // Only create refill records for items that actually reduce stock (positive quantities)

            if (salesItem.Product_QTY > 0 && salesItem.SalesItemTransactionType == SalesItemTransactionType.Sale)
            {
                var product = await _productServices.GetByIdAsync(salesItem.Product_ID);
                if (product.ShelfQuantity > 0 && product.StockroomQuantity == 0)
                {
                    continue;
                }
                var stockRefill = new StockRefill
                {
                    SaleTransaction_Item_ID = salesItem.Id,
                    Shift_ID = _userSessionService.GetCurrentShiftId(),
                    DayLog_ID = _userSessionService.GetCurrentDayLogId(),
                    Refill_Quantity = salesItem.Product_QTY, // Quantity that needs to be refilled
                    Quantity_Refilled = 0, // Initially no quantity has been refilled
                    Stock_Refilled = false, // Initially not refilled
                    Date_Created = DateTime.Now.ToUniversalTime(),
                    Last_Modified = DateTime.Now.ToUniversalTime(),
                    Created_By_ID = currentUserId,
                    Last_Modified_By_ID = currentUserId
                };

                stockRefillsToCreate.Add(stockRefill);
            }

        }

        // Save all stock refill records in bulk
        if (stockRefillsToCreate.Any())
        {
            await _stockRefillServices.AddRangeAsync(stockRefillsToCreate);
        }
    }
}

/// <summary>
/// Information about promotions applied to the basket
/// </summary>
public class AppliedPromotionInfo
{
    public string PromotionName { get; set; }
    public PromotionType PromotionType { get; set; }
    public decimal DiscountAmount { get; set; }
    public List<string> AffectedProducts { get; set; } = new List<string>();
}


