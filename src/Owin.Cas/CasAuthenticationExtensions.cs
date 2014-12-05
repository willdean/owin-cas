using System;
using Owin;

namespace Owin.Cas
{
    /// <summary>
    /// Extension methods for using <see cref="CasAuthenticationMiddleware"/>
    /// </summary>
    public static class CasAuthenticationExtensions
    {
        /// <summary>
        /// Authenticate users using Cas
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="options">Middleware configuration options</param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseCasAuthentication(this IAppBuilder app, CasAuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            app.Use(typeof(CasAuthenticationMiddleware), app, options);
            return app;
        }

        /// <summary>
        /// Authenticate users using Cas
        /// </summary>
        /// <param name="app">The <see cref="IAppBuilder"/> passed to the configuration method</param>
        /// <param name="serverUrl"></param>
        /// <returns>The updated <see cref="IAppBuilder"/></returns>
        public static IAppBuilder UseCasAuthentication(
            this IAppBuilder app, String serverUrl)
        {
            return UseCasAuthentication(
                app,
                new CasAuthenticationOptions() { CasServerUrlBase = serverUrl });
        }
    }
}