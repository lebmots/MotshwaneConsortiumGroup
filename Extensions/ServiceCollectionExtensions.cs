using MotshwaneConsortiumGroup.Services;
using MotshwaneConsortiumGroup.Services.InMemory;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// The single place where application services are registered.
    /// When the database is ready, swap the InMemory* classes for the EF Core versions here.
    /// Controllers only ever depend on the interfaces, so nothing else changes.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Demo data holder: holds the shared lists, so it must stay a singleton.
        // Remove this line when the InMemory services are retired.
        services.AddSingleton<DemoDataService>();

        // Scoped = one instance per web request. This is the lifetime the EF Core versions will need.
        services.AddScoped<IBookingService, InMemoryBookingService>();
        services.AddScoped<ICatalogService, InMemoryCatalogService>();
        services.AddScoped<ICustomerService, InMemoryCustomerService>();
        services.AddScoped<IStaffJobService, InMemoryStaffJobService>();
        services.AddScoped<IStaffService, InMemoryStaffService>();

        return services;
    }
}
