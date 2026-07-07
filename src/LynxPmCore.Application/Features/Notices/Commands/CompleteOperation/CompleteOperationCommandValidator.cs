using FluentValidation;

namespace LynxPmCore.Application.Features.Notices.Commands.CompleteOperation;

internal sealed class CompleteOperationCommandValidator : AbstractValidator<CompleteOperationCommand>
{
    public CompleteOperationCommandValidator()
    {
        RuleFor(x => x.Notes).MaximumLength(1000);
        RuleFor(x => x.Failure).MaximumLength(1000);

        RuleForEach(x => x.Causes).ChildRules(cause =>
        {
            cause.RuleFor(c => c.Code).NotEmpty().MaximumLength(20);
            cause.RuleFor(c => c.Text).MaximumLength(500);
        });
    }
}
