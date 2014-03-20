using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure;

namespace MetricsSdkSample
{
    internal static class TokenCredentialHelper
    {
        internal static TokenCloudCredentials SignIn(string subscriptionId, string tenant, string clientId, string redirectUri)
        {
            var token = GetAuthorizationHeader(tenant, clientId, redirectUri);
            return new TokenCloudCredentials(subscriptionId, token);
        }

        static string GetAuthorizationHeader(string tenant, string clientId, string redirectUri)
        {
            AuthenticationResult result = null;

            var context = new AuthenticationContext(
                string.Format("https://login.windows.net/{0}", tenant)
                );

            var thread = new Thread(() =>
            {
                result = context.AcquireToken(
                    clientId: clientId,
                    redirectUri: new Uri(redirectUri),
                    resource: "https://management.core.windows.net/",
                    promptBehavior:PromptBehavior.Auto
                    );
            });

            thread.SetApartmentState((ApartmentState.STA));
            thread.Name = "AcquireTokenThread";
            thread.Start();
            thread.Join();

            return result.CreateAuthorizationHeader().Substring("Bearer ".Length);
        }
    }
}
