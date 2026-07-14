using ASProducts.Api.DTOs;
using ASProducts.Api.Models;
using ASProducts.Api.Repositories;
using AutoMapper;

namespace ASProducts.Api.Services;

public class ProductService(IProductRepository _repository, IMapper _mapper) : IProductService
{
    public async Task<IEnumerable<ProductGetDto>> GetAllAsync()
    {
        var productos = await _repository.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductGetDto>>(productos);
    }

    public async Task<ProductGetDto?> GetByIdAsync(int id)
    {
        var producto = await _repository.GetByIdAsync(id);
        return _mapper.Map<ProductGetDto?>(producto);
    }

    public async Task<ProductGetDto> CreateAsync(ProductCreateDto dto)
    {
        var product = _mapper.Map<Product>(dto);
        await _repository.AddAsync(product);
        return _mapper.Map<ProductGetDto>(product);
    }

    public async Task<bool> UpdateAsync(int id, ProductUpdateDto dto)
    {
        var exists = await _repository.ExistsAsync(id);
        if (!exists)
            return false;

        var product = _mapper.Map<Product>(dto);
        product.Id = id;

        await _repository.UpdateAsync(product);
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var product = await _repository.GetByIdAsync(id);
        if (product is null)
            return false;

        await _repository.DeleteAsync(product);
        return true;
    }
}
