using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiWebBeachSA.Data;
using ApiWebBeachSA.Models;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static ApiWebBeachSA.Models.Cliente;


namespace ApiWebBeachSA.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientesController : Controller
    {
        //Variable para utilizar la referencia der ORM Entity Framework core
        private readonly DbContextHotel _context = null;

        public ClientesController(DbContextHotel pContext)
        {
            _context = pContext;
        }

        //metodo encargado de mostrar la informacion de todos los clientes 
        [HttpGet("Listado")]
        public List<Cliente> Listado()
        {
            List<Cliente> lista = null;
            lista = _context.Clientes.ToList();
            return lista;
        }

        //metodo encargado de agregar un cliente 
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

        //metodo encargado de eliminar a un cliente por medio de la cedula
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

        //metodo encargado de editar la informacion de un cliente 
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

        //Metodo encargado de consultar un cliente por medio de la cedula 
        [HttpGet("Buscar")]
        public Cliente Buscar(int cedula)
        {
            Cliente temp = null;
            temp = _context.Clientes.FirstOrDefault(x => x.Cedula == cedula);
            return temp == null ? new Cliente
                () { Nombre = "No existe" } : temp;
        }

        //Validar los requisitos basicos de una contraseña 
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

        [HttpPost("Login")]
        public async Task<string> Login([Bind] LoginDto loginDto)
        {
            if (loginDto == null)
            {
                return "Por favor ingrese los datos";
            }

            var validation = ValidarUsuario(loginDto);
            if (validation != null)
            {
                var userClaims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, loginDto.Email),
                    new Claim(ClaimTypes.Role, "User")
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("Key_Jwt_=*ApiHotelBeachSAWebAspNet");

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(userClaims),
                    Expires = DateTime.UtcNow.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return $"Inicio de sesión exitoso. Token: {tokenString}";
            }
            else
            {
                return "El usuario o la contraseña están mal";
            }
        }

        private Cliente ValidarUsuario(LoginDto loginDto)
        {
            var temp = _context.Clientes.FirstOrDefault(x => x.Email == loginDto.Email);
            if (temp == null || !temp.Password.Equals(loginDto.Password))
            {
                return null;
            }
            return temp;
        }

    }
}
