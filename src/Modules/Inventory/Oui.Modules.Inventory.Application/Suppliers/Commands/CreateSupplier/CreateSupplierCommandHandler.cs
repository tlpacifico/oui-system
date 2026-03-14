using Microsoft.EntityFrameworkCore;
using Oui.Modules.Inventory.Infrastructure;
using shs.Application.Messaging;
using shs.Domain.Entities;
using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Suppliers.Commands.CreateSupplier;

internal sealed class CreateSupplierCommandHandler(InventoryDbContext db)
    : ICommandHandler<CreateSupplierCommand, SupplierDetailResponse>
{
    public async Task<Result<SupplierDetailResponse>> Handle(
        CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var initialUpper = request.Initial.Trim().ToUpper();
        var initialExists = await db.Suppliers
            .AnyAsync(s => s.Initial.ToUpper() == initialUpper, cancellationToken);

        if (initialExists)
            return Result.Failure<SupplierDetailResponse>(SupplierErrors.InitialAlreadyExists);

        var emailLower = request.Email.Trim().ToLower();
        var emailExists = await db.Suppliers
            .AnyAsync(s => s.Email.ToLower() == emailLower, cancellationToken);

        if (emailExists)
            return Result.Failure<SupplierDetailResponse>(SupplierErrors.EmailAlreadyExists);

        if (!string.IsNullOrWhiteSpace(request.TaxNumber))
        {
            var nif = request.TaxNumber.Trim();
            var nifExists = await db.Suppliers
                .AnyAsync(s => s.TaxNumber != null && s.TaxNumber == nif, cancellationToken);

            if (nifExists)
                return Result.Failure<SupplierDetailResponse>(SupplierErrors.NifAlreadyExists);
        }

        var supplier = new SupplierEntity
        {
            ExternalId = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Email = emailLower,
            PhoneNumber = request.PhoneNumber.Trim(),
            TaxNumber = request.TaxNumber?.Trim(),
            Initial = initialUpper,
            Notes = request.Notes?.Trim(),
            CreditPercentageInStore = request.CreditPercentageInStore ?? 50m,
            CashRedemptionPercentage = request.CashRedemptionPercentage ?? 40m,
            CreatedOn = DateTime.UtcNow,
            CreatedBy = "system"
        };

        db.Suppliers.Add(supplier);
        await db.SaveChangesAsync(cancellationToken);

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
            0,
            supplier.CreatedOn,
            supplier.CreatedBy,
            supplier.UpdatedOn,
            supplier.UpdatedBy);
    }
}
