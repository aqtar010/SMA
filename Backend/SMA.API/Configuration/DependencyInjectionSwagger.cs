using Microsoft.OpenApi;

namespace SMA.API.Configuration
{
    public static class DependencyInjectionSwagger
    {
        public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description = "Enter: Bearer {token}",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                };

                options.AddSecurityDefinition("Bearer", securityScheme);

                options.AddSecurityRequirement(doc=>new OpenApiSecurityRequirement
                {
                    [
                        new OpenApiSecuritySchemeReference("Bearer",doc)
                    ] = []
                });
            });

            return services;
        }
    }
}