using Domain.Constants;
using Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Shared.Domain.Models;

namespace Presentation.Filters;

public class ExceptionHandleMiddleware : IExceptionHandler
{
    private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;

    private readonly ILogger<ExceptionHandleMiddleware> _logger;

    private readonly IStringLocalizer<ExceptionHandleMiddleware> _localizer;

    public ExceptionHandleMiddleware(ILogger<ExceptionHandleMiddleware> logger, IStringLocalizer<ExceptionHandleMiddleware> localizer)
    {
        _logger = logger;
        _localizer = localizer;

        // Register known exception types and handlers.
        _exceptionHandlers = new()
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(NotFoundException), HandleNotFoundException },
            { typeof(BadRequestException), HandleBadRequestException },
            { typeof(AlreadyExistException), HandleAlreadyExistException },
        };
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError("Error Message: {ExceptionMessage}, Time of occurrence {Time}", exception.Message, DateTime.UtcNow);

        var exceptionType = exception.GetType();

        if (!_exceptionHandlers.TryGetValue(exceptionType, out var value))
        {
            return false;
        }

        await value.Invoke(httpContext, exception);
        return true;
    }

    private async Task HandleValidationException(HttpContext httpContext, Exception ex)
    {
        var exception = (ValidationException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var result = new ValidationProblemDetails()
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = _localizer[Localized.ValidationExceptionTitle],
            Errors = exception.Errors
        };

        var correlation = AsyncStorage<Correlation>.Retrieve();

        if (correlation is not null)
        {
            result.Extensions.Add("correlationId", correlation.Id);
        }

        await httpContext.Response.WriteAsJsonAsync(result);
    }

    private async Task HandleNotFoundException(HttpContext httpContext, Exception ex)
    {
        var exception = (NotFoundException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        var result = new ProblemDetails()
        {
            Status = StatusCodes.Status404NotFound,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            Title = _localizer[Localized.NotFoundExceptionTitle],
            Detail = _localizer[Localized.NotFoundExceptionMessage, exception.Name, exception.Key]
        };

        var correlation = AsyncStorage<Correlation>.Retrieve();

        if (correlation is not null)
        {
            result.Extensions.Add("correlationId", correlation.Id);
        }

        await httpContext.Response.WriteAsJsonAsync(result);
    }

    private async Task HandleBadRequestException(HttpContext httpContext, Exception ex)
    {
        var exception = (BadRequestException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var result = new ProblemDetails()
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = _localizer[Localized.BadRequestExceptionTitle],
            Detail = _localizer[exception.LocalizedMessage, exception.Arguments]
        };

        var correlation = AsyncStorage<Correlation>.Retrieve();

        if (correlation is not null)
        {
            result.Extensions.Add("correlationId", correlation.Id);
        }

        await httpContext.Response.WriteAsJsonAsync(result);
    }

    private async Task HandleAlreadyExistException(HttpContext httpContext, Exception ex)
    {
        var exception = (AlreadyExistException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        var result = new ProblemDetails()
        {
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = _localizer[Localized.AlreadyExistExceptionTitle],
            Detail = _localizer[Localized.AlreadyExistExceptionMessage, exception.Name, exception.Key]
        };

        var correlation = AsyncStorage<Correlation>.Retrieve();

        if (correlation is not null)
        {
            result.Extensions.Add("correlationId", correlation.Id);
        }

        await httpContext.Response.WriteAsJsonAsync(result);
    }
}