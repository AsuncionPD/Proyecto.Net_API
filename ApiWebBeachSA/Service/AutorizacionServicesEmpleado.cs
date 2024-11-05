using ApiWebBeachSA.Data;
using ApiWebBeachSA.Models.Costume;
using ApiWebBeachSA.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ApiWebBeachSA.Service
{
    public class AutorizacionServicesEmpleado:IAutorizacionServicesEmpleado
    {
        private readonly IConfiguration _configuration;
        private readonly DbContextHotel _context;

        public AutorizacionServicesEmpleado(IConfiguration configuration, DbContextHotel context)
        {
            _configuration = configuration;
            _context = context;
        }


        public async Task<AutorizacionResponse> DevolverToken(Empleado empleado)
        {
            var temp = await _context.Empleados.FirstOrDefaultAsync(u => u.Email.Equals(empleado.Email) && u.Password.Equals(empleado.Password));

            if (temp != null)
            {
                string tokenCreado = GenerarToken(empleado.Email.ToString());
                return new AutorizacionResponse() { Token = tokenCreado, Resultado = true, Msj = "Ok", TipoUsuario = temp.TipoUsuario };
            }
            else
            {
                return await Task.FromResult<AutorizacionResponse>(null);
            }
        }


        private string GenerarToken(string IDEmpleado)
        {
            var key = _configuration.GetValue<string>("JwtSettings:key");
            var KeyBytes = Encoding.ASCII.GetBytes(key);

            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, IDEmpleado));

            var credencialesToken = new SigningCredentials(new SymmetricSecurityKey(KeyBytes)
              , SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Subject = claims,
                Expires = DateTime.UtcNow.AddMinutes(20),
                SigningCredentials = credencialesToken
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);
            var tokenCreado = tokenHandler.WriteToken(tokenConfig);

            return tokenCreado;
        }
    }
}
