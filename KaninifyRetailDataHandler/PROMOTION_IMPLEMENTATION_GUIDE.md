# Promotion System Implementation Guide

## Overview

This guide explains how to effectively apply promotions when adding products to the basket in the EposDataHandler system. The promotion system has been completely redesigned to provide robust, flexible, and accurate promotion handling.

## Key Features

### 1. Automatic Promotion Application
- Promotions are automatically applied when products are added to the basket
- All active promotions are evaluated for each basket operation
- Promotions are recalculated when items are removed or quantities change

### 2. Supported Promotion Types
- **Discount Promotions**: Percentage or fixed amount discounts with optional minimum spend
- **Buy X Get X Free**: BOGO and similar promotions
- **MultiBuy Promotions**: Special pricing for buying multiple quantities (e.g., 3 for £10)
- **Bundle Promotions**: Special pricing for buying specific product combinations
- **Extensible**: Easy to add new promotion types

### 3. Smart Promotion Logic
- Promotions are applied in order of type for consistency
- Free items are properly tracked and managed
- Prevents negative totals and handles edge cases

## Implementation Details

### CheckoutService Updates

The `CheckoutService` has been enhanced with the following key methods:

#### Core Methods

```csharp
// Add product with automatic promotion application
public async Task AddProductToBasketAsync(SalesBasket basket, Product product, bool isGeneric)

// Remove product and recalculate promotions
public async Task RemoveProductFromBasketAsync(SalesBasket basket, int productId, int quantityToRemove = 1)

// Manually refresh all promotions
public async Task RefreshPromotionsAsync(SalesBasket basket)

// Get detailed information about applied promotions
public async Task<List<AppliedPromotionInfo>> GetAppliedPromotionsAsync(SalesBasket basket)
```

#### Utility Methods

```csharp
// Recalculate basket totals
public void RecalculateBasketTotals(SalesBasket basket)

// Get total discount amount
public decimal GetTotalDiscountAmount(SalesBasket basket)
```

### How Promotions Are Applied

1. **Reset Phase**: All items are reset to original prices
2. **Fetch Active Promotions**: Get all currently active promotions from the database
3. **Apply Promotions**: Each promotion is evaluated and applied to eligible items
4. **Calculate Totals**: Final basket totals are calculated
5. **Handle Refunds**: Special handling for refund scenarios

## Usage Examples

### Adding Products to Basket

```csharp
// Old synchronous method (deprecated)
// checkoutService.AddProductToBasket(basket, product, false);

// New asynchronous method with promotion support
await checkoutService.AddProductToBasketAsync(basket, product, false);
```

### Removing Products from Basket

```csharp
// Remove 1 quantity of a product
await checkoutService.RemoveProductFromBasketAsync(basket, productId, 1);

// Remove entire product from basket
await checkoutService.RemoveProductFromBasketAsync(basket, productId, int.MaxValue);
```

### Getting Promotion Information

```csharp
// Get list of applied promotions
var appliedPromotions = await checkoutService.GetAppliedPromotionsAsync(basket);

foreach (var promotion in appliedPromotions)
{
    Console.WriteLine($"Promotion: {promotion.PromotionName}");
    Console.WriteLine($"Type: {promotion.PromotionType}");
    Console.WriteLine($"Discount: ${promotion.DiscountAmount:F2}");
    Console.WriteLine($"Products: {string.Join(", ", promotion.AffectedProducts)}");
}
```

### Manual Promotion Refresh

```csharp
// Useful when promotions are updated during a transaction
await checkoutService.RefreshPromotionsAsync(basket);
```

## Promotion Types in Detail

### Discount Promotions

**Percentage Discount**:
- Set `Discount_Percentage` > 0
- Applied as: `(item_total * percentage) / 100`
- Optional: Set `Minimum_Spend_Amount` for minimum spend requirement

**Fixed Amount Discount**:
- Set `Discount_Amount` > 0
- Applied as: `discount_amount * quantity`
- Optional: Set `Minimum_Spend_Amount` for minimum spend requirement

**Minimum Spend Logic**:
- If `Minimum_Spend_Amount` > 0, the total basket value must meet this threshold
- Discount is only applied if the minimum spend is met
- Calculated before any discounts are applied

### Buy X Get X Free Promotions

**Configuration**:
- Set `Buy_Quantity` (number of items to buy)
- Set `Free_Quantity` (number of free items per buy set)

**Logic**:
- Calculates how many complete sets qualify for free items
- Adds free items as separate basket entries with $0.00 price
- Updates original item pricing to reflect the promotion

### MultiBuy Promotions

**Configuration**:
- Set `Buy_Quantity` (minimum quantity required)
- Set either `Discount_Amount` (fixed price for the set) or `Discount_Percentage` (percentage off the set)

**Examples**:
- "Buy 3 for £10": `Buy_Quantity = 3`, `Discount_Amount = 10.00`
- "Buy 2 get 20% off": `Buy_Quantity = 2`, `Discount_Percentage = 20`

**Logic**:
- Calculates how many complete sets qualify for the special pricing
- Applies special pricing to complete sets
- Remaining items are charged at regular price

### Bundle Promotions

**Configuration**:
- Add multiple products to the promotion by setting their Promotion_Id
- Set `Discount_Amount` (fixed bundle price) or `Discount_Percentage` (percentage off bundle)

**Examples**:
- "Burger + Fries + Drink for £8.99": Add all 3 products, set `Discount_Amount = 8.99`
- "Buy any 3 items get 15% off": Add eligible products, set `Discount_Percentage = 15`

**Logic**:
- Requires at least 1 of each product in the bundle to be in the basket
- Calculates how many complete bundles can be formed
- Applies bundle pricing to complete bundles
- Remaining quantities are charged at regular price
- Discount is distributed proportionally across bundle items

## Best Practices

### 1. Always Use Async Methods
```csharp
// ✅ Correct
await checkoutService.AddProductToBasketAsync(basket, product, false);

// ❌ Avoid (deprecated)
checkoutService.AddProductToBasket(basket, product, false);
```

### 2. Handle Promotion Information
```csharp
// Show customers what promotions are applied
var totalDiscount = checkoutService.GetTotalDiscountAmount(basket);
if (totalDiscount > 0)
{
    Console.WriteLine($"You saved: ${totalDiscount:F2}");
}
```

### 3. Refresh Promotions When Needed
```csharp
// If promotions change during the day
await checkoutService.RefreshPromotionsAsync(basket);
```

### 4. Error Handling
```csharp
try
{
    await checkoutService.AddProductToBasketAsync(basket, product, false);
}
catch (Exception ex)
{
    // Handle promotion application errors
    logger.LogError(ex, "Error applying promotions to basket");
}
```

## Database Requirements

Ensure the following are properly configured:

1. **PromotionServices** is registered in dependency injection
2. **Active promotions** exist in the database with:
   - Valid start and end dates
   - Associated products via direct Product.Promotion_Id relationship
   - Proper promotion type configuration

## Migration from Old System

If you're updating existing code:

1. Replace `AddProductToBasket` calls with `AddProductToBasketAsync`
2. Add proper async/await handling
3. Update any custom promotion logic to use the new system
4. Test thoroughly with various promotion scenarios

## Troubleshooting

### Promotions Not Applying
1. Check if promotions are active (start/end dates)
2. Verify products are associated with promotions via Product.Promotion_Id
3. Ensure PromotionServices is properly injected
4. For discount promotions: Check if minimum spend requirement is met
5. For bundle promotions: Ensure all required products are in the basket

### Incorrect Discount Calculations
1. Verify promotion configuration (percentages, amounts, quantities)
2. Check for multiple overlapping promotions
3. Review promotion type settings
4. For MultiBuy: Ensure Buy_Quantity is set correctly
5. For Bundle: Verify all products are properly associated with the promotion

### MultiBuy Promotions Not Working
1. Check that Buy_Quantity is set and > 1
2. Verify either Discount_Amount or Discount_Percentage is set
3. Ensure customer has enough quantity to qualify for the promotion

### Bundle Promotions Not Working
1. Verify all required products have their Promotion_Id set to the bundle promotion
2. Check that customer has at least 1 of each required product
3. Ensure bundle pricing (Discount_Amount or Discount_Percentage) is configured
4. Verify products are not marked as deleted or inactive

### Minimum Spend Discounts Not Applying
1. Check that Minimum_Spend_Amount is set correctly
2. Verify basket total meets the minimum spend requirement
3. Ensure calculation is done before any discounts are applied

### Performance Issues
1. Ensure database indexes on promotion tables
2. Consider caching active promotions for high-volume scenarios
3. Monitor promotion query performance

## Future Enhancements

The system is designed to be extensible. Future promotion types can be added by:

1. Adding new `PromotionType` enum values
2. Implementing logic in `ApplyPromotionToBasketAsync` method
3. Adding validation rules in `PromotionServices.ValidateAsync`

This promotion system provides a solid foundation for complex promotional scenarios while maintaining performance and accuracy.