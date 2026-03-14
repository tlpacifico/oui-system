using System.Text.RegularExpressions;
using FluentValidation;

namespace Oui.Modules.Inventory.Application.Suppliers.Commands.UpdateSupplier;

internal sealed class UpdateSupplierCommandValidator : AbstractValidator<UpdateSupplierCommand>
{
    public UpdateSupplierCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("O nome é obrigatório.")
            .MaximumLength(256).WithMessage("O nome deve ter no máximo 256 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("O email é obrigatório.")
            .Must(IsValidEmail).WithMessage("O email não é válido.");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("O telefone é obrigatório.")
            .Must(IsValidPortuguesePhone).WithMessage("O telefone deve estar no formato +351XXXXXXXXX (9 dígitos após +351).");

        RuleFor(x => x.TaxNumber)
            .Must(nif => string.IsNullOrWhiteSpace(nif) || IsValidPortugueseNif(nif.Trim()))
            .WithMessage("O NIF deve conter exatamente 9 dígitos e ser válido.");

        RuleFor(x => x.Initial)
            .NotEmpty().WithMessage("A inicial é obrigatória.")
            .MaximumLength(5).WithMessage("A inicial deve ter no máximo 5 caracteres.")
            .Matches(@"^[A-Za-z]+$").WithMessage("A inicial deve conter apenas letras.");
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new global::System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPortuguesePhone(string phone)
    {
        return Regex.IsMatch(phone.Trim(), @"^\+351\d{9}$");
    }

    private static bool IsValidPortugueseNif(string nif)
    {
        if (!Regex.IsMatch(nif, @"^\d{9}$"))
            return false;

        var firstDigit = nif[0];
        if (firstDigit == '0' || firstDigit == '4')
            return false;

        var sum = 0;
        for (var i = 0; i < 8; i++)
            sum += (nif[i] - '0') * (9 - i);

        var remainder = sum % 11;
        var checkDigit = remainder < 2 ? 0 : 11 - remainder;

        return (nif[8] - '0') == checkDigit;
    }
}
