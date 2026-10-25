using Microsoft.EntityFrameworkCore;
using MotshwaneConsortiumGroup.Data;
using MotshwaneConsortiumGroup.Services.EfCore;
using MotshwaneConsortiumGroup.Services.FileSystem;
using MotshwaneConsortiumGroup.Services.Interfaces;

namespace MotshwaneConsortiumGroup.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// The single place where application services are registered. Backed by the real
    /// SQL Server database via ApplicationDbContext (see Data/ApplicationDbContext.cs).
    /// Controllers only ever depend on the interfaces, so swapping an implementation
    /// (e.g. for a different database later) only ever changes this file.
    /// </summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, EfBookingService>();
        services.AddScoped<ICatalogService, EfCatalogService>();
        services.AddScoped<ICustomerService, EfCustomerService>();
        services.AddScoped<IStaffJobService, EfStaffJobService>();
        services.AddScoped<IStaffService, EfStaffService>();
        services.AddScoped<IPaymentService, EfPaymentService>();
        services.AddScoped<IFileStorageService, LocalFileStorageService>();

        return services;
    }
}
