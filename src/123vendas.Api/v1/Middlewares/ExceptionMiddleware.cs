using _123vendas.Application.DTOs.Common;
using _123vendas.Domain.Base;
using _123vendas.Domain.Exceptions;
using FluentValidation;
using Newtonsoft.Json;

namespace _123vendas_server.v1.Middlewares;

public class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger)
{
    public async Task Invoke(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var statusCode = (int)ExceptionStatusCodes.GetExceptionStatusCode(ex);
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        logger.LogError(ex, "An error occurred while processing the request at {Url}", context.Request.Path);

        var errorResponse = ex switch
        {
            ValidationException validationEx => CreateValidationErrorResponseDTO(validationEx),
            BaseException baseEx => CreateBaseErrorResponseDTO(baseEx),
            _ => CreateGenericErrorResponseDTO(ex)
        };

        var result = JsonConvert.SerializeObject(errorResponse);

        await context.Response.WriteAsync(result);
    }

    private static ErrorResponseDTO CreateValidationErrorResponseDTO(ValidationException validationEx)
    => new()
    {
        Type = nameof(validationEx),
        Error = "Invalid input data",
        Detail = string.Join(", ", validationEx.Errors.Select(e => e.ErrorMessage))
    };

    private static ErrorResponseDTO CreateBaseErrorResponseDTO(BaseException baseEx)
    => new()
    {
        Type = nameof(baseEx),
        Error = baseEx.Message,
        Detail = IsTestEnvironment ? baseEx.StackTrace : string.Empty
    };

    private static ErrorResponseDTO CreateGenericErrorResponseDTO(Exception ex)
    => new()
    {
        Type = nameof(ex),
        Error = "An unexpected error occurred. Please try again later.",
        Detail = IsTestEnvironment ? ex.StackTrace : string.Empty
    };

    private static bool IsTestEnvironment =>
        string.Equals(EnvironmentName, "Development", StringComparison.OrdinalIgnoreCase) ||
        string.Equals(EnvironmentName, "Homologation", StringComparison.OrdinalIgnoreCase);

    private static readonly string? EnvironmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
}