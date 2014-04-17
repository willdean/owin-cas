using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.Infrastructure;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;

namespace OwinCas
{
    internal class CasAuthenticationHandler : AuthenticationHandler<CasAuthenticationOptions>
    {
        private readonly ILogger _logger;
        private readonly HttpClient _httpClient;

        public CasAuthenticationHandler(HttpClient httpClient, ILogger logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public override async Task<bool> InvokeAsync()
        {
            if (Options.CallbackPath.HasValue && Options.CallbackPath == Request.Path)
            {
                return await InvokeReturnPathAsync();
            }
            return false;
        }

        protected override async Task<AuthenticationTicket> AuthenticateCoreAsync()
        {
            AuthenticationProperties properties = null;

            try
            {
                IReadableStringCollection query = Request.Query;

                properties = UnpackStateParameter(query);
                if (properties == null)
                {
                    _logger.WriteWarning("Invalid return state");
                    return null;
                }

                // Anti-CSRF
                if (!ValidateCorrelationId(properties, _logger))
                {
                    return new AuthenticationTicket(null, properties);
                }

                string ticket = GetTicketParameter(query);
                if (String.IsNullOrEmpty(ticket))
                {
                    // No ticket
                    return new AuthenticationTicket(null, properties);
                }

                // Now, we need to get the ticket validated

                string validateUrl = Options.CasServerUrlBase + "/validate" +
                                     "?service=" + Uri.EscapeDataString(BuildReturnTo(GetStateParameter(query)))+
                                     "&ticket=" + Uri.EscapeDataString(ticket);
                
                HttpResponseMessage response = await _httpClient.GetAsync(validateUrl, Request.CallCancelled);

                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();


                String validatedUserName = null;
                var responseParts = responseBody.Split('\n');
                if (responseParts.Length >= 2)
                {
                    if (responseParts[0] == "yes")
                    {
                        validatedUserName = responseParts[1];
                    }
                }

                if (!String.IsNullOrEmpty(validatedUserName))
                {
                    var identity = new ClaimsIdentity(Options.AuthenticationType);
                    identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, validatedUserName, "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType));
                    identity.AddClaim(new Claim(ClaimTypes.Name, validatedUserName, "http://www.w3.org/2001/XMLSchema#string", Options.AuthenticationType));

                    var context = new CasAuthenticatedContext(
                        Context,
                        identity,
                        properties);

                    await Options.Provider.Authenticated(context);

                    return new AuthenticationTicket(context.Identity, context.Properties);
                }

                return new AuthenticationTicket(null, properties);
            }
            catch (Exception ex)
            {
                _logger.WriteError("Authentication failed", ex);
                return new AuthenticationTicket(null, properties);
            }
        }

        private static string GetStateParameter(IReadableStringCollection query)
        {
            IList<string> values = query.GetValues("state");
            if (values != null && values.Count == 1)
            {
                return values[0];
            }
            return null;
        }

        private static string GetTicketParameter(IReadableStringCollection query)
        {
            IList<string> values = query.GetValues("ticket");
            if (values != null && values.Count == 1)
            {
                return values[0];
            }
            return null;
        }

        private AuthenticationProperties UnpackStateParameter(IReadableStringCollection query)
        {
            string state = GetStateParameter(query);
            if (state != null)
            {
                return Options.StateDataFormat.Unprotect(state);
            }
            return null;
        }

        private string BuildReturnTo(string state)
        {
            return Request.Scheme + "://" + Request.Host +
                   RequestPathBase + Options.CallbackPath +
                   "?state=" + Uri.EscapeDataString(state);
        }
        
        protected override Task ApplyResponseChallengeAsync()
        {
            if (Response.StatusCode != 401)
            {
                return Task.FromResult<object>(null);
            }

            AuthenticationResponseChallenge challenge = Helper.LookupChallenge(Options.AuthenticationType, Options.AuthenticationMode);

            if (challenge != null)
            {
                string requestPrefix = Request.Scheme + Uri.SchemeDelimiter + Request.Host;

                var state = challenge.Properties;
                if (String.IsNullOrEmpty(state.RedirectUri))
                {
                    state.RedirectUri = requestPrefix + Request.PathBase + Request.Path + Request.QueryString;
                }

                // Anti-CSRF
                GenerateCorrelationId(state);

                string returnTo = BuildReturnTo(Options.StateDataFormat.Protect(state));

                string authorizationEndpoint =
                    Options.CasServerUrlBase + "/login" +
                    "?service=" + Uri.EscapeDataString(returnTo);

                Response.StatusCode = 302;
                Response.Headers.Set("Location", authorizationEndpoint);
            }

            return Task.FromResult<object>(null);
        }

        public async Task<bool> InvokeReturnPathAsync()
        {
            AuthenticationTicket model = await AuthenticateAsync();
            if (model == null)
            {
                _logger.WriteWarning("Invalid return state, unable to redirect.");
                Response.StatusCode = 500;
                return true;
            }

            var context = new CasReturnEndpointContext(Context, model);
            context.SignInAsAuthenticationType = Options.SignInAsAuthenticationType;
            context.RedirectUri = model.Properties.RedirectUri;
            model.Properties.RedirectUri = null;

            await Options.Provider.ReturnEndpoint(context);

            if (context.SignInAsAuthenticationType != null && context.Identity != null)
            {
                ClaimsIdentity signInIdentity = context.Identity;
                if (!string.Equals(signInIdentity.AuthenticationType, context.SignInAsAuthenticationType, StringComparison.Ordinal))
                {
                    signInIdentity = new ClaimsIdentity(signInIdentity.Claims, context.SignInAsAuthenticationType, signInIdentity.NameClaimType, signInIdentity.RoleClaimType);
                }
                Context.Authentication.SignIn(context.Properties, signInIdentity);
            }

            if (!context.IsRequestCompleted && context.RedirectUri != null)
            {
                if (context.Identity == null)
                {
                    // add a redirect hint that sign-in failed in some way
                    context.RedirectUri = WebUtilities.AddQueryString(context.RedirectUri, "error", "access_denied");
                }
                Response.Redirect(context.RedirectUri);
                context.RequestCompleted();
            }

            return context.IsRequestCompleted;
        }
    }
}