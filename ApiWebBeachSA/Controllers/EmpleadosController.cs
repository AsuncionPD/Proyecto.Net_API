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

    public class EmpleadosController : Controller
    {
        //Variable para utilizar la referencia der ORM Entity Framework core
        private readonly DbContextHotel _context = null;

        //Variable para utilizar los servicios de autorizacion
        private readonly IAutorizacionServicesEmpleado _autorizacionServices;

        public EmpleadosController(DbContextHotel pContext, IAutorizacionServicesEmpleado autorizacionServices)
        {
            _context = pContext;
            _autorizacionServices = autorizacionServices;
        }

        /// <summary>
        /// Método encargado de mostrar la información de todos los empleados 
        /// </summary>
        /// <returns></returns>
        [HttpGet("Listado")]
        public List<Empleado> Listado()
        {
            List<Empleado> lista = null;
            lista = _context.Empleados.ToList();
            return lista;
        }

        /// <summary>
        /// Método encargado de agregar un empleado
        /// </summary>
        /// <param name="temp"></param>
        /// <param name="confirmPassword"></param>
        /// <returns></returns>
        [HttpPost("CrearCuenta")]
        public async Task<IActionResult> CrearCuenta(Empleado temp, string confirmPassword)
        {
            if (temp == null)
            {
                return BadRequest("Debe ingresar los datos del usuario.");
            }

            var correoExistente = _context.Empleados.FirstOrDefault(x => x.Email.Equals(temp.Email));
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
                temp.TipoUsuario = 1;
                _context.Empleados.Add(temp);
                await _context.SaveChangesAsync();
                return Ok($"Empleado {temp.Nombre} almacenando con éxito.");
            }
            catch (Exception ex)
            {
                return BadRequest($"Error al agregar el empleado {temp.Nombre}, detalle {ex.InnerException}");
            }
        }

        /// <summary>
        /// Método encargado de eliminar a un empleado por medio del email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpDelete("Eliminar")]
        public async Task<string> Eliminar(string email)
        {
            string mensaje = $"Usuario no eliminado, el email: {email} no existe";

            Empleado temp = _context.Empleados.FirstOrDefault(x => x.Email.Equals(email));

            if (temp != null)
            {
                _context.Empleados.Remove(temp);
                await _context.SaveChangesAsync();
                mensaje = $"Empleado {temp.Nombre} eliminado correctamente..";
            }
            return mensaje;
        }

        /// <summary>
        /// Método encargado de editar la información de un empleado
        /// </summary>
        /// <param name="temp"></param>
        /// <returns></returns>
        [HttpPut("Editar")]
        public async Task<string> Editar(Empleado temp)
        {
            var aux = _context.Empleados.FirstOrDefault(x => x.Email.Equals(temp.Email));

            string mensaje = "";
            if (aux != null)
            {
                aux.Nombre = temp.Nombre;

                _context.Empleados.Update(aux);

                await _context.SaveChangesAsync();

                mensaje = $"El empleado {aux.Nombre} se actualizado correctamente";
            }
            else
            {
                mensaje = $"El empleado {temp.Nombre} no existe";
            }
            return mensaje;
        }

        /// <summary>
        /// Método encargado de consultar un empleado por medio del email
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        [HttpGet("Buscar")]
        public Empleado Buscar(string email)
        {
            Empleado temp = null;
            temp = _context.Empleados.FirstOrDefault(x => x.Email.Equals(email));
            return temp == null ? new Empleado
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
        /// Método encargado de manejar la autenticación del empleado
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost("Login")]
        public async Task<IActionResult> AutenticationPW(string email, string password)
        {
            var temp = await _context.Empleados.FirstOrDefaultAsync(u => u.Email.Equals(email) && u.Password.Equals(password));

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
