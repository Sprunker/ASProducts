using ASProducts.Api.Controllers;
using ASProducts.Api.DTOs;
using ASProducts.Api.Orchestrators;
using ASProducts.Api.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;

using Moq;
using Xunit;

namespace ASProducts.Api.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _serviceMock;
    private readonly Mock<IImageStorageService> _imageServiceMock;
    private readonly Mock<IProductOrchestrator> _orchestratorMock;
    private readonly ProductsController _sut;

    public ProductsControllerTests()
    {
        _serviceMock = new Mock<IProductService>();
        _imageServiceMock = new Mock<IImageStorageService>();
        _orchestratorMock = new Mock<IProductOrchestrator>();

        _sut = new ProductsController(_serviceMock.Object, _imageServiceMock.Object, _orchestratorMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            },
            Url = Mock.Of<IUrlHelper>(u =>
                    u.Action(It.IsAny<UrlActionContext>()) == "/api/products/1/image")
        };
    }

    /// <summary>
    /// Al listar el catálogo, cada producto con imagen almacenada debe exponer una URL
    /// de imagen consumible por el frontend, no solo la ruta física interna.
    /// </summary>
    [Fact]
    public async Task GetAll_WhenProductsHaveImages_SetsImageUrlForEach()
    {
        var products = new List<ProductGetDto>
        {
            new() { Id = 1, Name = "A" },
            new() { Id = 2, Name = "B" }
        };

        _serviceMock.Setup(s => s.GetAllAsync()).ReturnsAsync(products);
        _imageServiceMock.Setup(i => i.GetPhysicalPath(It.IsAny<int>())).Returns("/fake/path.jpg");

        var result = await _sut.GetAll();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedProducts = Assert.IsType<IEnumerable<ProductGetDto>>(okResult.Value, exactMatch: false);
        Assert.All(returnedProducts, p => Assert.False(string.IsNullOrEmpty(p.ImageUrl)));
    }

    /// <summary>
    /// Consultar un producto inexistente debe responder 404 sin realizar trabajo
    /// adicional innecesario, como buscar una imagen que nunca se usará.
    /// </summary>
    [Fact]
    public async Task GetById_WhenProductDoesNotExist_ReturnsNotFound()
    {
        _serviceMock.Setup(s => s.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((ProductGetDto?)null);

        var result = await _sut.GetById(999);

        Assert.IsType<NotFoundObjectResult>(result.Result);
        _imageServiceMock.Verify(i => i.GetPhysicalPath(It.IsAny<int>()), Times.Never);
    }

    /// <summary>
    /// Un producto válido pero sin imagen asociada no debe romper la respuesta; 
    /// ImageUrl debe quedar explícitamente en null en vez de un valor inválido.
    /// </summary>
    [Fact]
    public async Task GetById_WhenProductHasNoImage_ImageUrlIsNull()
    {
        var product = new ProductGetDto { Id = 1, Name = "No Image" };

        _serviceMock.Setup(s => s.GetByIdAsync(1)).ReturnsAsync(product);
        _imageServiceMock.Setup(i => i.GetPhysicalPath(1)).Returns((string?)null);

        var result = await _sut.GetById(1);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<ProductGetDto>(okResult.Value);
        Assert.Null(dto.ImageUrl);
    }

    /// <summary>
    /// Solicitar el archivo de imagen de un producto que no tiene una ruta 
    /// física registrada debe responder 404 en lugar de lanzar una excepción.
    /// </summary>
    [Fact]
    public void GetImage_WhenImageDoesNotExist_ReturnsNotFound()
    {
        _imageServiceMock.Setup(i => i.GetPhysicalPath(It.IsAny<int>())).Returns((string?)null);

        var result = _sut.GetImage(1);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    /// <summary>
    /// El controller debe permanecer "delgado" (thin controller) y delegar
    /// todo el flujo de creación (persistencia + imagen) al Orchestrator, sin invocar
    /// directamente a IProductService ni a IImageStorageService. Esto asegura que la
    /// lógica de negocio y la transaccionalidad vivan en una sola capa.
    /// </summary>
    [Fact]
    public async Task Create_DelegatesWorkflowToOrchestratorAndReturnsCreatedResult()
    {
        var dto = new ProductCreateDto { Name = "New", Company = "Co", Price = 10 };
        var created = new ProductGetDto { Id = 7, Name = "New" };

        _orchestratorMock
            .Setup(o => o.CreateProductWorkflowAsync(dto))
            .ReturnsAsync(created);

        _imageServiceMock.Setup(i => i.GetPhysicalPath(7)).Returns((string?)null);

        var result = await _sut.Create(dto);

        var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(ProductsController.GetById), createdResult.ActionName);
        Assert.Equal(7, ((ProductGetDto)createdResult.Value!).Id);

        _orchestratorMock.Verify(o => o.CreateProductWorkflowAsync(dto), Times.Once);
        _serviceMock.Verify(s => s.CreateAsync(It.IsAny<ProductCreateDto>()), Times.Never);
    }

    /// <summary>
    /// Actualizar un producto inexistente debe informarse como 404,
    /// reflejando el resultado que reporta el Orchestrator.
    /// </summary>
    [Fact]
    public async Task Update_WhenOrchestratorReturnsFalse_ReturnsNotFound()
    {
        var dto = new ProductUpdateDto { Name = "X", Company = "Y", Price = 1 };

        _orchestratorMock
            .Setup(o => o.UpdateProductWorkflowAsync(999, dto))
            .ReturnsAsync(false);

        var result = await _sut.Update(999, dto);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    /// <summary>
    /// Una actualización exitosa debe responder 204 No Content, siguiendo la
    /// semántica REST estándar para updates sin cuerpo de respuesta.
    /// </summary>
    [Fact]
    public async Task Update_WhenOrchestratorSucceeds_ReturnsNoContent()
    {
        var dto = new ProductUpdateDto { Name = "X", Company = "Y", Price = 1 };

        _orchestratorMock
            .Setup(o => o.UpdateProductWorkflowAsync(5, dto))
            .ReturnsAsync(true);

        var result = await _sut.Update(5, dto);

        Assert.IsType<NoContentResult>(result);
        _orchestratorMock.Verify(o => o.UpdateProductWorkflowAsync(5, dto), Times.Once);
    }

    /// <summary>
    /// Al eliminar un producto existente, su imagen asociada debe eliminarse
    /// usando exactamente el mismo Id, evitando borrar imágenes incorrectas.
    /// </summary>
    [Fact]
    public async Task Delete_WhenProductExists_CallsImageServiceDeleteWithCorrectId()
    {
        _serviceMock.Setup(s => s.DeleteAsync(3)).ReturnsAsync(true);

        var result = await _sut.Delete(3);

        Assert.IsType<NoContentResult>(result);
        _imageServiceMock.Verify(i => i.Delete(3), Times.Once);
    }

    /// <summary>
    /// Si el producto a eliminar no existe, no debe intentarse borrar ninguna 
    /// imagen, evitando efectos secundarios sobre recursos que no aplican.
    /// </summary>
    [Fact]
    public async Task Delete_WhenProductDoesNotExist_DoesNotCallImageServiceDelete()
    {
        _serviceMock.Setup(s => s.DeleteAsync(999)).ReturnsAsync(false);

        var result = await _sut.Delete(999);

        Assert.IsType<NotFoundObjectResult>(result);
        _imageServiceMock.Verify(i => i.Delete(It.IsAny<int>()), Times.Never);
    }
}