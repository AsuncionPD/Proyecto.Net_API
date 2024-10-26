using Microsoft.EntityFrameworkCore;
using ApiWebBeachSA.Models;

namespace ApiWebBeachSA.Data
{
    public class DbContextHotel:DbContext
    {
        public DbContextHotel(DbContextOptions<DbContextHotel> options) : base(options)
        {

        }

        /// <summary>
        /// propiedad para manejar los procesos CRUD
        /// </summary>
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Paquete> Paquetes { get; set; }
    }
}
