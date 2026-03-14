using shs.Application.Messaging;

namespace Oui.Modules.Inventory.Application.Consignment.Commands.CreateReception;

public sealed record CreateReceptionCommand(
    Guid? SupplierExternalId,
    int ItemCount,
    string? Notes) : ICommand<ReceptionDetailResponse>;
