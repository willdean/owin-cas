using System.Threading.Tasks;

namespace OwinCas
{
    public interface ICasAuthenticationProvider
    {
        Task Authenticated(CasAuthenticatedContext context);

        Task ReturnEndpoint(CasReturnEndpointContext context);
    }
}