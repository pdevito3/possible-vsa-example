namespace Infrastructure.Persistence
{
    using Infrastructure.Persistence.Contexts;
    using Application.Interfaces.Sample;
    using Application.Interfaces.Patient;
    using Infrastructure.Persistence.Repositories;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Sieve.Services;

    public static class ServiceRegistration
    {
        public static void AddPersistenceInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            #region DbContext -- Do Not Delete            
            if (configuration.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<VerticalLabTestPostgresDbContext>(options =>
                    options.UseInMemoryDatabase($"VerticalLabTestPostgresDbContext"));
            }
            else
            {
                services.AddDbContext<VerticalLabTestPostgresDbContext>(options =>
                    options.UseNpgsql(
                        configuration.GetConnectionString("VerticalLabTestPostgresDbContext"),
                        builder => builder.MigrationsAssembly(typeof(VerticalLabTestPostgresDbContext).Assembly.FullName)));
            }
            #endregion

            services.AddScoped<SieveProcessor>();

            #region Repositories -- Do Not Delete
            services.AddScoped<ISampleRepository, SampleRepository>();
            services.AddScoped<IPatientRepository, PatientRepository>();
            #endregion
        }
    }
}
