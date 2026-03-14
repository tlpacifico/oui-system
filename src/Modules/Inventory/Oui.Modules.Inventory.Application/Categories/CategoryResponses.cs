namespace Oui.Modules.Inventory.Application.Categories;

public sealed record CategoryListResponse(
    Guid ExternalId,
    string Name,
    string? Description,
    CategoryParentInfo? ParentCategory,
    int SubCategoryCount,
    int ItemCount,
    DateTime CreatedOn);

public sealed record CategoryDetailResponse(
    Guid ExternalId,
    string Name,
    string? Description,
    CategoryParentInfo? ParentCategory,
    List<CategoryChildInfo> SubCategories,
    int ItemCount,
    DateTime CreatedOn,
    string? CreatedBy,
    DateTime? UpdatedOn,
    string? UpdatedBy);

public sealed record CategoryParentInfo(Guid ExternalId, string Name);

public sealed record CategoryChildInfo(Guid ExternalId, string Name);
