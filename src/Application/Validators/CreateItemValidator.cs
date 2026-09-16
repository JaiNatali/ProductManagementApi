using Application.DTOs.Item;
using FluentValidation;

namespace Application.Validators;

public sealed class CreateItemValidator : AbstractValidator<CreateItemRequest>
{
    public CreateItemValidator()
    {
        RuleFor(x => x.Quantity)
            .NotNull().WithMessage("Quantity is required.")
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");
    }
}
