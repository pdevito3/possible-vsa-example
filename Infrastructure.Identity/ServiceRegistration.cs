namespace Infrastructure.Identity
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceRegistration
    {
        public static void AddIdentityInfrastructure(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            if(env.EnvironmentName != "IntegrationTesting")
            {
                services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.Authority = configuration["JwtSettings:Authority"];
                        options.Audience = configuration["JwtSettings:Audience"];
                    });
            }

            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanReadPatients", 
                    policy => policy.RequireClaim("scope", "patients.read"));
                options.AddPolicy("CanAddPatients", 
                    policy => policy.RequireClaim("scope", "patients.add"));
                options.AddPolicy("CanDeletePatients", 
                    policy => policy.RequireClaim("scope", "patients.delete"));
                options.AddPolicy("CanUpdatePatients", 
                    policy => policy.RequireClaim("scope", "patients.update"));
            });
        }
    }
}
