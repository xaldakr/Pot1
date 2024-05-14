using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
namespace Pot1_API.Models
{
    public class Pot1Context : DbContext
    {
        public Pot1Context(DbContextOptions<Pot1Context> options) : base(options)
        {

        }
        public DbSet<Tipo_Rol> Tipos_Rol { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Archivo> Archivos { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<Tarea> Tareas { get; set; }
    }
}
