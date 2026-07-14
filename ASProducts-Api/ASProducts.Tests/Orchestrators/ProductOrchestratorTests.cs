using ASProducts.Api.Data;
using ASProducts.Api.DTOs;
using ASProducts.Api.Orchestrators;
using ASProducts.Api.Services;

using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

using Moq;
using Xunit;

namespace ASProducts.Api.Tests.Orchestrators;

public class ProductOrchestratorTests : IDisposable
{
    private readonly Mock<IProductService> _serviceMock;
    private readonly Mock<IImageStorageService> _imageServiceMock;
    private readonly ProductsContext _context;
    private readonly ProductOrchestrator _sut;

    public ProductOrchestratorTests()
    {
        _serviceMock = new Mock<IProductService>();
        _imageServiceMock = new Mock<IImageStorageService>();

        // Se usa SQLite en modo :memory: para usar BeginTransactionAsync,
        // ya que InMemory de EF Core no lo soporta.
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ProductsContext>()
            .UseSqlite(connection)
            .Options;

        _context = new ProductsContext(options);
        _context.Database.EnsureCreated();

        _sut = new ProductOrchestrator(_serviceMock.Object, _imageServiceMock.Object, _context);
    }

    public void Dispose()
    {
        _context.Database.CloseConnection();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Crear un producto sin imagen adjunta no debe invocar al servicio
    /// de almacenamiento de imágenes bajo ninguna circunstancia.
    /// </summary>
    [Fact]
    public async Task CreateProductWorkflowAsync_WhenNoImage_CreatesProductWithoutCallingImageService()
    {
        var dto = new ProductCreateDto { Name = "Test", Company = "Co", Price = 10, Image = null };
        var created = new ProductGetDto { Id = 1, Name = "Test" };

        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);

        var result = await _sut.CreateProductWorkflowAsync(dto);

        Assert.Equal(1, result.Id);
        _imageServiceMock.Verify(
            i => i.SaveAsync(It.IsAny<int>(), It.IsAny<IFormFile>()),
            Times.Never);
    }

    /// <summary>
    /// Al crear un producto con imagen, el Id generado por la creación del producto
    /// debe usarse para guardar la imagen (no un Id arbitrario), garantizando
    /// que la imagen quede correctamente asociada al registro recién creado.
    /// </summary>
    [Fact]
    public async Task CreateProductWorkflowAsync_WhenImageProvided_SavesImageWithCreatedProductId()
    {
        var fakeImage = CreateFakeFormFile;
        var dto = new ProductCreateDto { Name = "Test", Company = "Co", Price = 10, Image = fakeImage };
        var created = new ProductGetDto { Id = 42, Name = "Test" };

        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);
        _imageServiceMock.Setup(i => i.SaveAsync(42, fakeImage)).ReturnsAsync("/fake/path.jpg");

        var result = await _sut.CreateProductWorkflowAsync(dto);

        Assert.Equal(42, result.Id);
        _imageServiceMock.Verify(i => i.SaveAsync(42, fakeImage), Times.Once);
    }

    /// <summary>
    /// Si falla el guardado de la imagen a mitad de la transacción
    /// (por ejemplo, disco lleno), la excepción original debe propagarse tal cual,
    /// confirmando que el rollback no la enmascara ni produce un error distinto.
    /// </summary>
    [Fact]
    public async Task CreateProductWorkflowAsync_WhenImageServiceThrows_PropagatesException()
    {
        var fakeImage = CreateFakeFormFile;
        var dto = new ProductCreateDto { Name = "Test", Company = "Co", Price = 10, Image = fakeImage };
        var created = new ProductGetDto { Id = 1, Name = "Test" };

        _serviceMock.Setup(s => s.CreateAsync(dto)).ReturnsAsync(created);
        _imageServiceMock
            .Setup(i => i.SaveAsync(It.IsAny<int>(), It.IsAny<IFormFile>()))
            .ThrowsAsync(new IOException("Disk full"));

        await Assert.ThrowsAsync<IOException>(() => _sut.CreateProductWorkflowAsync(dto));
    }

    /// <summary>
    /// Intentar actualizar un producto inexistente no debe tocar el servicio de imágenes 
    /// (ni guardar ni eliminar), evitando efectos secundarios sobre un recurso que nunca existió.
    /// </summary>
    [Fact]
    public async Task UpdateProductWorkflowAsync_WhenProductDoesNotExist_ReturnsFalseWithoutTouchingImageService()
    {
        var dto = new ProductUpdateDto { Name = "X", Company = "Y", Price = 1 };

        _serviceMock.Setup(s => s.UpdateAsync(999, dto)).ReturnsAsync(false);

        var result = await _sut.UpdateProductWorkflowAsync(999, dto);

        Assert.False(result);
        _imageServiceMock.Verify(i => i.Delete(It.IsAny<int>()), Times.Never);
        _imageServiceMock.Verify(i => i.SaveAsync(It.IsAny<int>(), It.IsAny<IFormFile>()), Times.Never);
    }

    /// <summary>
    /// Al actualizar un producto existente con una nueva imagen, se debe guardar la imagen
    /// con el Id correcto, se elimina la imagen previa como parte de este flujo (SaveAsync).
    /// </summary>
    [Fact]
    public async Task UpdateProductWorkflowAsync_WhenImageProvided_CallsSaveAsyncWithCorrectParameters()
    {
        var fakeImage = CreateFakeFormFile;
        var dto = new ProductUpdateDto { Name = "X", Company = "Y", Price = 1, Image = fakeImage };

        _serviceMock.Setup(s => s.UpdateAsync(5, dto)).ReturnsAsync(true);
        _imageServiceMock.Setup(i => i.SaveAsync(5, fakeImage)).ReturnsAsync("/new/path.jpg");

        var result = await _sut.UpdateProductWorkflowAsync(5, dto);

        Assert.True(result);
        _imageServiceMock.Verify(i => i.SaveAsync(5, fakeImage), Times.Once);
        _imageServiceMock.Verify(i => i.Delete(It.IsAny<int>()), Times.Never);
    }

    /// <summary>
    /// Actualizar un producto sin adjuntar imagen nueva y sin solicitar su eliminación
    /// (RemoveImage=false, valor por defecto) debe conservar la imagen existente sin
    /// tocar el servicio de almacenamiento en absoluto.
    /// </summary>
    [Fact]
    public async Task UpdateProductWorkflowAsync_WhenNoImageAndNoRemoveFlag_DoesNotTouchImageService()
    {
        var dto = new ProductUpdateDto { Name = "X", Company = "Y", Price = 1, Image = null, RemoveImage = false };

        _serviceMock.Setup(s => s.UpdateAsync(5, dto)).ReturnsAsync(true);

        var result = await _sut.UpdateProductWorkflowAsync(5, dto);

        Assert.True(result);
        _imageServiceMock.Verify(i => i.Delete(It.IsAny<int>()), Times.Never);
        _imageServiceMock.Verify(i => i.SaveAsync(It.IsAny<int>(), It.IsAny<IFormFile>()), Times.Never);
    }

    /// <summary>
    /// Actualizar un producto con RemoveImage=true y sin imagen nueva debe eliminar
    /// la imagen existente. Esta es la única vía explícita para borrarla
    /// </summary>
    [Fact]
    public async Task UpdateProductWorkflowAsync_WhenRemoveImageIsTrueAndNoNewImage_DeletesExistingImage()
    {
        var dto = new ProductUpdateDto { Name = "X", Company = "Y", Price = 1, Image = null, RemoveImage = true };

        _serviceMock.Setup(s => s.UpdateAsync(5, dto)).ReturnsAsync(true);

        var result = await _sut.UpdateProductWorkflowAsync(5, dto);

        Assert.True(result);
        _imageServiceMock.Verify(i => i.Delete(5), Times.Once);
        _imageServiceMock.Verify(i => i.SaveAsync(It.IsAny<int>(), It.IsAny<IFormFile>()), Times.Never);
    }

    /// <summary>
    /// Si se envía una imagen nueva, esta tiene prioridad sobre RemoveImage=true,
    /// evitando un estado contradictorio (guardar y borrar la imagen en la misma operación).
    /// </summary>
    [Fact]
    public async Task UpdateProductWorkflowAsync_WhenImageProvidedAndRemoveImageAlsoTrue_SavesNewImageAndIgnoresRemoveFlag()
    {
        var fakeImage = CreateFakeFormFile;
        var dto = new ProductUpdateDto { Name = "X", Company = "Y", Price = 1, Image = fakeImage, RemoveImage = true };

        _serviceMock.Setup(s => s.UpdateAsync(5, dto)).ReturnsAsync(true);
        _imageServiceMock.Setup(i => i.SaveAsync(5, fakeImage)).ReturnsAsync("/new/path.jpg");

        var result = await _sut.UpdateProductWorkflowAsync(5, dto);

        Assert.True(result);
        _imageServiceMock.Verify(i => i.SaveAsync(5, fakeImage), Times.Once);
        _imageServiceMock.Verify(i => i.Delete(It.IsAny<int>()), Times.Never);
    }

    private static IFormFile CreateFakeFormFile
    {
        get
        {
            var content = "fake image content";
            var bytes = System.Text.Encoding.UTF8.GetBytes(content);
            var stream = new MemoryStream(bytes);

            return new FormFile(stream, 0, bytes.Length, "Image", "test.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
        }
    }
}