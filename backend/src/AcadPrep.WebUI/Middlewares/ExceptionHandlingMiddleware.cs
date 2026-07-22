using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace WebUI.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while handling the request: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        object responseBody;

        if (exception is ValidationException validationEx)
        {
            statusCode = StatusCodes.Status400BadRequest;
            responseBody = new
            {
                IsSuccess = false,
                Error = "Invalid input data.",
                Code = StatusCodes.Status400BadRequest,
                ValidationErrors = validationEx.Errors
                    .Select(e => new { Field = e.PropertyName, Message = e.ErrorMessage })
                    .ToList()
            };
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;
            responseBody = new
            {
                IsSuccess = false,
                Error = "A critical system error occurred. Please contact the administrator.",
                Code = StatusCodes.Status500InternalServerError,
                ValidationErrors = (object?)null
            };
        }

        context.Response.StatusCode = statusCode;
        
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var jsonResponse = JsonSerializer.Serialize(responseBody, jsonOptions);

        await context.Response.WriteAsync(jsonResponse);
    }
}
