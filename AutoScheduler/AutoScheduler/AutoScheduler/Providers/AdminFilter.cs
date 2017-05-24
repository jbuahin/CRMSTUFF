using Hangfire.Dashboard;
using Microsoft.Owin;
using System.Collections.Generic;

namespace AutoScheduler.Providers
{
    public class AdminFilter : IAuthorizationFilter
    {
        public bool Authorize(IDictionary<string, object> owinEnvironment)
        {
            // In case you need an OWIN context, use the next line,
            // `OwinContext` class is the part of the `Microsoft.Owin` package.
            var context = new OwinContext(owinEnvironment);

            // Allow all authenticated prenticeworx users to see the Dashboard (potentially dangerous).
            return context.Authentication.User.Identity.Name.Contains("prenticeworx.com");
        }
    }
}