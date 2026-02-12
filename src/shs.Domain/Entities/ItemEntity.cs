using shs.Domain.Enums;
using shs.Domain.Interfaces;

namespace shs.Domain.Entities;

public class ItemEntity : EntityWithIdAuditable<long>, IHaveSoftDelete
{
    // Identification
    public string IdentificationNumber { get; set; } = string.Empty; // M202602-0001
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Classification
    public long BrandId { get; set; }
    public long? CategoryId { get; set; }
    public string Size { get; set; } = string.Empty; // XS, S, M, L, XL, XXL
    public string Color { get; set; } = string.Empty;
    public string? Composition { get; set; } // Fabric composition
    public ItemCondition Condition { get; set; }

    // Pricing
    public decimal EvaluatedPrice { get; set; } // Selling price
    public decimal? CostPrice { get; set; } // Only for own-purchase items
    public decimal? FinalSalePrice { get; set; } // Actual sale price (after discount)

    // Status & Origin
    public ItemStatus Status { get; set; }
    public AcquisitionType AcquisitionType { get; set; }
    public ItemOrigin Origin { get; set; }

    // Consignment-specific fields
    public long? SupplierId { get; set; }
    public long? ReceptionId { get; set; }
    public decimal CommissionPercentage { get; set; } = 50m; // Default 50%
    public decimal? CommissionAmount { get; set; } // Calculated on sale

    // Rejection (if item was rejected during evaluation)
    public bool IsRejected { get; set; }
    public string? RejectionReason { get; set; }

    // Sales info
    public long? SaleId { get; set; }
    public DateTime? SoldAt { get; set; }

    // Stock tracking
    public int DaysInStock { get; set; } // Calculated field, updated daily

    public bool IsDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DeletedOn { get; set; }

    // Navigation properties
    public BrandEntity Brand { get; set; } = null!;
    public CategoryEntity? Category { get; set; }
    public SupplierEntity? Supplier { get; set; }
    public ReceptionEntity? Reception { get; set; }
    public SaleEntity? Sale { get; set; }
    public ICollection<TagEntity> Tags { get; set; } = new List<TagEntity>();
    public ICollection<ItemPhotoEntity> Photos { get; set; } = new List<ItemPhotoEntity>();
}
