using FluentValidation;

namespace Oui.Modules.Auth.Application.Users;

internal sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email é obrigatório.")
            .MaximumLength(256).WithMessage("O email não pode exceder 256 caracteres.")
            .EmailAddress().WithMessage("O email não é válido.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("A password é obrigatória.")
            .MinimumLength(6).WithMessage("A password deve ter pelo menos 6 caracteres.");
    }
}
