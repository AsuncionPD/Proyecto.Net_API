using ApiWebBeachSA.Models.Costume;
using ApiWebBeachSA.Models;

namespace ApiWebBeachSA.Service
{

    public interface IAutorizacionServicesEmpleado
    {
        Task<AutorizacionResponse> DevolverToken(Empleado autorizacion);
    }

}
