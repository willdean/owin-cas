using System.Security.Claims;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Provider;

namespace Owin.Cas
{
    /// <summary>
    /// Contains information about the login session as well as the user <see cref="System.Security.Claims.ClaimsIdentity"/>.
    /// </summary>
    public class CasAuthenticatedContext : BaseContext
    {
        /// <summary>
        /// Initializes a <see cref="CasAuthenticatedContext"/>
        /// </summary>
        /// <param name="context">The OWIN environment</param>
        /// <param name="identity">The <see cref="ClaimsIdentity"/> representing the user</param>
        /// <param name="properties">A property bag for common authentication properties</param>
        public CasAuthenticatedContext(
            IOwinContext context,
            ClaimsIdentity identity,
            AuthenticationProperties properties)
            : base(context)
        {
            Identity = identity;
            Properties = properties;
        }

        /// <summary>
        /// Gets or sets the <see cref="ClaimsIdentity"/> representing the user
        /// </summary>
        public ClaimsIdentity Identity { get; set; }

        /// <summary>
        /// Gets or sets a property bag for common authentication properties
        /// </summary>
        public AuthenticationProperties Properties { get; set; }

    }
}