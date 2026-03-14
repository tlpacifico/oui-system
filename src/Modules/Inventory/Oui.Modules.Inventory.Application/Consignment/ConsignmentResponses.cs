namespace Oui.Modules.Inventory.Application.Consignment;

public sealed record ReceptionSupplierInfo(
    Guid ExternalId,
    string Name,
    string Initial);

public sealed record ReceptionListItemResponse(
    Guid ExternalId,
    ReceptionSupplierInfo Supplier,
    DateTime ReceptionDate,
    int ItemCount,
    string Status,
    int EvaluatedCount,
    int AcceptedCount,
    int RejectedCount,
    string? Notes,
    DateTime CreatedOn);

public sealed record ReceptionDetailResponse(
    Guid ExternalId,
    ReceptionSupplierInfo Supplier,
    DateTime ReceptionDate,
    int ItemCount,
    string Status,
    string? Notes,
    int EvaluatedCount,
    int AcceptedCount,
    int RejectedCount,
    DateTime? EvaluatedAt,
    string? EvaluatedBy,
    DateTime CreatedOn,
    string? CreatedBy);

public sealed record ReceptionPagedResult(
    List<ReceptionListItemResponse> Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);

public sealed record EvaluationItemResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string Brand,
    string Size,
    string Color,
    string Condition,
    decimal EvaluatedPrice,
    decimal CommissionPercentage,
    string Status,
    bool IsRejected,
    string? RejectionReason,
    DateTime CreatedOn);
