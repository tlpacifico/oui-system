namespace Oui.Modules.Inventory.Application.SupplierReturns;

public sealed record ReturnSupplierInfo(
    Guid ExternalId,
    string Name,
    string Initial);

public sealed record ReturnableItemResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string Brand,
    string Size,
    string Color,
    string Condition,
    decimal EvaluatedPrice,
    string Status,
    bool IsRejected,
    int DaysInStock,
    string? PrimaryPhotoUrl,
    DateTime CreatedOn);

public sealed record ReturnItemResponse(
    Guid ExternalId,
    string IdentificationNumber,
    string Name,
    string Brand,
    string Size,
    string Color,
    string Condition,
    decimal EvaluatedPrice,
    bool IsRejected);

public sealed record SupplierReturnListItemResponse(
    Guid ExternalId,
    ReturnSupplierInfo Supplier,
    DateTime ReturnDate,
    int ItemCount,
    string? Notes,
    DateTime CreatedOn);

public sealed record SupplierReturnDetailResponse(
    Guid ExternalId,
    ReturnSupplierInfo Supplier,
    DateTime ReturnDate,
    int ItemCount,
    string? Notes,
    DateTime CreatedOn,
    string? CreatedBy,
    List<ReturnItemResponse> Items);

public sealed record SupplierReturnPagedResult(
    List<SupplierReturnListItemResponse> Data,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
