using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Prueba.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriasController : ControllerBase
    {
        private readonly AppDBContext _appDbContext;

        public CategoriasController(AppDBContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategorias()
        {
            var categorias = await _appDbContext.Categorias.ToListAsync();
            return Ok(categorias);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoria(int id)
        {
            var categoria = await _appDbContext.Categorias.FindAsync(id);
            if (categoria == null) return Ok("La categoría no existe");
            return Ok(categoria);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategoria(Categoria categoria)
        {
            var validationResult = await ValidarCategoria(categoria);
            if (!string.IsNullOrEmpty(validationResult))
            {
                return Ok(validationResult);
            }

            _appDbContext.Categorias.Add(categoria);
            await _appDbContext.SaveChangesAsync();
            return Ok(categoria);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditCategoria(int id, Categoria categoria)
        {
            if (id != categoria.Id) return Ok("El ID de la categoría no coincide");

            var categoriaExistente = await _appDbContext.Categorias.FindAsync(id);
            if (categoriaExistente == null) return Ok("La categoría no existe");

            var validationResult = await ValidarCategoria(categoria, id);
            if (!string.IsNullOrEmpty(validationResult))
            {
                return Ok(validationResult);
            }

            categoriaExistente.Nombre = categoria.Nombre;
            categoriaExistente.Descripcion = categoria.Descripcion;

            _appDbContext.Categorias.Update(categoriaExistente);
            await _appDbContext.SaveChangesAsync();
            return Ok(categoriaExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _appDbContext.Categorias.FindAsync(id);
            if (categoria == null)
                return Ok("La categoría no existe");

            _appDbContext.Categorias.Remove(categoria);
            await _appDbContext.SaveChangesAsync();
            return Ok(new { message = $"Categoría con ID {id} ha sido eliminada correctamente" });
        }

        private async Task<string?> ValidarCategoria(Categoria categoria, int? id = null)
        {
            if (string.IsNullOrWhiteSpace(categoria.Nombre))
            {
                return "El nombre es obligatorio";
            }

            var categoriaExistente = await _appDbContext.Categorias
                .FirstOrDefaultAsync(c => c.Nombre == categoria.Nombre && (!id.HasValue || c.Id != id.Value));
            if (categoriaExistente != null)
            {
                return "Ya existe una categoría con el mismo nombre";
            }

            return null;
        }
    }
}