using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using System.Runtime.CompilerServices;

namespace Everwell.API.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSwaggerGenWithAuth(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                var securityScheme = new OpenApiSecurityScheme
                {
                    Name = "JWT Authorization",
                    Description = "Enter JWT token to access protected endpoints",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme,
                    BearerFormat = "JWT"
                };

                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);

                var securityRequirement = new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            }
                        },
                        new List<string>()
                    }
                }; 

                options.SwaggerDoc("v2.5", new OpenApiInfo
                {
                    Title = "Everwell.API",
                    Version = "v2.5",
                    Description = "A Gender Healthcare Service System "
                });

                options.AddSecurityRequirement(securityRequirement);
            });

            return services;
        }
    }
}
