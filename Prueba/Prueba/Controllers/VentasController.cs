using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Prueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentasController : ControllerBase
    {
        private readonly AppDBContext _appDbContext;

        public VentasController(AppDBContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetVentas()
        {
            var ventas = await _appDbContext.Ventas
                                            .Include(v => v.Producto)
                                            .ThenInclude(p => p.Categoria)
                                            .Include(v => v.Cliente)
                                            .ToListAsync();
            return Ok(ventas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVenta(int id)
        {
            var venta = await _appDbContext.Ventas
                                           .Include(v => v.Producto)
                                           .ThenInclude(p => p.Categoria)
                                           .Include(v => v.Cliente)
                                           .FirstOrDefaultAsync(v => v.Id == id);
            if (venta == null) return Ok("La venta no existe");
            return Ok(venta);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVenta(Venta venta)
        {
            var producto = await _appDbContext.Productos
                                              .Include(p => p.Categoria)
                                              .FirstOrDefaultAsync(p => p.Id == venta.Producto.Id);
            if (producto == null)
            {
                return Ok("El producto no existe");
            }

            var cliente = await _appDbContext.Clientes.FindAsync(venta.Cliente.Id);
            if (cliente == null)
            {
                return Ok("El cliente no existe");
            }

            venta.Producto = producto;
            venta.Cliente = cliente;

            var validationResult = await ValidarVenta(venta);
            if (!string.IsNullOrEmpty(validationResult))
            {
                return Ok(validationResult);
            }

            _appDbContext.Ventas.Add(venta);
            await _appDbContext.SaveChangesAsync();

            // Recargar la venta para incluir la información de la categoría y el cliente
            var ventaConDetalles = await _appDbContext.Ventas
                                                      .Include(v => v.Producto)
                                                      .ThenInclude(p => p.Categoria)
                                                      .Include(v => v.Cliente)
                                                      .FirstOrDefaultAsync(v => v.Id == venta.Id);

            return Ok(ventaConDetalles);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditVenta(int id, Venta venta)
        {
            if (id != venta.Id) return Ok("El ID de la venta no coincide");

            var ventaExistente = await _appDbContext.Ventas
                                                    .Include(v => v.Producto)
                                                    .ThenInclude(p => p.Categoria)
                                                    .Include(v => v.Cliente)
                                                    .FirstOrDefaultAsync(v => v.Id == id);
            if (ventaExistente == null) return Ok("La venta no existe");

            var producto = await _appDbContext.Productos
                                              .Include(p => p.Categoria)
                                              .FirstOrDefaultAsync(p => p.Id == venta.Producto.Id);
            if (producto == null)
            {
                return Ok("El producto no existe");
            }

            var cliente = await _appDbContext.Clientes.FindAsync(venta.Cliente.Id);
            if (cliente == null)
            {
                return Ok("El cliente no existe");
            }

            ventaExistente.Producto = producto;
            ventaExistente.Cliente = cliente;
            ventaExistente.Cantidad = venta.Cantidad;
            ventaExistente.FechaVenta = venta.FechaVenta;
            ventaExistente.Total = venta.Total;

            var validationResult = await ValidarVenta(ventaExistente, id);
            if (!string.IsNullOrEmpty(validationResult))
            {
                return Ok(validationResult);
            }

            _appDbContext.Ventas.Update(ventaExistente);
            await _appDbContext.SaveChangesAsync();
            return Ok(ventaExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenta(int id)
        {
            var venta = await _appDbContext.Ventas.FindAsync(id);
            if (venta == null)
                return Ok("La venta no existe");

            _appDbContext.Ventas.Remove(venta);
            await _appDbContext.SaveChangesAsync();
            return Ok(new { message = $"Venta con ID {id} ha sido eliminada correctamente" });
        }

        private async Task<string?> ValidarVenta(Venta venta, int? id = null)
        {
            if (venta.Cantidad <= 0)
            {
                return "La cantidad debe ser mayor a 0";
            }

            if (venta.Total <= 0)
            {
                return "El total debe ser mayor a 0";
            }

            if (venta.Producto == null)
            {
                return "El producto es obligatorio";
            }

            if (venta.Cliente == null)
            {
                return "El cliente es obligatorio";
            }

            var productoExistente = await _appDbContext.Productos.FindAsync(venta.Producto.Id);
            if (productoExistente == null)
            {
                return "El producto no existe";
            }

            var clienteExistente = await _appDbContext.Clientes.FindAsync(venta.Cliente.Id);
            if (clienteExistente == null)
            {
                return "El cliente no existe";
            }

            return null;
        }
    }
}