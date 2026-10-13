using System.Net;
using System.Text.Json;
using FluentValidation;
using FoodDelivery.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FoodDelivery.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var traceId = context.TraceIdentifier;
        _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}, Message: {Message}", traceId, exception.Message);

        var (statusCode, title, detail, extensions) = exception switch
        {
            ValidationException validationException => (
                HttpStatusCode.BadRequest,
                "Validation Error",
                "One or more validation errors occurred.",
                GetValidationErrors(validationException)
            ),
            NotFoundException notFoundException => (
                HttpStatusCode.NotFound,
                "Resource Not Found",
                notFoundException.Message,
                null
            ),
            InvalidOrderOperationException invalidOpException => (
                HttpStatusCode.UnprocessableEntity,
                "Invalid Order Operation",
                invalidOpException.Message,
                null
            ),
            ArgumentException argException => (
                HttpStatusCode.BadRequest,
                "Bad Request",
                argException.Message,
                null
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                "Internal Server Error",
                _env.IsDevelopment() ? exception.Message : "An unexpected internal server error occurred. Please contact support.",
                null
            )
        };

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;
        if (extensions != null)
        {
            problemDetails.Extensions["errors"] = extensions;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static Dictionary<string, string[]> GetValidationErrors(ValidationException validationException)
    {
        return validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => JsonNamingPolicy.CamelCase.ConvertName(g.Key),
                g => g.Select(e => e.ErrorMessage).ToArray()
            );
    }
}
