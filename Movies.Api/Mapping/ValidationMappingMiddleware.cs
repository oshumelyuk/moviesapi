using FluentValidation;
using Movies.Contracts.Responses;

namespace Movies.Api.Mapping;

public class ValidationMappingMiddleware
{
    private readonly RequestDelegate _next;

    public ValidationMappingMiddleware(RequestDelegate next)
    {
        _next = next;   
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = 400;
            var validationFailureResponse = new ValidationFailureResponse()
            {
                Errors = ex.Errors.Select(x => new ValidationResponse()
                {
                    Message = x.ErrorMessage,
                    PropertyName = x.PropertyName
                })
            };
            await context.Response.WriteAsJsonAsync(validationFailureResponse);
        }
    }
}