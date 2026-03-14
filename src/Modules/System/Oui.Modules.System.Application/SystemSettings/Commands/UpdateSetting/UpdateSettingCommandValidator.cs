using FluentValidation;

namespace Oui.Modules.System.Application.SystemSettings.Commands.UpdateSetting;

internal sealed class UpdateSettingCommandValidator : AbstractValidator<UpdateSettingCommand>
{
    public UpdateSettingCommandValidator()
    {
        RuleFor(c => c.Key).NotEmpty();
        RuleFor(c => c.Value).NotNull();
    }
}
