using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiWebBeachSA.Data;
using ApiWebBeachSA.Models;
using ApiWebBeachSA.Models.Costume;
using ApiWebBeachSA.Service;
using System.Text.RegularExpressions;


namespace ApiWebBeachSA.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientesController : Controller
    {
        //Variable para utilizar la referencia der ORM Entity Framework core
        private readonly DbContextHotel _context = null;

        //Variable para utilizar los servicios de autorizacion
        private readonly IAutorizacionServices _autorizacionServices;

        public ClientesController(DbContextHotel pContext, IAutorizacionServices autorizacionServices)
        {
            _context = pContext;
            _autorizacionServices = autorizacionServices;
        }

        /// <summary>
        /// Método encargado de mostrar la información de todos los clientes 
        /// </summary>
        /// <returns></returns>
        [HttpGet("Listado")]
        public List<Cliente> Listado()
        {
            List<Cliente> lista = null;
            lista = _context.Clientes.ToList();
            return lista;
        }

        /// <summary>
        /// Método encargado de agregar un cliente
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="confirmPassword"></param>
        /// <returns></returns>
        [HttpPost("CrearCuenta")]
        public async Task<IActionResult> CrearCuenta(Cliente temp, string confirmPassword)
        {
            if (temp == null)
            {
                return BadRequest("Debe ingresar los datos del usuario.");
            }

            var correoExistente = _context.Clientes.FirstOrDefault(x => x.Email.Equals(temp.Email));
            if (correoExistente != null)
            {
                return BadRequest("Ya existe un usuario con ese correo.");
            }

            if (!temp.Confirmar(confirmPassword))
            {
                return BadRequest("La confirmación de la contraseña no coincide.");
            }

            string mensajeValidacion = ValidarContraseña(temp.Password, temp.Nombre);
            if (!string.IsNullOrEmpty(mensajeValidacion))
            {
                return BadRequest(mensajeValidacion);
            }

            try
            {
                temp.TipoUsuario = 2;
                _context.Clientes.Add(temp);
                await _context.SaveChangesAsync();
                return Ok($"Usuario {temp.Nombre} almacenando con éxito.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al agregar el usuario {temp.Nombre}, detalle {ex.InnerException}");
            }
        }

        /// <summary>
        /// Método encargado de eliminar a un cliente por medio de la cédula
        /// </summary>
        /// <param name="cedula"></param>
        /// <returns></returns>
        [HttpDelete("Eliminar")]
        public async Task<string> Eliminar(int cedula)
        {
            string mensaje = $"Usuario no eliminado, cedula: {cedula} no existe";

            Cliente temp = _context.Clientes.FirstOrDefault(x => x.Cedula == cedula);

            if (temp != null)
            {
                _context.Clientes.Remove(temp);
                await _context.SaveChangesAsync();
                mensaje = $"Usuario {temp.Nombre} eliminado correctamente..";
            }
            return mensaje;
        }

        /// <summary>
        /// Método encargado de editar la información de un cliente
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        [HttpPut("Editar")]
        public async Task<string> Editar(Cliente temp)
        {
            var aux = _context.Clientes.FirstOrDefault(x => x.Cedula == temp.Cedula);

            string mensaje = "";
            if (aux != null)
            {
                aux.Nombre = temp.Nombre;
                aux.Telefono = temp.Telefono;
                aux.Direccion = temp.Direccion;
                aux.Email = temp.Email;

                _context.Clientes.Update(aux);

                await _context.SaveChangesAsync();

                mensaje = $"El Usuario {aux.Nombre} se actualizado correctamente";
            }
            else
            {
                mensaje = $"El Usuario {temp.Nombre} no existe";
            }
            return mensaje;
        }

        /// <summary>
        /// Método encargado de consultar un cliente por medio de la cédula
        /// </summary>
        /// <param name="cedula"></param>
        /// <returns></returns>
        [HttpGet("Buscar")]
        public Cliente Buscar(int cedula)
        {
            Cliente temp = null;
            temp = _context.Clientes.FirstOrDefault(x => x.Cedula == cedula);
            return temp == null ? new Cliente
                () { Nombre = "No existe" } : temp;
        }

        //Validar los requisitos básicos de una contraseña 
        private string ValidarContraseña(string password, string userName)
        {
            if (password.Equals(userName, StringComparison.OrdinalIgnoreCase))
            {
                return "El password no debe ser igual al nombre del usuario.";
            }

            if (!Regex.IsMatch(password, @"\d"))
            {
                return "El password debe tener al menos un número.";
            }

            if (!Regex.IsMatch(password, @"[a-z]"))
            {
                return "La contraseña debe tener al menos una letra minúscula.";
            }

            if (!Regex.IsMatch(password, @"[A-Z]"))
            {
                return "La contraseña debe tener al menos una letra mayúscula.";
            }

            if (!Regex.IsMatch(password, @"[\W_]"))
            {
                return "La contraseña debe tener al menos un carácter especial.";
            }

            return null;
        }

        /// <summary>
        /// Método encargado de manejar la autenticación del cliente
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost("Login")]
        public async Task<IActionResult> AutenticationPW(string email, string password)
        {
            var temp = await _context.Clientes.FirstOrDefaultAsync(u => u.Email.Equals(email) && u.Password.Equals(password));

            if (temp == null)
            {
                return Unauthorized(new AutorizacionResponse() { Token = "", Msj = "No autorizado", Resultado = false });
            }
            else
            {
                var autorizado = await _autorizacionServices.DevolverToken(temp);
                if (autorizado == null)
                {
                    return Unauthorized(new AutorizacionResponse() { Token = "", Msj = "No autorizado", Resultado = false });
                }
                else
                {
                    return Ok(autorizado);
                }
            }
        }


    }
}
