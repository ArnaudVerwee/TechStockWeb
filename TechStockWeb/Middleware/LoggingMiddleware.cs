using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace TechStockWeb.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew(); 
            var path = context.Request.Path;
            var method = context.Request.Method;

            Debug.WriteLine($"start request : {method} {path}");

            await _next(context); 

            stopwatch.Stop(); 
            Debug.WriteLine($" end request : {method} {path} - Time : {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
