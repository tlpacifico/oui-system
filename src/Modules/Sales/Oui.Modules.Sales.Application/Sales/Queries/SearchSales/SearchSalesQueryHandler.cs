using Microsoft.EntityFrameworkCore;
using Oui.Modules.Sales.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Enums;
using shs.Domain.Results;

namespace Oui.Modules.Sales.Application.Sales.Queries.SearchSales;

internal sealed class SearchSalesQueryHandler(SalesDbContext salesDb)
    : IQueryHandler<SearchSalesQuery, SalesPagedResult>
{
    public async Task<Result<SalesPagedResult>> Handle(
        SearchSalesQuery request, CancellationToken cancellationToken)
    {
        var query = salesDb.Sales
            .Include(s => s.CashRegister)
            .Include(s => s.Items)
            .Include(s => s.Payments)
            .AsQueryable();

        if (request.DateFrom.HasValue)
            query = query.Where(s => s.SaleDate >= request.DateFrom.Value);

        if (request.DateTo.HasValue)
            query = query.Where(s => s.SaleDate < request.DateTo.Value.AddDays(1));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.ToLower();
            query = query.Where(s => s.SaleNumber.ToLower().Contains(term)
                                     || s.CashRegister.OperatorName.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var sales = await query
            .OrderByDescending(s => s.SaleDate)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(s => new SaleListItem(
                s.ExternalId,
                s.SaleNumber,
                s.SaleDate,
                s.TotalAmount,
                s.Items.Count,
                s.Status.ToString(),
                string.Join(", ", s.Payments.Select(p => GetPaymentLabel(p.PaymentMethod)))))
            .ToListAsync(cancellationToken);

        return new SalesPagedResult(
            sales,
            totalCount,
            request.Page,
            request.PageSize,
            (int)Math.Ceiling(totalCount / (double)request.PageSize));
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
