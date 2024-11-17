using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiWebBeachSA.Data;
using ApiWebBeachSA.Models;

namespace ApiWebBeachSA.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PaquetesController : Controller
    {
        //Variable para utilizar la referencia der ORM Entity Framework core
        private readonly DbContextHotel _context = null;

        public PaquetesController(DbContextHotel pContext)
        {
            _context = pContext;
        }


        /// <summary>
        /// Método encargado de mostrar la información de todos los paquetes 
        /// </summary>
        /// <returns></returns>
        [HttpGet("Listado")]
        public List<Paquete> Listado()
        {
            List<Paquete> lista = null;
            lista = _context.Paquetes.ToList();
            return lista;
        }

        /// <summary>
        /// Método encargado de agregar un paquete
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        [HttpPost("Agregar")]
        public async Task<string> Agregar(Paquete temp)
        {
            string mensaje = "Debe ingresar la informacion del paquete";

            if (temp == null)
            {
                return mensaje;
            }
            else
            {
                try
                {
                    _context.Paquetes.Add(temp);
                    await _context.SaveChangesAsync();
                    mensaje = $"Paquete {temp.Nombre} almacenando con exito..";
                }
                catch (Exception ex)
                {
                    mensaje = $"Error al agregar el paquete {temp.Nombre} detalle {ex.InnerException}";
                }
                return mensaje;
            }
        }

        /// <summary>
        /// Método para eliminar un paquete por medio del id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("Eliminar")]
        public async Task<string> Eliminar(int id)
        {
            string mensaje = $"Paquete no eliminado, el id {id} no existe";

            Paquete temp = _context.Paquetes.FirstOrDefault(x => x.PaqueteID == id);

            if (temp != null)
            {
                _context.Paquetes.Remove(temp);
                await _context.SaveChangesAsync();
                mensaje = $"Paquete {temp.Nombre} eliminado correctamente..";
            }
            return mensaje;
        }

        /// <summary>
        /// Método encargado de editar la información de un paquete
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        [HttpPut("Editar")]
        public async Task<string> Editar(Paquete temp)
        {
            var aux = _context.Paquetes.FirstOrDefault(x => x.PaqueteID == temp.PaqueteID);

            string mensaje = "";
            if (aux != null)
            {
                aux.Nombre = temp.Nombre;
                aux.PrecioPorPersonaPorNoche = temp.PrecioPorPersonaPorNoche;
                aux.PrimaPorcentaje = temp.PrimaPorcentaje;
                aux.Mensualidades = temp.Mensualidades;

                _context.Paquetes.Update(aux);
                await _context.SaveChangesAsync();
                mensaje = $"El Paquete {aux.PaqueteID} se actualizado correctamente";
            }
            else
            {
                mensaje = $"El Paquete {temp.Nombre} no existe";
            }
            return mensaje;
        }

        /// <summary>
        /// Método encargado de consultar un paquete por medio del Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Buscar")]
        public Paquete Buscar(int id)
        {
            Paquete temp = null;
            temp = _context.Paquetes.FirstOrDefault(x => x.PaqueteID == id);
            return temp == null ? new Paquete() { Nombre = "No existe" } : temp;
        }

    }
}
