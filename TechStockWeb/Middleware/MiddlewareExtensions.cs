using Microsoft.AspNetCore.Builder;



namespace TechStockWeb.Middleware
    {
        public static class MiddlewareExtensions
        {
            public static IApplicationBuilder UseLoggingMiddleware(this IApplicationBuilder builder)
            {
                return builder.UseMiddleware<LoggingMiddleware>();
            }
        }
    }
