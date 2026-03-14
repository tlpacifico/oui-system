using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.SupplierReturns.Commands.CreateReturn;

public sealed record CreateReturnCommand(
    Guid SupplierExternalId,
    Guid[] ItemExternalIds,
    string? Notes) : ICommand<SupplierReturnDetailResponse>;
