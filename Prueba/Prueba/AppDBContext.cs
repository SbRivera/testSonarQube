using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Prueba
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
    }

    public class Producto
    {
        public required int Id { get; set; }
        public required string Nombre { get; set; }
        public string? Descripcion { get; set; }
        [JsonRequired] public decimal Precio { get; set; }
        [JsonRequired] public int CantidadStock { get; set; }
        public required Categoria Categoria { get; set; } = null!;
    }

    public class Categoria
    {
        public required int Id { get; set; }
        public required string Nombre { get; set; }
        public string? Descripcion { get; set; }
    }

    public class Venta
    {
        public required int Id { get; set; }
        public required Producto Producto { get; set; } = null!;
        public required Cliente Cliente { get; set; } = null!;
        [JsonRequired] public int Cantidad { get; set; }
        [JsonRequired] public DateTime FechaVenta { get; set; }
        [JsonRequired] public decimal Total { get; set; }
    }

    public class Cliente
    {
        public required int Id { get; set; }
        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
    }
}