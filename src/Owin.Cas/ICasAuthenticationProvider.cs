using System.Threading.Tasks;

namespace Owin.Cas
{
    public interface ICasAuthenticationProvider
    {
        Task Authenticated(CasAuthenticatedContext context);

        Task ReturnEndpoint(CasReturnEndpointContext context);
    }
}