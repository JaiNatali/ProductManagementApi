using Application.DTOs.Common;
using Application.DTOs.Product;

namespace Application.Interfaces;

public interface IProductService
{
    Task<PagedResponse<ProductSummaryResponse>> GetAllAsync(PaginationRequest request, CancellationToken cancellationToken = default);
    Task<ProductDetailsResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
