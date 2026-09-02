using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Web.Fiap.Carbono.Config.Swagger;

public sealed class AuthorizationOperationFilter : IOperationFilter
{
    public void Apply(
        OpenApiOperation operation,
        OperationFilterContext context
    )
    {
        var controllerAttributes = context.MethodInfo.DeclaringType?
            .GetCustomAttributes(true) ?? [];
        var actionAttributes = context.MethodInfo.GetCustomAttributes(true);
        var attributes = controllerAttributes
            .Concat(actionAttributes)
            .ToArray();

        if (attributes.OfType<AllowAnonymousAttribute>().Any())
        {
            return;
        }

        var authorization = attributes
            .OfType<AuthorizeAttribute>()
            .ToArray();

        if (authorization.Length == 0)
        {
            return;
        }

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                [new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                }] = Array.Empty<string>()
            }
        );

        operation.Responses.TryAdd(
            StatusCodes.Status401Unauthorized.ToString(),
            new OpenApiResponse { Description = "Unauthorized" }
        );

        if (authorization.Any(attribute =>
                !string.IsNullOrWhiteSpace(attribute.Roles)))
        {
            operation.Responses.TryAdd(
                StatusCodes.Status403Forbidden.ToString(),
                new OpenApiResponse { Description = "Forbidden" }
            );
        }
    }
}
