using System.Threading.Tasks;
using owin_cas;

namespace Web2010.Models.Cas
{
    public interface ICasAuthenticationProvider
    {
        Task Authenticated(CasAuthenticatedContext context);

        Task ReturnEndpoint(CasReturnEndpointContext context);
    }
}