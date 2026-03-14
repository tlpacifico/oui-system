using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Suppliers.Commands.UpdateSupplier;

internal sealed class UpdateSupplierCommandHandler(InventoryDbContext db)
    : ICommandHandler<UpdateSupplierCommand, SupplierDetailResponse>
{
    public async Task<Result<SupplierDetailResponse>> Handle(
        UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await db.Suppliers
            .FirstOrDefaultAsync(s => s.ExternalId == request.ExternalId, cancellationToken);

        if (supplier is null)
            return Result.Failure<SupplierDetailResponse>(SupplierErrors.NotFound);

        var initialUpper = request.Initial.Trim().ToUpper();
        var initialExists = await db.Suppliers
            .AnyAsync(s => s.Initial.ToUpper() == initialUpper && s.Id != supplier.Id, cancellationToken);

        if (initialExists)
            return Result.Failure<SupplierDetailResponse>(SupplierErrors.InitialAlreadyExists);

        var emailLower = request.Email.Trim().ToLower();
        var emailExists = await db.Suppliers
            .AnyAsync(s => s.Email.ToLower() == emailLower && s.Id != supplier.Id, cancellationToken);

        if (emailExists)
            return Result.Failure<SupplierDetailResponse>(SupplierErrors.EmailAlreadyExists);

        if (!string.IsNullOrWhiteSpace(request.TaxNumber))
        {
            var nif = request.TaxNumber.Trim();
            var nifExists = await db.Suppliers
                .AnyAsync(s => s.TaxNumber != null && s.TaxNumber == nif && s.Id != supplier.Id, cancellationToken);

            if (nifExists)
                return Result.Failure<SupplierDetailResponse>(SupplierErrors.NifAlreadyExists);
        }

        supplier.Name = request.Name.Trim();
        supplier.Email = emailLower;
        supplier.PhoneNumber = request.PhoneNumber.Trim();
        supplier.TaxNumber = request.TaxNumber?.Trim();
        supplier.Initial = initialUpper;
        supplier.Notes = request.Notes?.Trim();
        supplier.CreditPercentageInStore = request.CreditPercentageInStore ?? supplier.CreditPercentageInStore;
        supplier.CashRedemptionPercentage = request.CashRedemptionPercentage ?? supplier.CashRedemptionPercentage;
        supplier.UpdatedOn = DateTime.UtcNow;
        supplier.UpdatedBy = "system";

        await db.SaveChangesAsync(cancellationToken);

        var itemCount = await db.Items.CountAsync(i => i.SupplierId == supplier.Id && !i.IsDeleted, cancellationToken);

        return new SupplierDetailResponse(
            supplier.Id,
            supplier.ExternalId,
            supplier.Name,
            supplier.Email,
            supplier.PhoneNumber,
            supplier.TaxNumber,
            supplier.Initial,
            supplier.Notes,
            supplier.CreditPercentageInStore,
            supplier.CashRedemptionPercentage,
            itemCount,
            supplier.CreatedOn,
            supplier.CreatedBy,
            supplier.UpdatedOn,
            supplier.UpdatedBy);
    }
}
