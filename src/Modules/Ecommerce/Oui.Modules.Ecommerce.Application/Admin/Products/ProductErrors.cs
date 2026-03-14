using shs.Domain.Results;

namespace Oui.Modules.Ecommerce.Application.Admin.Products;

public static class ProductErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Product.NotFound", "Produto não encontrado.");

    public static readonly Error ItemNotFound = Error.NotFound(
        "Product.ItemNotFound", "Item não encontrado.");

    public static readonly Error ItemNotToSell = Error.Problem(
        "Product.ItemNotToSell", "Item deve estar com status 'À Venda' para ser publicado.");

    public static readonly Error AlreadyPublished = Error.Conflict(
        "Product.AlreadyPublished", "Este item já está publicado no e-commerce.");

    public static readonly Error AlreadyUnpublished = Error.Problem(
        "Product.AlreadyUnpublished", "Produto já está despublicado.");

    public static readonly Error PhotoNotFound = Error.NotFound(
        "Product.PhotoNotFound", "Foto não encontrada.");

    public static readonly Error NoFilesProvided = Error.Problem(
        "Product.NoFilesProvided", "Nenhum ficheiro enviado.");

    public static readonly Error NoItemsProvided = Error.Problem(
        "Product.NoItemsProvided", "Nenhum item fornecido.");

    public static Error MaxPhotosExceeded(int current) => Error.Problem(
        "Product.MaxPhotosExceeded", $"Máximo de 10 fotos por produto. Atualmente tem {current}.");

    public static Error UnsupportedFileType(string contentType) => Error.Problem(
        "Product.UnsupportedFileType", $"Tipo de ficheiro não suportado: {contentType}. Use JPEG, PNG ou WebP.");

    public static Error FileTooLarge(string fileName) => Error.Problem(
        "Product.FileTooLarge", $"Ficheiro demasiado grande: {fileName}. Máximo 10 MB.");
}
