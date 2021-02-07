using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Utf8Json;
using Serilog;

namespace CloudAPI.CustomMiddlewares
{
    public class CustomExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        ILogger _logger;

        public CustomExceptionMiddleware(RequestDelegate next, ILogger logger) {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke(HttpContext context) {
            try {
                await _next.Invoke(context);
            }
            catch(KeyNotFoundException ex) {
                await HandleExceptionAsync(_logger, context, ex, (int)HttpStatusCode.NotFound).ConfigureAwait(false);
            }
            catch(UnauthorizedAccessException ex) {
                await HandleExceptionAsync(_logger, context, ex, (int)HttpStatusCode.Unauthorized).ConfigureAwait(false);
            }
            catch(Exception ex) {
                await HandleExceptionAsync(_logger, context, ex, (int)HttpStatusCode.InternalServerError).ConfigureAwait(false);
            }
        }
        private static Task HandleExceptionAsync(ILogger logger, HttpContext context, Exception exception, int statusCode) {
            logger.Error(exception.ToString());

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            return context.Response.WriteAsync(exception.Message);
        }
    }
}
