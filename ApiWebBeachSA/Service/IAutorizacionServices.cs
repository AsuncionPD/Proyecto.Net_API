using ApiWebBeachSA.Models;
using ApiWebBeachSA.Models.Costume;
using ApiWebBeachSA.Controllers;

namespace ApiWebBeachSA.Service
{
    public interface IAutorizacionServices
    {
        Task<AutorizacionResponse> DevolverToken(Cliente autorizacion);
    }
}
