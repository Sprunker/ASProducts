using ASProducts.Api.Data;
using ASProducts.Api.Models;
using ASProducts.Api.Repositories;

using Microsoft.EntityFrameworkCore;

using Xunit;

namespace ASProducts.Api.Tests.Repositories;

public class ProductRepositoryTests : IDisposable
{
    private readonly ProductsContext _context;
    private readonly ProductRepository _sut;

    public ProductRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ProductsContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ProductsContext(options);
        _sut = new ProductRepository(_context);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Al agregar un producto nuevo, el repositorio debe persistirlo y EF Core debe 
    /// autogenerar el Id, confirmando que el objeto queda correctamente guardado en 
    /// la base de datos.
    /// </summary>
    [Fact]
    public async Task AddAsync_PersistsProductAndAssignsGeneratedId()
    {
        var product = new Product
        {
            Name = "Test Product",
            Company = "Test Co",
            Price = 99.99m
        };

        await _sut.AddAsync(product);

        Assert.True(product.Id > 0);
        var saved = await _context.Products.FindAsync(product.Id);
        Assert.NotNull(saved);
        Assert.Equal("Test Product", saved!.Name);
    }

    /// <summary>
    /// Obtener todos los productos debe devolver exactamente los registros 
    /// persistidos, sin omitir ni duplicar ninguno.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsAllPersistedProducts()
    {
        _context.Products.AddRange(
            new Product { Name = "A", Company = "Co1", Price = 10 },
            new Product { Name = "B", Company = "Co2", Price = 20 },
            new Product { Name = "C", Company = "Co3", Price = 30 }
        );
        await _context.SaveChangesAsync();

        var result = await _sut.GetAllAsync();

        Assert.Equal(3, result.Count());
    }

    /// <summary>
    /// Las consultas de listado deben usar AsNoTracking() para evitar sobrecarga de 
    /// memoria y efectos secundarios de tracking en escenarios de solo lectura, 
    /// típicos de una request HTTP real donde el contexto es efímero.
    /// </summary>
    [Fact]
    public async Task GetAllAsync_ReturnsUntrackedEntities()
    {
        _context.Products.Add(new Product { Name = "A", Company = "Co1", Price = 10 });
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var result = await _sut.GetAllAsync();

        Assert.Empty(_context.ChangeTracker.Entries<Product>());
        Assert.Single(result);
    }

    /// <summary>
    /// Buscar un producto con un Id inexistente debe devolver null en lugar de 
    /// lanzar una excepción.
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_WhenProductDoesNotExist_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(999);

        Assert.Null(result);
    }

    /// <summary>
    /// Actualizar un producto debe persistir los cambios en base de datos incluso 
    /// partiendo de una entidad "desconectada" (sin tracking previo), como ocurriría
    /// al recibir los datos desde un nuevo request HTTP.
    /// </summary>
    [Fact]
    public async Task UpdateAsync_PersistsChangesToExistingProduct()
    {
        var product = new Product { Name = "Original", Company = "Co", Price = 50 };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var productId = product.Id;
        _context.ChangeTracker.Clear();

        var updatedProduct = new Product
        {
            Id = productId,
            Name = "Updated Name",
            Company = "Co",
            Price = 999
        };

        await _sut.UpdateAsync(updatedProduct);

        var fromDb = await _context.Products.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId);

        Assert.NotNull(fromDb);
        Assert.Equal("Updated Name", fromDb!.Name);
        Assert.Equal(999, fromDb.Price);
    }

    /// <summary>
    /// Eliminar un producto existente debe removerlo definitivamente de la base de datos.
    /// </summary>
    [Fact]
    public async Task DeleteAsync_RemovesProductFromDatabase()
    {
        var product = new Product { Name = "To Delete", Company = "Co", Price = 10 };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var productId = product.Id;
        _context.ChangeTracker.Clear();

        var toDelete = await _context.Products.FindAsync(productId);

        await _sut.DeleteAsync(toDelete!);

        var fromDb = await _context.Products.FindAsync(productId);
        Assert.Null(fromDb);
    }

    /// <summary>
    /// ExistsAsync debe confirmar de forma eficiente que un producto existe, sin 
    /// necesidad de traer la entidad completa.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_WhenProductExists_ReturnsTrue()
    {
        var product = new Product { Name = "Exists", Company = "Co", Price = 10 };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _sut.ExistsAsync(product.Id);

        Assert.True(result);
    }

    /// <summary>
    /// ExistsAsync debe devolver false ante un Id que no corresponde a ningún producto persistido.
    /// </summary>
    [Fact]
    public async Task ExistsAsync_WhenProductDoesNotExist_ReturnsFalse()
    {
        var result = await _sut.ExistsAsync(999);

        Assert.False(result);
    }
}