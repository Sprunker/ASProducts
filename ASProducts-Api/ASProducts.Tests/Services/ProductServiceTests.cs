using ASProducts.Api.DTOs;
using ASProducts.Api.Mappings;
using ASProducts.Api.Models;
using ASProducts.Api.Repositories;
using ASProducts.Api.Services;

using Microsoft.Extensions.Logging.Abstractions;

using AutoMapper;
using Moq;
using Xunit;

namespace ASProducts.Api.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly IMapper _mapper;
    private readonly ProductService _sut;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();

        var config = new MapperConfiguration(
            cfg => cfg.AddProfile<ProductProfile>(),
            NullLoggerFactory.Instance
        );

        _mapper = config.CreateMapper();

        _sut = new ProductService(_repositoryMock.Object, _mapper);
    }

    /// <summary>
    /// Consultar un producto por un Id que no existe en el repositorio
    /// debe devolver null en lugar de lanzar una excepción o un DTO vacío.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Product?)null);

        var result = await _sut.GetByIdAsync(999);

        Assert.Null(result);
        _repositoryMock.Verify(r => r.GetByIdAsync(999), Times.Once);
    }

    /// <summary>
    /// Valida que el mapeo de AutoMapper entre la entidad Product y el
    /// ProductGetDto transfiere correctamente las propiedades esperadas.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_WhenProductExists_MapsToDto()
    {
        var product = new Product
        {
            Id = 1,
            Name = "Test Product",
            Company = "Test Co",
            Price = 100
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(product);

        var result = await _sut.GetByIdAsync(1);

        Assert.NotNull(result);
        Assert.Equal(product.Id, result!.Id);
        Assert.Equal(product.Name, result.Name);
    }

    /// <summary>
    /// Intentar actualizar un producto que no existe debe fallar de forma controlada
    /// (retornando false) sin llegar a invocar la persistencia en el repositorio.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WhenProductDoesNotExist_ReturnsFalseAndDoesNotCallRepository()
    {
        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<int>()))
            .ReturnsAsync(false);

        var dto = new ProductUpdateDto
        {
            Name = "Test",
            Company = "Test Co",
            Price = 100
        };

        var result = await _sut.UpdateAsync(999, dto);

        Assert.False(result);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Product>()), Times.Never);
    }

    /// <summary>
    /// El Id de la entidad a actualizar debe tomarse siempre del parámetro de ruta,
    /// no de un valor que pudiera venir manipulado dentro del DTO, evitando así que un
    /// cliente actualice un recurso distinto al indicado en la URL.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_WhenProductExists_AssignsCorrectIdFromRouteParameter()
    {
        const int productId = 42;
        Product? capturedProduct = null;

        _repositoryMock
            .Setup(r => r.ExistsAsync(productId))
            .ReturnsAsync(true);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Product>()))
            .Callback<Product>(p => capturedProduct = p)
            .Returns(Task.CompletedTask);

        var dto = new ProductUpdateDto
        {
            Name = "Updated Name",
            Company = "Updated Co",
            Price = 250
        };

        var result = await _sut.UpdateAsync(productId, dto);

        Assert.True(result);
        Assert.NotNull(capturedProduct);
        Assert.Equal(productId, capturedProduct!.Id);
        Assert.Equal(dto.Name, capturedProduct.Name);
    }

    /// <summary>
    /// Al crear un producto, todas las propiedades relevantes del DTO
    /// (incluyendo campos opcionales como RestrictionAge y Description)
    /// deben mapearse correctamente a la entidad persistida.
    /// </summary>
    [Fact]
    public async Task CreateAsync_MapsAllPropertiesFromDtoToEntity()
    {
        var dto = new ProductCreateDto
        {
            Name = "New Product",
            Company = "Acme Inc",
            Price = 199.99m,
            RestrictionAge = 18,
            Description = "Test description"
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Product>()))
            .Callback<Product>(p => p.Id = 1)
            .Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(dto);

        Assert.Equal(dto.Name, result.Name);
        Assert.Equal(dto.Company, result.Company);
        Assert.Equal(dto.Price, result.Price);
        Assert.Equal(dto.RestrictionAge, result.RestrictionAge);
        Assert.Equal(1, result.Id);
    }

    /// <summary>
    /// Eliminar un producto que no existe debe reportarse como fallido
    /// sin invocar el borrado en el repositorio.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WhenProductDoesNotExist_ReturnsFalseAndDoesNotCallDelete()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Product?)null);

        var result = await _sut.DeleteAsync(999);

        Assert.False(result);
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Product>()), Times.Never);
    }

    /// <summary>
    /// Eliminar un producto existente debe delegar en el repositorio
    /// pasando exactamente la entidad correspondiente al Id solicitado.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_WhenProductExists_CallsRepositoryDeleteWithCorrectEntity()
    {
        var existingProduct = new Product
        {
            Id = 5,
            Name = "To Delete",
            Company = "Acme",
            Price = 50
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(5))
            .ReturnsAsync(existingProduct);

        _repositoryMock
            .Setup(r => r.DeleteAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        var result = await _sut.DeleteAsync(5);

        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteAsync(
            It.Is<Product>(p => p.Id == 5)),
            Times.Once);
    }
}