namespace WebApi
{
    using Application;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Infrastructure.Persistence;
    using Infrastructure.Shared;
    using Infrastructure.Persistence.Seeders;
    using Infrastructure.Persistence.Contexts;
    using WebApi.Extensions;
    using Serilog;
    using Infrastructure.Identity;
    using MediatR;
    using FluentValidation.AspNetCore;
    using System.Reflection;
    using AutoMapper;

    public class StartupDevelopment
    {
        public IConfiguration _config { get; }
        public IWebHostEnvironment _env { get; }

        public StartupDevelopment(IConfiguration configuration, IWebHostEnvironment env)
        {
            _config = configuration;
            _env = env;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCorsService("MyCorsPolicy");
            services.AddPersistenceInfrastructure(_config);
            services.AddIdentityInfrastructure(_config, _env);
            services.AddSharedInfrastructure(_config);
            services.AddControllers()
                .AddNewtonsoftJson();
            services.AddApiVersioningExtension();
            services.AddHealthChecks();
            services.AddMediatR(typeof(Startup));
            services.AddMvc()
                .AddFluentValidation(cfg => { cfg.RegisterValidatorsFromAssemblyContaining<Startup>(); });
            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            #region Dynamic Services
            services.AddSwaggerExtension(_config);
            #endregion
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage();

            app.UseHttpsRedirection();

            #region Entity Context Region - Do Not Delete

                using (var context = app.ApplicationServices.GetService<VerticalLabTestPostgresDbContext>())
                {
                    context.Database.EnsureCreated();

                    #region VerticalLabTestPostgresDbContext Seeder Region - Do Not Delete
                    
                    PatientSeeder.SeedSamplePatientData(app.ApplicationServices.GetService<VerticalLabTestPostgresDbContext>());
                    SampleSeeder.SeedSampleSampleData(app.ApplicationServices.GetService<VerticalLabTestPostgresDbContext>());
                    #endregion
                }

            #endregion

            app.UseCors("MyCorsPolicy");

            app.UseSerilogRequestLogging();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseErrorHandlingMiddleware();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/api/health");
                endpoints.MapControllers();
            });

            #region Dynamic App
            app.UseSwaggerExtension(_config);
            #endregion
        }
    }
}
