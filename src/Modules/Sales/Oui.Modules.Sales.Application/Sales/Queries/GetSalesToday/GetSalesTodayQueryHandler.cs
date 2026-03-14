using Microsoft.EntityFrameworkCore;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Sales.Queries.GetSalesToday;

internal sealed class GetSalesTodayQueryHandler(SalesDbContext salesDb)
    : IQueryHandler<GetSalesTodayQuery, TodaySalesResponse>
{
    public async Task<Result<TodaySalesResponse>> Handle(
        GetSalesTodayQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var todaySales = await salesDb.Sales
            .Where(s => s.SaleDate >= today && s.SaleDate < tomorrow && s.Status == SaleStatus.Active)
            .Include(s => s.Payments)
            .Include(s => s.Items)
            .ToListAsync(cancellationToken);

        var salesCount = todaySales.Count;
        var totalRevenue = todaySales.Sum(s => s.TotalAmount);
        var averageTicket = salesCount > 0 ? totalRevenue / salesCount : 0;
        var totalItems = todaySales.Sum(s => s.Items.Count);

        var byPaymentMethod = todaySales
            .SelectMany(s => s.Payments)
            .GroupBy(p => p.PaymentMethod)
            .ToDictionary(
                g => g.Key.ToString(),
                g => new PaymentMethodSummary(g.Count(), g.Sum(p => p.Amount)));

        var recentSales = todaySales
            .OrderByDescending(s => s.SaleDate)
            .Take(10)
            .Select(s => new SaleListItem(
                s.ExternalId,
                s.SaleNumber,
                s.SaleDate,
                s.TotalAmount,
                s.Items.Count,
                s.Status.ToString(),
                string.Join(", ", s.Payments.Select(p => GetPaymentLabel(p.PaymentMethod)))))
            .ToList();

        return new TodaySalesResponse(
            salesCount,
            totalRevenue,
            averageTicket,
            totalItems,
            byPaymentMethod,
            recentSales);
    }

    private static string GetPaymentLabel(PaymentMethodType method) => method switch
    {
        PaymentMethodType.Cash => "Dinheiro",
        PaymentMethodType.CreditCard => "Cartão Crédito",
        PaymentMethodType.DebitCard => "Cartão Débito",
        PaymentMethodType.PIX => "PIX",
        PaymentMethodType.StoreCredit => "Crédito Loja",
        _ => method.ToString()
    };
}
