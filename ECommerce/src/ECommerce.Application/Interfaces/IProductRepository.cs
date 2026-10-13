using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces;

public interface IProductRepository
{
    IQueryable<Product> Query();

    Task<Product?> GetByIdAsync(Guid id);

    Task AddAsync(Product product);

    void Update(Product product);

    void Remove(Product product);
    void SetOriginalRowVersion(
    Product product,
    byte[] rowVersion);
}