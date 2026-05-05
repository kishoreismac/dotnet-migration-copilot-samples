using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ContosoUniversity
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Initialise the OpenTelemetry logging pipeline first so that all
            // subsequent startup log messages are captured.
            LoggingService.Initialize();

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            
            // Initialize database with EF Core
            InitializeDatabase();
        }

        protected void Application_End()
        {
            // Flush and dispose the OpenTelemetry pipeline to ensure any
            // buffered log records are exported before the process exits.
            LoggingService.Shutdown();
        }

        private void InitializeDatabase()
        {
            var connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            var optionsBuilder = new DbContextOptionsBuilder<SchoolContext>();
            optionsBuilder.UseSqlServer(connectionString);
            
            using (var context = new SchoolContext(optionsBuilder.Options))
            {
                DbInitializer.Initialize(context);
            }
        }
    }
}

