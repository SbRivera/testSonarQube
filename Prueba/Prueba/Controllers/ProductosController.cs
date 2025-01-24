using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Prueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly AppDBContext _appDbContext;

        public ProductosController(AppDBContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            var productos = await _appDbContext.Productos.Include(p => p.Categoria).ToListAsync();
            return Ok(productos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProducto(int id)
        {
            var producto = await _appDbContext.Productos.Include(p => p.Categoria).FirstOrDefaultAsync(p => p.Id == id);
            if (producto == null) return Ok("El producto no existe");
            return Ok(producto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProducto(Producto producto)
        {
            var validationResult = await ValidarProducto(producto);
            if (!string.IsNullOrEmpty(validationResult))
            {
                return Ok(validationResult);
            }

            var categoria = await _appDbContext.Categorias.FindAsync(producto.Categoria.Id);
            if (categoria == null)
            {
                return Ok("La categoría no existe");
            }

            producto.Categoria = categoria;

            _appDbContext.Productos.Add(producto);
            await _appDbContext.SaveChangesAsync();
            return Ok(producto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditProducto(int id, Producto producto)
        {
            if (id != producto.Id) return Ok("El ID del producto no coincide");

            var productoExistente = await _appDbContext.Productos.FindAsync(id);
            if (productoExistente == null) return Ok("El producto no existe");

            var validationResult = await ValidarProducto(producto, id);
            if (!string.IsNullOrEmpty(validationResult))
            {
                return Ok(validationResult);
            }

            var categoria = await _appDbContext.Categorias.FindAsync(producto.Categoria.Id);
            if (categoria == null)
            {
                return Ok("La categoría no existe");
            }

            productoExistente.Nombre = producto.Nombre;
            productoExistente.Descripcion = producto.Descripcion;
            productoExistente.Precio = producto.Precio;
            productoExistente.CantidadStock = producto.CantidadStock;
            productoExistente.Categoria = categoria;

            _appDbContext.Productos.Update(productoExistente);
            await _appDbContext.SaveChangesAsync();
            return Ok(productoExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _appDbContext.Productos.FindAsync(id);
            if (producto == null)
                return Ok("El producto no existe");

            _appDbContext.Productos.Remove(producto);
            await _appDbContext.SaveChangesAsync();
            return Ok(new { message = $"Producto con ID {id} ha sido eliminado correctamente" });
        }

        private async Task<string?> ValidarProducto(Producto producto, int? id = null)
        {
            if (string.IsNullOrWhiteSpace(producto.Nombre))
            {
                return "El nombre es obligatorio";
            }

            if (producto.Precio <= 0)
            {
                return "El precio debe ser mayor a 0";
            }

            if (producto.CantidadStock < 0)
            {
                return "La cantidad en stock no puede ser negativa";
            }

            var productoExistente = await _appDbContext.Productos
                .FirstOrDefaultAsync(p => p.Nombre == producto.Nombre && (!id.HasValue || p.Id != id.Value));
            if (productoExistente != null)
            {
                return "Ya existe un producto con el mismo nombre";
            }

            return null;
        }
    }
}