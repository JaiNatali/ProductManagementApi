using Application.DTOs.Common;
using Application.DTOs.Product;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public sealed class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductService(
        IUnitOfWork unitOfWork,
        IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResponse<ProductSummaryResponse>> GetAllAsync(PaginationRequest request, CancellationToken cancellationToken = default)
    {
        var products = await _unitOfWork.Products.GetPagedAsync(request.PageNumber, request.PageSize, cancellationToken);
        var totalCount = await _unitOfWork.Products.CountAsync(cancellationToken);

        return new PagedResponse<ProductSummaryResponse>
        {
            Items = _mapper.Map<IReadOnlyList<ProductSummaryResponse>>(products),
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ProductDetailsResponse> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var product = await _unitOfWork.Products.GetProductWithItemsAsync(id, cancellationToken);
        if (product is null)
        {
            throw new KeyNotFoundException($"Product with id {id} was not found.");
        }

        return _mapper.Map<ProductDetailsResponse>(product);
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var existingProduct = await _unitOfWork.Products.ProductExistsAsync(request.ProductName.Trim(), cancellationToken);
        if (existingProduct)
        {
            throw new InvalidOperationException($"Product '{request.ProductName.Trim()}' already exists.");
        }

        var product = _mapper.Map<Product>(request);
        product.ProductName = request.ProductName.Trim();
        product.CreatedBy = "system";
        product.CreatedOn = DateTime.UtcNow;

        await _unitOfWork.Products.AddAsync(product, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        var existingProduct = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (existingProduct is null)
        {
            throw new KeyNotFoundException($"Product with id {id} was not found.");
        }

        existingProduct.ProductName = request.ProductName.Trim();
        existingProduct.ModifiedBy = "system";
        existingProduct.ModifiedOn = DateTime.UtcNow;

        await _unitOfWork.Products.UpdateAsync(existingProduct, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ProductResponse>(existingProduct);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var existingProduct = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (existingProduct is null)
        {
            throw new KeyNotFoundException($"Product with id {id} was not found.");
        }

        await _unitOfWork.Products.DeleteAsync(existingProduct, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
