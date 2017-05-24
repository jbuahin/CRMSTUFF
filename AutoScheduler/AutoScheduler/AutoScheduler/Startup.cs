﻿using Microsoft.Owin;
using Owin;
using Hangfire;

[assembly: OwinStartup(typeof(AutoScheduler.Startup))]

namespace AutoScheduler
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            GlobalConfiguration.Configuration.UseSqlServerStorage("DefaultConnection");
            app.UseHangfireServer();
            app.UseHangfireDashboard("/hangfireadmin", new DashboardOptions
            {
                AuthorizationFilters = new[] {new Providers.AdminFilter() }
            });
        }
    }
}
