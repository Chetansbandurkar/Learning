using ECommerce.Application.Common;
using ECommerce.Application.DTOs.Products;

namespace ECommerce.Application.Interfaces
{
    public interface IProductService
    {
        Task<PagedResult<ProductResponse>> GetProductsAsync(
            ProductQueryParameters parameters);

        Task<ProductResponse> CreateAsync(
            CreateProductRequest request);

        Task<ProductResponse?> GetByIdAsync(Guid id);

        Task<ProductResponse> UpdateAsync(
                    Guid id,
                    UpdateProductRequest request);
        Task DeleteAsync(Guid id);
    }
}
