using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Refacto.DotNet.Controllers.Exceptions;

namespace Refacto.DotNet.Controllers.Filters
{
    /// <summary>Translates known application exceptions into HTTP error responses.</summary>
    public sealed class DomainExceptionFilter : IExceptionFilter
    {
        /// <summary>Converts domain violations to 422 and missing orders to 404.</summary>
        /// <param name="context">Exception context for the current HTTP request.</param>
        public void OnException(ExceptionContext context)
        {
            int? statusCode = context.Exception switch
            {
                DomainException => StatusCodes.Status422UnprocessableEntity,
                OrderNotFoundException => StatusCodes.Status404NotFound,
                _ => null
            };

            if (statusCode.HasValue)
            {
                context.Result = new ObjectResult(new ProblemDetails
                {
                    Status = statusCode.Value,
                    Title = statusCode == StatusCodes.Status422UnprocessableEntity
                        ? "Unprocessable Entity"
                        : "Not Found",
                    Detail = context.Exception.Message
                })
                {
                    StatusCode = statusCode.Value
                };
                context.ExceptionHandled = true;
            }
        }
    }
}
