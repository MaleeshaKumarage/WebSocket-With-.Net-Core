using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSocketExample.Server.Middleware
{
    public static class WebSocketServerMiddlewareExtention
    {
        public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder builder)
        {

            return builder.UseMiddleware<WebSocketServerMiddleware>();
        }
        public static IServiceCollection AddWebSocketManager(this IServiceCollection services)
        {

            services.AddSingleton<WebSocketServerConnectionManager>();
            return services;

        }
    }
}
