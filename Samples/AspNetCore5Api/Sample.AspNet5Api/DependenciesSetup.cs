using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Salix.Dapper.Cqrs.Abstractions;
using Salix.Dapper.Cqrs.MsSql;
using Sample.AspNet5Api.Logic;

namespace Sample.AspNet5Api
{
    public static class DependenciesSetup
    {
        /// <summary>
        /// Registers logic and other dependencies with ASP.Net IoC container (services).
        /// </summary>
        /// <param name="services">ASP.Net built in IoC container.</param>
        public static void RegisterLogicDependencies(this IServiceCollection services)
        {
            // Database access "magic" - registering all parts with Dependency Injection (DI, IoC) container (services).
            // Scoped - object created once per request, discarded (closed) after request ends
            // For Database Context - using factory to supply connection string and logger (from same IoC container).
            services.AddScoped<IMsSqlContext, DatabaseContext>(svc =>
                new DatabaseContext(
                    svc.GetService<DatabaseConfiguration>().ConnectionString,
                    svc.GetService<ILogger<DatabaseContext>>()));
            services.AddScoped<IDatabaseSession, SqlDatabaseSession>();
            services.AddScoped<ICommandQueryContext, CommandQueryContext>();

            // Logic class registration
            services.AddScoped<IArtistsLogic, ArtistsLogic>();
            services.AddScoped<IAlbumsLogic, AlbumsLogic>();
        }
    }
}
