
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(Guid id);

        Task<Category?> GetByNameAsync(string name);
        Task<bool> HasProductsAsync(Guid categoryId);
        Task AddAsync(Category category);

        void Remove(Category category);
        Task DeleteAsync(Guid id);
    }
}
