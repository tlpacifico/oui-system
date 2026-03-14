using Oui.Modules.Inventory.Application.Items;

namespace Oui.Modules.Inventory.Application.Suppliers;

public sealed record SupplierListResponse(
    long Id,
    Guid ExternalId,
    string Name,
    string Email,
    string PhoneNumber,
    string? TaxNumber,
    string Initial,
    decimal CreditPercentageInStore,
    decimal CashRedemptionPercentage,
    int ItemCount,
    DateTime CreatedOn);

public sealed record SupplierDetailResponse(
    long Id,
    Guid ExternalId,
    string Name,
    string Email,
    string PhoneNumber,
    string? TaxNumber,
    string Initial,
    string? Notes,
    decimal CreditPercentageInStore,
    decimal CashRedemptionPercentage,
    int ItemCount,
    DateTime CreatedOn,
    string? CreatedBy,
    DateTime? UpdatedOn,
    string? UpdatedBy);

public sealed record SupplierItemResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string Brand,
    string Size,
    decimal EvaluatedPrice,
    string Status,
    string Condition,
    int DaysInStock,
    DateTime CreatedOn);

public sealed record SupplierReceptionResponse(
    Guid ExternalId,
    DateTime ReceptionDate,
    int ItemCount,
    string Status,
    int EvaluatedCount,
    int AcceptedCount,
    int RejectedCount,
    string? Notes,
    DateTime CreatedOn);
