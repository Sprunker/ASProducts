using ASProducts.Api.Services;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Moq;
using Xunit;

namespace ASProducts.Api.Tests.Services;

/// <summary>
/// ImageStorageService hace I/O directo sobre el filesystem (no depende de una
/// abstracción inyectable), así que estos tests usan un directorio temporal real
/// en vez de mockear el sistema de archivos. Cada test corre en su propia carpeta
/// aislada (Guid) y se limpia en Dispose para no dejar basura entre corridas.
/// </summary>
public class ImageStorageServiceTests : IDisposable
{
    private readonly string _webRootPath;
    private readonly Mock<IWebHostEnvironment> _envMock;
    private readonly ImageStorageService _sut;

    public ImageStorageServiceTests()
    {
        _webRootPath = Path.Combine(Path.GetTempPath(), "ASProducts.ApiTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_webRootPath);

        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_webRootPath);

        _sut = new ImageStorageService(_envMock.Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_webRootPath))
            Directory.Delete(_webRootPath, recursive: true);

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// El servicio debe garantizar que la carpeta de destino exista desde su construcción,
    /// para que SaveAsync no falle en el primer request por un directorio inexistente.
    /// </summary>
    [Fact]
    public void Constructor_CreatesUploadsFolder_WhenItDoesNotExist()
    {
        var uploadsFolder = Path.Combine(_webRootPath, "images", "products");

        Assert.True(Directory.Exists(uploadsFolder));
    }

    /// <summary>
    /// Solo se deben aceptar formatos de imagen conocidos (whitelist).
    /// Un archivo con extensión no soportada (ej. .exe o .pdf) debe rechazarse
    /// antes de escribir nada en disco, evitando que se suban archivos arbitrarios
    /// disfrazados de "imagen".
    /// </summary>
    [Fact]
    public async Task SaveAsync_WhenExtensionNotAllowed_ThrowsAndDoesNotWriteFile()
    {
        var file = CreateFakeFormFile("malware.exe");

        await Assert.ThrowsAsync<InvalidOperationException>(() => _sut.SaveAsync(1, file));

        var uploadsFolder = Path.Combine(_webRootPath, "images", "products");
        Assert.Empty(Directory.GetFiles(uploadsFolder));
    }

    /// <summary>
    /// La validación de extensión debe ser insensible a mayúsculas/minúsculas
    /// (".JPG" y ".jpg" deben tratarse igual), ya que muchos clientes móviles envían
    /// nombres de archivo en mayúsculas.
    /// </summary>
    [Fact]
    public async Task SaveAsync_WhenExtensionIsUppercase_IsTreatedAsValid()
    {
        var file = CreateFakeFormFile("photo.JPG");

        var result = await _sut.SaveAsync(1, file);

        Assert.True(File.Exists(result));
    }

    /// <summary>
    /// Al guardar la primera imagen de un producto, el archivo debe
    /// nombrarse usando el Id del producto, evitando colisiones 
    /// de nombres (decisión tomada por temas de desarrollo local).
    /// </summary>
    [Fact]
    public async Task SaveAsync_WhenNoPreviousImage_SavesFileNamedAfterProductId()
    {
        var file = CreateFakeFormFile("cualquier-nombre.png");

        var result = await _sut.SaveAsync(7, file);

        Assert.Equal(Path.Combine(_webRootPath, "images", "products", "7.png"), result);
        Assert.True(File.Exists(result));
    }

    /// <summary>
    /// Si un producto ya tenía una imagen guardada con una extensión
    /// distinta a la nueva (ej. tenía .png y ahora se sube un .jpg), el archivo viejo debe
    /// eliminarse. De lo contrario quedarían dos archivos de imagen "activos" para el mismo
    /// producto (7.png y 7.jpg), y GetPhysicalPath podría seguir devolviendo la imagen
    /// obsoleta dependiendo del orden de AllowedExtensions.
    /// </summary>
    [Fact]
    public async Task SaveAsync_WhenPreviousImageHasDifferentExtension_DeletesOldFileBeforeSavingNew()
    {
        var oldFile = CreateFakeFormFile("original.png");
        await _sut.SaveAsync(3, oldFile);
        var oldPath = Path.Combine(_webRootPath, "images", "products", "3.png");
        Assert.True(File.Exists(oldPath));

        var newFile = CreateFakeFormFile("nuevo.jpg");
        var newPath = await _sut.SaveAsync(3, newFile);

        Assert.False(File.Exists(oldPath));
        Assert.True(File.Exists(newPath));
    }

    /// <summary>
    /// Reemplazar una imagen con la misma extensión (caso más común,
    /// ej. actualizar la foto de un producto) debe sobrescribir el archivo existente
    /// con el nuevo contenido, no fallar por "archivo en uso" ni duplicarlo.
    /// </summary>
    [Fact]
    public async Task SaveAsync_WhenPreviousImageHasSameExtension_OverwritesFileContent()
    {
        var firstFile = CreateFakeFormFile("v1.jpg", "contenido original");
        var path = await _sut.SaveAsync(4, firstFile);

        var secondFile = CreateFakeFormFile("v2.jpg", "contenido actualizado");
        await _sut.SaveAsync(4, secondFile);

        var finalContent = await File.ReadAllTextAsync(path);
        Assert.Equal("contenido actualizado", finalContent);
    }

    /// <summary>
    /// Consultar la ruta física de un producto sin imagen almacenada debe devolver null en
    /// lugar de una ruta inválida o una excepción, para que se pueda mostrar una imagen por defecto.
    /// </summary>
    [Fact]
    public void GetPhysicalPath_WhenNoImageExists_ReturnsNull()
    {
        var result = _sut.GetPhysicalPath(999);

        Assert.Null(result);
    }

    /// <summary>
    /// Si el producto tiene una imagen guardada, GetPhysicalPath debe
    /// encontrarla sin importar cuál de las extensiones permitidas sea.
    /// </summary>
    [Fact]
    public async Task GetPhysicalPath_WhenImageExists_ReturnsCorrectPath()
    {
        var file = CreateFakeFormFile("foto.webp");
        var savedPath = await _sut.SaveAsync(10, file);

        var result = _sut.GetPhysicalPath(10);

        Assert.Equal(savedPath, result);
    }

    /// <summary>
    /// Eliminar la imagen de un producto que sí tiene una imagen guardada
    /// debe borrar físicamente el archivo del disco.
    /// </summary>
    [Fact]
    public async Task Delete_WhenImageExists_RemovesFileFromDisk()
    {
        var file = CreateFakeFormFile("foto.png");
        var savedPath = await _sut.SaveAsync(2, file);
        Assert.True(File.Exists(savedPath));

        _sut.Delete(2);

        Assert.False(File.Exists(savedPath));
    }

    /// <summary>
    /// Eliminar la imagen de un producto que nunca tuvo una no debe
    /// lanzar excepción. Es un flujo válido (ej. crear un producto sin imagen y
    /// luego "actualizarlo" sin adjuntar una nueva), no un error.
    /// </summary>
    [Fact]
    public void Delete_WhenNoImageExists_DoesNotThrow()
    {
        var exception = Record.Exception(() => _sut.Delete(12345));

        Assert.Null(exception);
    }

    private static IFormFile CreateFakeFormFile(string fileName, string content = "fake image content")
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);

        return new FormFile(stream, 0, bytes.Length, "Image", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }
}