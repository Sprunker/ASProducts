
using System.ComponentModel.DataAnnotations;

namespace ASProducts.Api.Models;

public class Product
{
    public int Id { get; set; }

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
}