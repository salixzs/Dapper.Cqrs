using System.Collections.Generic;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Salix.AspNetCore.Utilities;

namespace Sample.AspNet5Api.Services
{
    /// <summary>
    /// Frontend page show functionality to avoid having 404 page does not exist.
    /// Also giving nicer output of some description.
    /// </summary>
    [ApiController]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : ControllerBase
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly HealthCheckService _healthChecks;
        private readonly IConfigurationValuesLoader _configLoader;
        private const string HealthTestEndpoint = "/api/healthtest";

        /// <summary>
        /// Home controller for two pages, available from Utilities.
        /// </summary>
        /// <param name="hostingEnvironment">Hosting environment.</param>
        /// <param name="logic">Demonstration business logic (throwing errors).</param>
        /// <param name="healthChecks">ASP.Net built in health checking services. DO NOT INJECT this, if you do not have Health checks configured in API.</param>
        /// <param name="logger">Logging object.</param>
        public HomeController(IWebHostEnvironment hostingEnvironment, HealthCheckService healthChecks, IConfigurationValuesLoader configLoader)
        {
            _hostingEnvironment = hostingEnvironment;
            _healthChecks = healthChecks;
            _configLoader = configLoader;
        }

        /// <summary>
        /// Retrieves simple frontend/index page to display when API is open on its base URL.
        /// </summary>
        [HttpGet("/")]
        public ContentResult Index()
        {
            Dictionary<string, string> configurationItems =
                _configLoader.GetConfigurationValues(new HashSet<string>
                {
                    "Logging", "Database"
                });

            var apiAssembly = Assembly.GetAssembly(typeof(Startup));
            IndexPage indexPage = new IndexPage("Sample API")
                .SetDescription("Database access through Dapper with CQRS.")
                .SetHostingEnvironment(_hostingEnvironment.EnvironmentName)
                .SetVersionFromAssembly(apiAssembly, 2)
                .SetBuildTimeFromAssembly(apiAssembly)
                .SetHealthPageUrl(HealthTestEndpoint)
                .SetSwaggerUrl("/swagger/index.html")
                .SetConfigurationValues(configurationItems);

#if DEBUG
            indexPage.SetBuildMode("#DEBUG (Should not be in production!)");
#else
            indexPage.SetBuildMode("Release");
#endif

            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = indexPage.GetContents(),
            };
        }

        /// <summary>
        /// Displays separate Health status page, extracted from standard Health Check report.
        /// Added few testing links to showcase possibility to add custom links for API testing.
        /// </summary>
        [HttpGet(HealthTestEndpoint)]
        public async Task<ContentResult> HealthTest()
        {
            HealthReport healthResult = await _healthChecks.CheckHealthAsync().ConfigureAwait(false);
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = (int)HttpStatusCode.OK,
                Content = HealthTestPage.GetContents(
                    healthResult,
                    "/api/health",
                    new List<HealthTestPageLink>
                    {
                        new HealthTestPageLink { TestEndpoint = "/api/artists", Name = "Artists", Description = "Returns all records." },
                        new HealthTestPageLink { TestEndpoint = "/api/artists/150", Name = "U2", Description = "Returns single record." },
                        new HealthTestPageLink { TestEndpoint = "/api/artists/150/albums", Name = "U2 albums", Description = "Returns multiple queries results." },
                    }),
            };
        }
    }
}
