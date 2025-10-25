# Stock Order Generation System - Implementation Guide

## Overview

The Stock Order Generation System analyzes yearly sales patterns to predict inventory needs for the next 7 days. It automatically calculates required case quantities based on historical sales data while excluding non-stock items like payouts, services, and generic department sales.

## Key Features

### 🎯 Intelligent Analysis
- **Yearly Sales Data**: Analyzes 365 days of sales history for accurate patterns
- **7-Day Forecast**: Calculates inventory needs for the next week
- **Smart Filtering**: Excludes payouts, services, and generic departments
- **Active Products Only**: Only considers active, non-deleted products

### 📊 Comprehensive Reporting
- **Detailed Product Information**: Name, barcode, department, current stock
- **Sales Analytics**: Yearly sales, daily averages, transaction frequency
- **Cost Estimation**: Calculates estimated order costs based on product costs
- **Case Calculations**: Determines optimal case quantities needed

## System Components

### 1. StockOrderGenerationService
**Location**: `EposDataHandler/Services/StockOrderGenerationService.cs`

**Key Methods**:
- `GetDetailedStockOrderReportAsync()`: Generates comprehensive stock order report
- `GetStockOrdersAsync()`: Returns basic stock order list
- `CalculateRequiredCases()`: Determines case quantities needed

### 2. StockOrderModel
**Location**: `EposDataHandler/Models/StockOrderModel.cs`

**Properties**:
- `Product`: Product information with department and pricing
- `TotalSold`: Total units sold in analysis period
- `TotalSalesPerDay`: Average daily sales rate
- `RequiredCases`: Number of cases needed for forecast period
- `AverageTransactionDays`: Average days between transactions

### 3. StockOrderReport
**Location**: `EposDataHandler/Models/StockOrderModel.cs`

**Properties**:
- `StockOrders`: List of products requiring restocking
- `TotalProductsAnalyzed`: Count of products needing attention
- `TotalCasesRequired`: Sum of all cases needed
- `EstimatedOrderValue`: Total estimated cost
- `GeneratedDate`: Report generation timestamp
- `AnalysisPeriodDays`: Days of data analyzed (365)
- `ForecastPeriodDays`: Days forecasted (7)

## Integration with InventoryManagement.razor

### Service Injection
```csharp
@inject DataHandlerLibrary.Services.StockOrderGenerationService StockOrderService
```

### Generate Stock Order Method
```csharp
private async Task GenerateStockOrder()
{
    try
    {
        // Generate stock orders based on yearly sales patterns for next 7 days
        var stockOrderReport = await StockOrderService.GetDetailedStockOrderReportAsync();
        stockOrders = stockOrderReport.StockOrders.ToList();
        
        if (stockOrders.Any())
        {
            await JSRuntime.InvokeVoidAsync("alert", 
                $"Stock order generated successfully! Found {stockOrderReport.TotalProductsAnalyzed} products requiring restocking for the next 7 days based on yearly sales patterns.");
        }
        else
        {
            await JSRuntime.InvokeVoidAsync("alert", 
                "Stock analysis completed. All active products have sufficient stock for the next 7 days based on sales patterns.");
        }
        
        StateHasChanged();
    }
    catch (Exception ex)
    {
        await JSRuntime.InvokeVoidAsync("alert", $"Failed to generate stock order: {ex.Message}");
    }
}
```

### Helper Methods
```csharp
private int GetCurrentStock(Product? product)
{
    if (product == null) return 0;
    return (product.ShopQuantity ?? 0) + (product.StockroomQuantity ?? 0);
}

private decimal GetEstimatedCost(StockOrderModel order)
{
    if (order?.Product == null) return 0;
    var costPerCase = (order.Product.Product_Cost ?? 0) * (order.Product.Case_Size ?? 1);
    return costPerCase * order.RequiredCases;
}
```

## Service Registration

### ServiceConfiguration.cs
```csharp
// Register Stock Order Generation Service
services.AddScoped<StockOrderGenerationService>();
```

## Usage Instructions

### 1. Accessing the Feature
1. Navigate to **Inventory Management** page
2. Click on **Stock Orders** tab
3. Click **Generate Order** button

### 2. Understanding the Results

**Table Columns Explained**:
- **Product**: Product name and barcode
- **Department**: Product department (excludes generic/service departments)
- **Current Stock**: Combined shop and stockroom quantities
- **Yearly Sales**: Total units sold in past 365 days
- **Daily Avg**: Average units sold per day
- **7-Day Forecast**: Predicted sales for next 7 days
- **Cases Needed**: Number of cases required to meet forecast
- **Case Size**: Units per case
- **Est. Cost**: Estimated cost for the required cases

### 3. Analysis Logic

**Inclusion Criteria**:
- ✅ Active products (`Is_Activated = true`)
- ✅ Non-deleted products (`Is_Deleted = false`)
- ✅ Products with sales history
- ✅ Products where current stock < 7-day forecast

**Exclusion Criteria**:
- ❌ Payouts
- ❌ Services
- ❌ Generic department sales
- ❌ Inactive or deleted products
- ❌ Products with sufficient stock

### 4. Calculation Method

```
Daily Average = Total Yearly Sales ÷ 365
7-Day Forecast = Daily Average × 7
Cases Needed = ⌈(7-Day Forecast - Current Stock) ÷ Case Size⌉
Estimated Cost = Cases Needed × (Product Cost × Case Size)
```

## Advanced Features

### StockOrderGeneration.razor Component
**Location**: `EposDataHandler/Components/StockOrderGeneration.razor`

A dedicated component with enhanced features:
- **Summary Cards**: Visual overview of analysis results
- **Search and Filter**: Find specific products quickly
- **Export to CSV**: Download results for external analysis
- **Print Functionality**: Print orders for physical reference
- **Sorting Options**: Sort by cases needed, sales, name, or department
- **Priority Indicators**: Visual priority levels (High/Medium/Low)

### Using the Dedicated Component
```html
@page "/stock-orders"
<!-- Component provides full-featured stock order management -->
```

## Error Handling

The system includes comprehensive error handling:
- **Database Connection Issues**: Graceful fallback with user notification
- **Invalid Data**: Skips problematic records and continues processing
- **Calculation Errors**: Safe defaults and error logging
- **UI Feedback**: Clear success/error messages to users

## Performance Considerations

- **Efficient Queries**: Uses optimized database queries with proper includes
- **Async Operations**: All database operations are asynchronous
- **Memory Management**: Processes data in chunks for large datasets
- **Caching**: Results can be cached for repeated access

## Customization Options

### Adjusting Analysis Period
Modify the `AnalysisPeriodDays` in `StockOrderGenerationService.cs`:
```csharp
private const int AnalysisPeriodDays = 365; // Change to desired days
```

### Adjusting Forecast Period
Modify the `ForecastPeriodDays`:
```csharp
private const int ForecastPeriodDays = 7; // Change to desired days
```

### Excluding Additional Departments
Add department names to the exclusion list:
```csharp
private readonly string[] ExcludedDepartments = { "Payouts", "Services", "Generic", "YourCustomDepartment" };
```

## Troubleshooting

### Common Issues

1. **No Results Generated**
   - Check if products have sales history
   - Verify products are active and not deleted
   - Ensure database contains sales transaction data

2. **Incorrect Calculations**
   - Verify product case sizes are set correctly
   - Check product costs are populated
   - Ensure current stock quantities are accurate

3. **Performance Issues**
   - Consider adding database indexes on frequently queried columns
   - Implement result caching for repeated requests
   - Optimize database queries if needed

### Debug Information

Enable detailed logging in `StockOrderGenerationService.cs` to track:
- Products being analyzed
- Sales data retrieval
- Calculation steps
- Exclusion reasons

## Future Enhancements

- **Seasonal Adjustments**: Account for seasonal sales patterns
- **Supplier Integration**: Direct order placement with suppliers
- **Automated Scheduling**: Automatic order generation on schedules
- **Machine Learning**: Improved forecasting with ML algorithms
- **Multi-location Support**: Cross-location stock optimization

This system provides a robust foundation for intelligent inventory management based on actual sales patterns and business requirements.