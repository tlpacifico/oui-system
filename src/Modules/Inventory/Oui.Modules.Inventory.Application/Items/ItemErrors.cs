using shs.Domain.Results;

namespace Oui.Modules.Inventory.Application.Items;

public static class ItemErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Item.NotFound", "Item não encontrado.");

    public static readonly Error BrandNotFound = Error.NotFound(
        "Item.BrandNotFound", "Marca não encontrada.");

    public static readonly Error CategoryNotFound = Error.NotFound(
        "Item.CategoryNotFound", "Categoria não encontrada.");

    public static readonly Error SupplierNotFound = Error.NotFound(
        "Item.SupplierNotFound", "Fornecedor não encontrado.");

    public static readonly Error InvalidCondition = Error.Problem(
        "Item.InvalidCondition", "Condição inválida.");

    public static readonly Error InvalidAcquisitionType = Error.Problem(
        "Item.InvalidAcquisitionType", "Tipo de aquisição inválido.");

    public static readonly Error InvalidOrigin = Error.Problem(
        "Item.InvalidOrigin", "Origem inválida.");

    public static readonly Error SupplierRequiredForConsignment = Error.Problem(
        "Item.SupplierRequiredForConsignment", "Fornecedor é obrigatório para itens de consignação.");

    public static readonly Error CannotEditSoldItem = Error.Conflict(
        "Item.CannotEditSoldItem", "Não é possível editar um item já vendido.");

    public static readonly Error CannotDeleteSoldItem = Error.Conflict(
        "Item.CannotDeleteSoldItem", "Não é possível eliminar um item já vendido.");

    public static readonly Error PhotoNotFound = Error.NotFound(
        "Item.PhotoNotFound", "Foto não encontrada.");

    public static readonly Error NoFilesProvided = Error.Problem(
        "Item.NoFilesProvided", "Nenhum ficheiro enviado.");

    public static Error MaxPhotosExceeded(int current) => Error.Problem(
        "Item.MaxPhotosExceeded", $"Máximo de 10 fotos por item. Atualmente tem {current}.");

    public static Error UnsupportedFileType(string contentType) => Error.Problem(
        "Item.UnsupportedFileType", $"Tipo de ficheiro não suportado: {contentType}. Use JPEG, PNG ou WebP.");

    public static Error FileTooLarge(string fileName) => Error.Problem(
        "Item.FileTooLarge", $"Ficheiro demasiado grande: {fileName}. Máximo 10 MB.");

    public static readonly Error EmptyPhotoList = Error.Problem(
        "Item.EmptyPhotoList", "Lista de fotos vazia.");

    public static readonly Error ReceptionNotFound = Error.NotFound(
        "Item.ReceptionNotFound", "Reception not found.");

    public static readonly Error TagsNotFound = Error.Problem(
        "Item.TagsNotFound", "One or more tags not found.");
}
