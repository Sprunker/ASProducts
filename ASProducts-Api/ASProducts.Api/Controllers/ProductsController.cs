using ASProducts.Api.DTOs;
using ASProducts.Api.Models;
using ASProducts.Api.Orchestrators;
using ASProducts.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASProducts.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IProductService _service, IImageStorageService _imageService, IProductOrchestrator _orchestrator) : ControllerBase
    {
        /// <summary>
        /// GetAll
        /// </summary>
        /// <remarks>
        /// Retorna la lista de productos.
        /// </remarks>
        /// <returns>Lista de productos.</returns>
        /// <response code="200">Retorna la lista de productos.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<ProductGetDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<ProductGetDto>>> GetAll()
        {
            var products = await _service.GetAllAsync();

            foreach (var product in products)
                EnrichWithImageInfo(product);

            return Ok(products);
        }

        /// <summary>
        /// GetById
        /// </summary>
        /// <remarks>
        /// Obtén un producto por su Id.
        /// </remarks>
        /// <param name="id">Id del producto.</param>
        /// <returns>El producto solicitado.</returns>
        /// <response code="200">Producto encontrado.</response>
        /// <response code="404">Producto no encontrado.</response>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(ProductGetDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductGetDto>> GetById(int id)
        {
            var product = await _service.GetByIdAsync(id);

            if (product is null)
                return NotFound($"The product with Id {id} was not found.");

            EnrichWithImageInfo(product);

            return Ok(product);
        }

        /// <summary>
        /// GetImage
        /// </summary>
        /// <remarks>
        /// Obtiene la imagen de un producto por su Id.
        /// </remarks>
        /// <param name="id">Id del producto.</param>
        /// <response code="200">Retorna el archivo de imagen.</response>
        /// <response code="404">El producto o su imagen no fueron encontrados.</response>
        [HttpGet("{id:int}/image")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetImage(int id)
        {
            var path = _imageService.GetPhysicalPath(id);

            if (path is null)
                return NotFound($"No image found for product with Id {id}.");

            var contentType = Path.GetExtension(path).ToLowerInvariant() switch
            {
                ".png" => "image/png",
                ".webp" => "image/webp",
                _ => "image/jpeg",
            };

            return PhysicalFile(path, contentType);
        }

        /// <summary>
        /// Create
        /// </summary>
        /// <remarks>
        /// Crea un nuevo producto, incluyendo imagen opcional.
        /// </remarks>
        /// <param name="dto">Datos del producto a crear.</param>
        /// <response code="201">Producto creado exitosamente.</response>
        /// <response code="400">Datos de entrada inválidos.</response>
        [HttpPost]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(typeof(ProductGetDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductGetDto>> Create([FromForm] ProductCreateDto dto)
        {
            var created = await _orchestrator.CreateProductWorkflowAsync(dto);

            EnrichWithImageInfo(created);

            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <remarks>
        /// Actualiza un producto existente. Si se envía una nueva imagen, reemplaza la anterior.
        /// </remarks>
        /// <param name="id">Id del producto a actualizar.</param>
        /// <param name="dto">Datos actualizados del producto.</param>
        /// <response code="204">Producto actualizado exitosamente.</response>
        /// <response code="404">Producto no encontrado.</response>
        [HttpPut("{id:int}")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, [FromForm] ProductUpdateDto dto)
        {
            var success = await _orchestrator.UpdateProductWorkflowAsync(id, dto);

            if (!success)
                return NotFound($"The product with Id {id} was not found.");

            return NoContent();
        }

        /// <summary>
        /// Delete
        /// </summary>
        /// <remarks>
        /// Elimina un producto por su Id, incluyendo su imagen si existe.
        /// </remarks>
        /// <param name="id">Id del producto a eliminar.</param>
        /// <response code="204">Producto eliminado exitosamente.</response>
        /// <response code="404">Producto no encontrado.</response>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);

            if (!deleted)
                return NotFound($"The product with Id {id} was not found.");

            _imageService.Delete(id);

            return NoContent();
        }

        /// <summary>
        /// Enriquece un ProductGetDto con la información de imagen (ImageUrl),
        /// resuelta a través de IImageStorageService. Se hace aquí, en el controlador,
        /// para no acoplar ProductService al almacenamiento de imágenes.
        /// </summary>
        private void EnrichWithImageInfo(ProductGetDto dto)
        {
            var hasImage = _imageService.GetPhysicalPath(dto.Id) is not null;
            dto.ImageUrl = hasImage ? Url.Action(nameof(GetImage), new { id = dto.Id }) : null;
        }
    }
}