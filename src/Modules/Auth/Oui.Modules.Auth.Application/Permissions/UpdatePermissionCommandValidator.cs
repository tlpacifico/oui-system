using FluentValidation;

namespace Oui.Modules.Auth.Application.Permissions;

internal sealed class UpdatePermissionCommandValidator : AbstractValidator<UpdatePermissionCommand>
{
    public UpdatePermissionCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome e obrigatorio.")
            .Must(name => name?.Split('.').Length >= 2)
            .WithMessage("O nome deve seguir o formato 'categoria.recurso.acao' (ex: admin.users.view).");
    }
}
