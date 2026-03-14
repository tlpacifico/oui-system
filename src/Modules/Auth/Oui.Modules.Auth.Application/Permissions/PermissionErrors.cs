using shs.Domain.Results;

namespace Oui.Modules.Auth.Application.Permissions;

public static class PermissionErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Permission.NotFound",
        "Permissao nao encontrada.");

    public static readonly Error NameRequired = Error.Problem(
        "Permission.NameRequired",
        "O nome e obrigatorio.");

    public static readonly Error InvalidNameFormat = Error.Problem(
        "Permission.InvalidNameFormat",
        "O nome deve seguir o formato 'categoria.recurso.acao' (ex: admin.users.view).");

    public static readonly Error NameAlreadyExists = Error.Conflict(
        "Permission.NameAlreadyExists",
        "Ja existe uma permissao com este nome.");

    public static readonly Error HasAssignedRoles = Error.Problem(
        "Permission.HasAssignedRoles",
        "Nao e possivel eliminar uma permissao que esta atribuida a roles. Remova a permissao das roles primeiro.");
}
