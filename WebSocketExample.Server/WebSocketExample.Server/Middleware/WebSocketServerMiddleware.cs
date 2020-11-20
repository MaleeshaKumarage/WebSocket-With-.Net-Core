using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketExample.Server.Middleware
{
    public class WebSocketServerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly WebSocketServerConnectionManager _manager;

        public WebSocketServerMiddleware(RequestDelegate next, WebSocketServerConnectionManager manager)
        {
            _next = next;
            _manager = manager;
        }

        public async Task SendConnIDAsync(WebSocket socket, string ConnID)
        {
            var buffer = Encoding.UTF8.GetBytes("ConnID : " + ConnID);
            await socket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken: CancellationToken.None);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                Console.WriteLine("WebSocket Connected.");

                string ConnId = _manager.AddSocket(webSocket);
                await SendConnIDAsync(webSocket, ConnId);
                await ReceiveMessage(webSocket, async (result, buffer) =>
                {
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        Console.WriteLine("Message Recived");
                        Console.WriteLine($"Message : { Encoding.UTF8.GetString(buffer, 0, result.Count)}");
                        await RouteJSONMessageAsync(Encoding.UTF8.GetString(buffer, 0, result.Count));
                        return;
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        string id = _manager.GetAllSockets().FirstOrDefault(s => s.Value == webSocket).Key;
                        Console.WriteLine("Recived close message");
                        _manager.GetAllSockets().TryRemove(id,out WebSocket soc);
                        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, cancellationToken: CancellationToken.None);
                        
                        return;
                    }
                });
            }
            else
            {
                await _next(context);
            }
        }

        private async Task ReceiveMessage(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage)
        {
            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                var result = await socket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer), cancellationToken: CancellationToken.None);
                handleMessage(result, buffer);
            }
        }

        public async Task RouteJSONMessageAsync(string message)
        {
            var routeOBJ = JsonConvert.DeserializeObject<dynamic>(message);
            if (Guid.TryParse(routeOBJ.To.ToString(), out Guid guidOutput))
            {
                Console.WriteLine("Targeted");
                var soc = _manager.GetAllSockets().FirstOrDefault(a => a.Key == routeOBJ.To.ToString());
                if (soc.Value != null)
                {
                    if (soc.Value.State == WebSocketState.Open)
                    {
                        await soc.Value.SendAsync(Encoding.UTF8.GetBytes(routeOBJ.Message.ToString()),
                                                  WebSocketMessageType.Text, true, cancellationToken: CancellationToken.None);
                    }
                    
                }
                else
                {
                    Console.WriteLine("Invalid Recipient");
                }
            }
            else
            {
                Console.WriteLine("Broadcast");
                foreach (var soc in _manager.GetAllSockets())
                {
                    if (soc.Value.State == WebSocketState.Open)
                    {
                        await soc.Value.SendAsync(Encoding.UTF8.GetBytes(routeOBJ.Message.ToString()),
                            WebSocketMessageType.Text, true, cancellationToken: CancellationToken.None);
                    }
                }
            }
        }

    }
}
