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
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Everwell.API",
                    Version = "v1",
                    Description = "A Gender Healthcare Service System "
                });
                options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter your token:"
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                });
            });

            return services;
        }
    }
}
