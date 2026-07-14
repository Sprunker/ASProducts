using ASProducts.Api.DTOs;

namespace ASProducts.Api.Orchestrators;

public interface IProductOrchestrator
{
    Task<ProductGetDto> CreateProductWorkflowAsync(ProductCreateDto dto);
    Task<bool> UpdateProductWorkflowAsync(int id, ProductUpdateDto dto);
}
