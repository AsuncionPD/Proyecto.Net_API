using ApiWebBeachSA.Models;
using ApiWebBeachSA.Models.Costume;
using ApiWebBeachSA.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;


namespace ApiWebBeachSA.Service
{
    public class AutorizacionServices:IAutorizacionServices
    {
        private readonly IConfiguration _configuration;
        private readonly DbContextHotel _context;

        public AutorizacionServices(IConfiguration configuration, DbContextHotel context)
        {
            _configuration = configuration;
            _context = context;
        }


        public async Task<AutorizacionResponse> DevolverToken(Cliente cliente)
        {
            var temp = await _context.Clientes.FirstOrDefaultAsync(u => u.Email.Equals(cliente.Email) && u.Password.Equals(cliente.Password));

            if (temp != null)
            {
                string tokenCreado = GenerarToken(cliente.Email.ToString());
                return new AutorizacionResponse() { Token = tokenCreado, Resultado = true, Msj = "Ok", TipoUsuario = temp.TipoUsuario ,Cedula= temp.Cedula };
            }
            else
            {
                return await Task.FromResult<AutorizacionResponse>(null);
            }
        }


        private string GenerarToken(string IDCliente)
        {
            var key = _configuration.GetValue<string>("JwtSettings:key");
            var KeyBytes = Encoding.ASCII.GetBytes(key);

            var claims = new ClaimsIdentity();
            claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, IDCliente));

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
