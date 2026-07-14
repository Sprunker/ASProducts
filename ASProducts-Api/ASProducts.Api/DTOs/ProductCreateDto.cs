using System.ComponentModel.DataAnnotations;

namespace ASProducts.Api.DTOs;

/// <summary>
/// DTO utilizado para crear un nuevo producto.
/// </summary>
public class ProductCreateDto
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(50)]
    public required string Name { get; set; }

    [Required(ErrorMessage = "Company is required")]
    [MaxLength(50)]
    public required string Company { get; set; }

    [Required(ErrorMessage = "Price is required")]
    [Range(1, 1000, ErrorMessage = "Invalid price")]
    public required decimal Price { get; set; }

    [Range(0, 100, ErrorMessage = "Invalid age")]
    public int? RestrictionAge { get; set; }

    [MaxLength(100)]
    public string? Description { get; set; }

    public IFormFile? Image { get; set; }
}
