using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Prueba;

namespace Prueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly AppDBContext _appDbContext;

        public ClientesController(AppDBContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetClientes()
        {
            var clientes = await _appDbContext.Clientes.ToListAsync();
            return Ok(clientes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCliente(int id)
        {
            var cliente = await _appDbContext.Clientes.FindAsync(id);
            if (cliente == null) return Ok("El cliente no existe");
            return Ok(cliente);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCliente(Cliente cliente)
        {
            var validationResult = await ValidarCliente(cliente);
            if (!string.IsNullOrEmpty(validationResult))
            {
                return Ok(validationResult);
            }

            _appDbContext.Clientes.Add(cliente);
            await _appDbContext.SaveChangesAsync();
            return Ok(cliente);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditCliente(int id, Cliente cliente)
        {
            if (id != cliente.Id) return Ok("El ID del cliente no coincide");

            var clienteExistente = await _appDbContext.Clientes.FindAsync(id);
            if (clienteExistente == null) return Ok("El cliente no existe");

            var validationResult = await ValidarCliente(cliente, id);
            if (!string.IsNullOrEmpty(validationResult))
            {
                return Ok(validationResult);
            }

            clienteExistente.Nombre = cliente.Nombre;
            clienteExistente.Apellido = cliente.Apellido;
            clienteExistente.Email = cliente.Email;
            clienteExistente.Telefono = cliente.Telefono;

            _appDbContext.Clientes.Update(clienteExistente);
            await _appDbContext.SaveChangesAsync();
            return Ok(clienteExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _appDbContext.Clientes.FindAsync(id);
            if (cliente == null)
                return Ok("El cliente no existe");

            _appDbContext.Clientes.Remove(cliente);
            await _appDbContext.SaveChangesAsync();
            return Ok(new { message = $"Cliente con ID {id} ha sido eliminado correctamente" });
        }


        private async Task<string?> ValidarCliente(Cliente cliente, int? id = null)
        {
            if (string.IsNullOrWhiteSpace(cliente.Nombre))
            {
                return "El nombre es obligatorio";
            }
            if (cliente.Nombre.Any(char.IsDigit))
            {
                return "El nombre no debe contener números";
            }

            if (string.IsNullOrWhiteSpace(cliente.Apellido))
            {
                return "El apellido es obligatorio";
            }
            if (cliente.Apellido.Any(char.IsDigit))
            {
                return "El apellido no debe contener números";
            }

            if (string.IsNullOrWhiteSpace(cliente.Email))
            {
                return "El email es obligatorio";
            }
            if (!cliente.Email.Contains("@") || !cliente.Email.Contains("."))
            {
                return "El email ingresado no es válido";
            }

            if (!string.IsNullOrWhiteSpace(cliente.Telefono))
            {
                if (!cliente.Telefono.StartsWith("09") || cliente.Telefono.Length != 10 || !cliente.Telefono.All(char.IsDigit))
                {
                    return "El número de teléfono debe iniciar con 09 y tener exactamente 10 dígitos";
                }
            }

            var clienteExistente = await _appDbContext.Clientes
                .FirstOrDefaultAsync(c => c.Email == cliente.Email && (!id.HasValue || c.Id != id.Value));
            if (clienteExistente != null)
            {
                return "Ya existe un cliente con el mismo email";
            }

            return null;
        }
    }
}