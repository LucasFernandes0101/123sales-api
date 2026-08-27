using Microsoft.OpenApi.Models;

namespace _123vendas_server.v1.Configurations;

public static class SwaggerExtension
{
    public static void AddSwaggerConfig(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.CustomSchemaIds(type => type.FullName);
            c.SwaggerDoc("v1", new()
            {
                Title = "123Vendas - API de Vendas",
                Description = "Esta API permite o gerenciamento completo de vendas na plataforma 123Vendas, incluindo operações de cadastro, edição, consulta, exclusão e registro de eventos relacionados às vendas.",
                Version = "v1"
            });
            c.AddSecurityDefinition("Bearer", new()
            {
                Description = "Autenticação via JWT. Insira o token no campo abaixo **sem** o prefixo `Bearer `.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer"
            });
            c.AddSecurityRequirement(new()
            {
                {
                    new()
                    {
                        Reference = new()
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }
}