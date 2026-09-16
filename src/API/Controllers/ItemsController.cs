using Application.DTOs.Item;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products/{productId:int}/items")]
public sealed class ItemsController : ControllerBase
{
    private readonly IItemService _itemService;
    private readonly ILogger<ItemsController> _logger;

    public ItemsController(IItemService itemService, ILogger<ItemsController> logger)
    {
        _itemService = itemService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<ItemSummaryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItems(int productId, CancellationToken cancellationToken)
    {
        var result = await _itemService.GetAllByProductAsync(productId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{itemId:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetItem(int productId, int itemId, CancellationToken cancellationToken)
    {
        var result = await _itemService.GetByIdAsync(productId, itemId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateItem(int productId, [FromBody] CreateItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _itemService.CreateAsync(productId, request, cancellationToken);
        _logger.LogInformation("Item created. ItemId: {ItemId} ProductId: {ProductId} Quantity: {Quantity}", result.Id, productId, request.Quantity);
        return CreatedAtAction(nameof(GetItem), new { productId, itemId = result.Id }, result);
    }

    [HttpPut("{itemId:int}")]
    [Authorize(Roles = "Admin")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(ItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(int productId, int itemId, [FromBody] UpdateItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _itemService.UpdateAsync(productId, itemId, request, cancellationToken);
        _logger.LogInformation("Item updated. ItemId: {ItemId} ProductId: {ProductId} Quantity: {Quantity}", itemId, productId, request.Quantity);
        return Ok(result);
    }

    [HttpDelete("{itemId:int}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(int productId, int itemId, CancellationToken cancellationToken)
    {
        await _itemService.DeleteAsync(productId, itemId, cancellationToken);
        _logger.LogInformation("Item deleted. ItemId: {ItemId} ProductId: {ProductId}", itemId, productId);
        return NoContent();
    }
}
