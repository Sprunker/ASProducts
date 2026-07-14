using ASProducts.Api.Data;
using ASProducts.Api.DTOs;
using ASProducts.Api.Services;

namespace ASProducts.Api.Orchestrators;

public class ProductOrchestrator(IProductService _productService, IImageStorageService _imageService, ProductsContext _context) : IProductOrchestrator
{
    public async Task<ProductGetDto> CreateProductWorkflowAsync(ProductCreateDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var product = await _productService.CreateAsync(dto);

            if (dto.Image != null)
            {
                var imagePath = await _imageService.SaveAsync(product.Id, dto.Image);
            }

            await transaction.CommitAsync();
            return product;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> UpdateProductWorkflowAsync(int id, ProductUpdateDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var updated = await _productService.UpdateAsync(id, dto);
            if (!updated) return false;

            if (dto.Image != null)
                await _imageService.SaveAsync(id, dto.Image);
            else if (dto.RemoveImage)
                _imageService.Delete(id);

            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}