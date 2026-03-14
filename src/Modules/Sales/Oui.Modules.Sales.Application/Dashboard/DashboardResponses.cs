namespace Oui.Modules.Sales.Application.Dashboard;

public sealed record DashboardResponse(
    DashboardSalesToday SalesToday,
    DashboardSalesMonth SalesMonth,
    DashboardInventory Inventory,
    DashboardPendingSettlements PendingSettlements,
    List<DashboardTopSellingItem> TopSellingItems,
    DashboardAlerts Alerts,
    List<DashboardChartPoint> SalesChart);

public sealed record DashboardSalesToday(int Count, decimal Revenue, decimal AverageTicket);
public sealed record DashboardSalesMonth(int Count, decimal Revenue, decimal AverageTicket, decimal GrowthPercent);
public sealed record DashboardInventory(int TotalItems, decimal TotalValue, int StagnantCount);
public sealed record DashboardPendingSettlements(decimal TotalAmount, int SuppliersCount);
public sealed record DashboardTopSellingItem(string Name, string Brand, decimal FinalPrice, DateTime SoldDate);
public sealed record DashboardOpenRegister(string OperatorName, DateTime OpenedAt, int SalesCount);
public sealed record DashboardAlerts(
    int ExpiringConsignments,
    int StagnantItems30,
    int StagnantItems45,
    int StagnantItems60,
    List<DashboardOpenRegister> OpenRegisters);
public sealed record DashboardChartPoint(string Date, decimal Revenue, int Count);
