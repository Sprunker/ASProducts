namespace ASProducts.Api.DTOs;

/// <summary>
/// DTO utilizado para exponer la información de un producto al cliente.
/// </summary>
public class ProductGetDto
{
    public int Id { get; set; } 
    public string Name { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public decimal Price { get; set; } = 1.0m;
    public int? RestrictionAge { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
}