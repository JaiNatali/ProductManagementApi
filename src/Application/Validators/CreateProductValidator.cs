using Application.DTOs.Product;
using FluentValidation;

namespace Application.Validators;

public sealed class CreateProductValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.ProductName)
            .NotNull().WithMessage("Product name is required.")
            .NotEmpty().WithMessage("Product name is required.")
            .Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("Product name cannot be whitespace.")
            .MaximumLength(255).WithMessage("Product name must not exceed 255 characters.");
    }
}
