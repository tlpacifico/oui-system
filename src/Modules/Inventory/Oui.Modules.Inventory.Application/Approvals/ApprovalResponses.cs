namespace Oui.Modules.Inventory.Application.Approvals;

public sealed record ApprovalDetailsResponse(
    string SupplierName,
    DateTime ReceptionDate,
    string ReceptionRef,
    List<ApprovalItemResponse> Items,
    decimal TotalValue,
    DateTime ExpiresAt);

public sealed record ApprovalItemResponse(
    string IdentificationNumber,
    string Name,
    string Brand,
    string Size,
    string Color,
    string Condition,
    decimal EvaluatedPrice,
    decimal CommissionPercentage);
