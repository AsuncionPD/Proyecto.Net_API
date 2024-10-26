﻿using Microsoft.AspNetCore.Mvc;
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


        //metodo encargado de mostrar la informacion de todos los paquetes 
        [HttpGet("Listado")]
        public List<Paquete> Listado()
        {
            List<Paquete> lista = null;
            lista = _context.Paquetes.ToList();
            return lista;
        }

        //metodo encargado de agregar un paquete 
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

        //Metodo para eliminar un paquete por medio del id
        [HttpDelete("Eliminar")]
        public async Task<string> Eliminar(int id)
        {
            string mensaje = $"Paquete no eliminado, el id {id} no existe";

            Paquete temp = _context.Paquetes.FirstOrDefault(x => x.Id == id);

            if (temp != null)
            {
                _context.Paquetes.Remove(temp);
                await _context.SaveChangesAsync();
                mensaje = $"Paquete {temp.Nombre} eliminado correctamente..";
            }
            return mensaje;
        }

        //Metodo encardado de editar la informacion de un paquete
        [HttpPut("Editar")]
        public async Task<string> Editar(Paquete temp)
        {
            var aux = _context.Paquetes.FirstOrDefault(x => x.Id == temp.Id);

            string mensaje = "";
            if (aux != null)
            {
                aux.Nombre = temp.Nombre;
                aux.Precio = temp.Precio;
                aux.PorcentajeReserva = temp.PorcentajeReserva;
                aux.Mensualidades = temp.Mensualidades;

                _context.Paquetes.Update(aux);
                await _context.SaveChangesAsync();
                mensaje = $"El Paquete {aux.Id} se actualizado correctamente";
            }
            else
            {
                mensaje = $"El Paquete {temp.Nombre} no existe";
            }
            return mensaje;
        }

        //Metodo encargado de consultar un paquete por medio del Id
        [HttpGet("Buscar")]
        public Paquete Buscar(int id)
        {
            Paquete temp = null;
            temp = _context.Paquetes.FirstOrDefault(x => x.Id == id);
            return temp == null ? new Paquete() { Nombre = "No existe" } : temp;
        }

    }
}
