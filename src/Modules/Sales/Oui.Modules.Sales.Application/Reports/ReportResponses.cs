namespace Oui.Modules.Sales.Application.Reports;

public sealed record SalesReportResponse(
    decimal Revenue,
    int SalesCount,
    decimal AvgTicket,
    List<TopBrandResult> TopBrands,
    List<TopCategoryResult> TopCategories,
    Dictionary<string, PaymentBreakdownItem> PaymentBreakdown,
    List<DailySalesChartItem> DailySalesChart,
    PreviousPeriodComparison PreviousPeriodComparison);

public sealed record TopBrandResult(string BrandName, decimal Revenue, int Count);
public sealed record TopCategoryResult(string? CategoryName, decimal Revenue, int Count);
public sealed record PaymentBreakdownItem(int Count, decimal Total);
public sealed record DailySalesChartItem(string Date, decimal Revenue, int Count);
public sealed record PreviousPeriodComparison(double PercentChange, decimal PreviousRevenue);

public sealed record InventoryReportResponse(
    int TotalItems,
    decimal TotalValue,
    AgingDistribution AgingDistribution,
    double SellThroughRate,
    List<SellThroughByBrandItem> SellThroughByBrand,
    List<StagnantItemInfo> StagnantItemsList);

public sealed record AgingDistribution(int Days0_15, int Days15_30, int Days30_45, int Days45_60, int Days60Plus);

public sealed record SellThroughByBrandItem(
    string BrandName,
    int InStock,
    int Sold,
    double SellThroughRate);

public sealed record StagnantItemInfo(
    long Id,
    Guid ExternalId,
    string BrandName,
    string? CategoryName,
    decimal EvaluatedPrice,
    int DaysInStock);

public sealed record SuppliersReportResponse(
    ReportPeriod Period,
    List<SupplierRankingItem> Ranking);

public sealed record ReportPeriod(DateTime Start, DateTime End);

public sealed record SupplierRankingItem(
    long Id,
    Guid ExternalId,
    string Name,
    string Initial,
    decimal Revenue,
    int SoldCount,
    int ReturnedCount,
    double ReturnRate,
    decimal PendingAmount,
    double AvgDaysToSell);

public sealed record FinanceReportResponse(
    ReportPeriod Period,
    decimal GrossRevenue,
    decimal CommissionRevenue,
    decimal PendingSettlements,
    decimal PaidSettlements,
    decimal ProjectedCashflow);
