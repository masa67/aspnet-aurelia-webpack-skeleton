using Client.DataManager;
using Client.Models;
using Client.Repository;
using Microsoft.AspNet.OData.Builder;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OData.Edm;

namespace Client
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOData();

            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddDbContext<CustomerContext>(options => options.UseSqlServer(Configuration["ConnectionStrings:Default"]));
            services.AddScoped<IDataRepository<Customer>, CustomerManager>();
            services.AddScoped<IDataRepository<ContactPerson>, ContactPersonManager>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseWebpackDevMiddleware(new WebpackDevMiddlewareOptions
                {
                    ConfigFile = "webpack.development.js",
                    HotModuleReplacement = true,
                });
            }

            // Serve all static files 
            app.UseStaticFiles();
            app.UseMvc(routes =>
            {
                routes.MapODataServiceRoute("odata", "odata", GetEdmModel());

                routes.MapRoute(
                    name: "default",
                    template: "{controller=SpaIndex}/{action=Index}");

                routes.MapSpaFallbackRoute(
                    name: "spa-fallback",
                    defaults: new { controller = "SpaIndex", action = "Index" });
            });
        }

        private static IEdmModel GetEdmModel()
        {
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Address>("Addresses");
            builder.EntitySet<ContactPerson>("ContactPersons");
            builder.EntitySet<Customer>("Customers");
            builder.EntitySet<PhoneNumber>("PhoneNumbers");
            builder.EntitySet<Role>("Roles");
            builder.EntitySet<RoleGroup>("RoleGroups");

            return builder.GetEdmModel();
        }
    }
}
