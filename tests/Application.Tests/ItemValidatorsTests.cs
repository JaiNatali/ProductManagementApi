using Application.DTOs.Item;
using Application.Validators;

namespace Application.Tests;

public class ItemValidatorsTests
{
    [Fact]
    public void CreateItemValidator_Should_Fail_When_Quantity_Is_Zero()
    {
        var validator = new CreateItemValidator();
        var result = validator.Validate(new CreateItemRequest { Quantity = 0 });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateItemRequest.Quantity));
    }

    [Fact]
    public void UpdateItemValidator_Should_Pass_When_Quantity_Is_Positive()
    {
        var validator = new UpdateItemValidator();
        var result = validator.Validate(new UpdateItemRequest { Quantity = 5 });

        Assert.True(result.IsValid);
    }
}
