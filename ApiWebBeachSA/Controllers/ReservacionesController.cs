using Microsoft.AspNetCore.Mvc;
using ApiWebBeachSA.Models.Costume;
using ApiWebBeachSA.Service;
using System.Text.RegularExpressions;
using ApiWebBeachSA.Data;
using ApiWebBeachSA.Models;


namespace ApiWebBeachSA.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReservacionesController : Controller
    {
        private readonly DbContextHotel _context = null;

        public ReservacionesController(DbContextHotel pContext)
        {
            _context = pContext;

        }

        [HttpGet("Listado")]
        public List<Reservacion> Listado()
        {
            List<Reservacion> lista = null;
            lista = _context.Reservaciones.ToList();
            return lista;
        }

        /// <summary>
        /// Método encargado de agregar una reservacion
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        [HttpPost("Agregar")]
        public async Task<string> Agregar(Reservacion temp)
        {
            string mensaje = "Debe ingresar la informacion completa de la reservacion ";

            if (temp == null)
            {
                return mensaje;
            }
            else
            {
                try
                {
                    _context.Reservaciones.Add(temp);
                    await _context.SaveChangesAsync();
                    mensaje = $"Reservacion almacenando con exito!";
                }
                catch (Exception ex)
                {
                    var aux= _context.Clientes.FirstOrDefault(x=>x.Cedula==temp.ClienteID);
                    mensaje = $"Error al agregar la reservacion del cliente {aux.Nombre} detalle {ex.InnerException}";
                }
                return mensaje;
            }
        }

        /// <summary>
        /// Método para eliminar una reservacion por medio del id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("Eliminar")]
        public async Task<string> Eliminar(int id)
        {
            string mensaje = $"Reservacion no eliminada, el id {id} no existe";

            Reservacion temp = _context.Reservaciones.FirstOrDefault(x => x.ReservacionID == id);

            if (temp != null)
            {
                _context.Reservaciones.Remove(temp);
                await _context.SaveChangesAsync();
                mensaje = $"La reservacion con la id {temp.ReservacionID} fue eliminado correctamente..";
            }
            return mensaje;
        }

        /// <summary>
        /// Método encargado de editar la información de una reservacion
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        [HttpPut("Editar")]
        public async Task<string> Editar(Reservacion temp)
        {
            var aux = _context.Reservaciones.FirstOrDefault(x => x.ReservacionID == temp.ReservacionID);

            string mensaje = $"La reservacion con la id {temp.ReservacionID} no existe";

            if (aux != null)
            {
                aux.PaqueteID = temp.PaqueteID;
                aux.FechaReservacion = temp.FechaReservacion;
                aux.Noches = temp.Noches;
                aux.Personas = temp.Personas;
                aux.TotalSinDescuento = temp.TotalSinDescuento;
                aux.DescuentoPorcentaje = temp.DescuentoPorcentaje;
                aux.TotalConDescuento = temp.TotalConDescuento;
                aux.FormaPago = temp.FormaPago;
                if (temp.FormaPago.Equals("Cheque"))
                {
                    aux.NumeroCheque = temp.NumeroCheque;
                    aux.Banco = temp.Banco;
                }
                aux.Prima = temp.Prima;
                aux.RestoEnMensualidades = temp.RestoEnMensualidades;
                aux.TipoCambio = temp.TipoCambio;
                aux.TotalUSD = temp.TotalUSD;


                _context.Reservaciones.Update(aux);
                await _context.SaveChangesAsync();
                mensaje = $"La reservacion con la id {aux.ReservacionID} se actualizado correctamente";
            }
           
            return mensaje;
        }

        /// <summary>
        /// Método encargado de consultar una reservacion por medio del Id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("Buscar")]
        public Reservacion Buscar(int id)
        {
            Reservacion temp = null;
            temp = _context.Reservaciones.FirstOrDefault(x => x.ReservacionID == id);
            return temp == null ? new Reservacion() { Banco = "No existe" } : temp;
        }

        /// <summary>
        /// Método encargado de consultar las reservaciones de un usuario por medio de su id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpGet("ReservacionesCliente")]
        public List<Reservacion> ClienteReservaciones(int id)
        {
            var reservacionesCliente = _context.Reservaciones.Where(x => x.ClienteID == id).ToList();
            return reservacionesCliente == null ? null : reservacionesCliente;
        }





    }//Cierre del controlador
}//Cierre del namespace
