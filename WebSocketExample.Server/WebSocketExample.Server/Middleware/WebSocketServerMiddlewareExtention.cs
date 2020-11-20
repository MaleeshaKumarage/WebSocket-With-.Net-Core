using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebSocketExample.Server.Middleware
{
    public static class WebSocketServerMiddlewareExtention
    {
        public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder builder) {

            return builder.UseMiddleware<WebSocketServerMiddleware>();
        }
    }
}
