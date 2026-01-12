using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SEP490_Robot_FoodOrdering.API.Middleware
{
    public class CustomExceptionHandlerMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<CustomExceptionHandlerMiddleware> _logger;

        public CustomExceptionHandlerMiddleware(RequestDelegate next, ILogger<CustomExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }/*#region test
        public async Task Invoke(HttpContext context)
            {
                try
                {
                    var check = context.User.Identity?.IsAuthenticated;
                    await _next(context);
                }
                catch (CoreException ex)
                {
                    _logger.LogError(ex, ex.Message);
                    context.Response.StatusCode = ex.StatusCode;
                    var result = JsonSerializer.Serialize(new { ex.Code, ex.Message, ex.AdditionalData });
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(result);
                }
                catch (ErrorException ex)
                {
                    _logger.LogError(ex, ex.ErrorDetail.ErrorMessage.ToString());
                    context.Response.StatusCode = ex.StatusCode;
                    var result = JsonSerializer.Serialize(ex.ErrorDetail);
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(result);
                }
                catch (ValidationException ex)
                {
                    _logger.LogError(ex, "Validation error occurred.");
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    var errorResponse = new
                    {
                        
                        errorCode = "Validation Error",
                        errorMessage = ex.Message,  // Serialize only the Message property
                    };
                    var result = JsonSerializer.Serialize(errorResponse);
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unexpected error occurred.");
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    var result = JsonSerializer.Serialize(new { error = $"An unexpected error occurred. Detail{ex.Message}" });
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(result);
                }
        }
        #endregion*/
        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (CoreException ex)
            {
                _logger.LogError(ex, ex.Message);
                context.Response.StatusCode = ex.StatusCode;
                var result = JsonSerializer.Serialize(new
                {
                    statusCode = ex.StatusCode,
                    errorCode = ex.Code,
                    errorMessage = ex.Message,
                    additionalData = ex.AdditionalData
                });
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(result);
            }
            catch (ErrorException ex)
            {
                _logger.LogError(ex, ex.ErrorDetail.ErrorMessage.ToString());
                    context.Response.StatusCode = ex.StatusCode;
                var result = JsonSerializer.Serialize(new
                {
                    statusCode = ex.StatusCode,
                    errorCode = ex.ErrorDetail.ErrorCode,
                    errorMessage = ex.ErrorDetail.ErrorMessage
                });
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(result);
            }
            catch (ValidationException ex)
            {
                _logger.LogError(ex, "Validation error occurred.");
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                var result = JsonSerializer.Serialize(new
                {
                    statusCode = StatusCodes.Status400BadRequest,
                    errorCode = "Validation Error",
                    errorMessage = ex.Message
                });
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                var result = JsonSerializer.Serialize(new
                {
                    statusCode = StatusCodes.Status500InternalServerError,
                    errorCode = "InternalServerError",
                    errorMessage = $"An unexpected error occurred. Detail: {ex.Message}"
                });
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(result);
            }
        }

    }
}
