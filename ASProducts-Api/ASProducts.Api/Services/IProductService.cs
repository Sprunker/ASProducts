using ASProducts.Api.DTOs;

namespace ASProducts.Api.Services;

public interface IProductService
{
    Task<IEnumerable<ProductGetDto>> GetAllAsync();
    Task<ProductGetDto?> GetByIdAsync(int id);
    Task<ProductGetDto> CreateAsync(ProductCreateDto dto);
    Task<bool> UpdateAsync(int id, ProductUpdateDto dto);
    Task<bool> DeleteAsync(int id);
}
