namespace API.Controllers;

using System;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using API.EventHandling;
using API.EventHandling.Interface;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/salary/subscriber")]
public class SalarySubscriberController : ControllerBase
{
    private readonly ILogger<SalarySubscriberController> logger;
    private readonly IEventInvoiceBrokerSubscriber eventBroker;

    public SalarySubscriberController(
        ILogger<SalarySubscriberController> logger,
        IEventInvoiceBrokerSubscriber eventBroker)
    {
        this.logger = logger;
        this.eventBroker = eventBroker;
    }
        
    public async Task Get()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;            
        }

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

        async void subFuncAsync(object? sender, EventInvoiceData data) => await InvoiceEventListenerAsync(webSocket, data);

        eventBroker.Subscribe(subFuncAsync);

        await SocketLoop(webSocket, subFuncAsync);

        eventBroker.Unsubscribe(subFuncAsync);
    }

    private async Task SocketLoop(WebSocket webSocket, EventHandler<EventInvoiceData> handler)
    {
        var buffer = new byte[1024 * 4];
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            var response = Encoding.UTF8.GetString(buffer, 0, receiveResult.Count);

            if (response == "unsubscribe")
            {
                eventBroker.Unsubscribe(handler);
            }
            else if (response == "subscribe")
            {
                eventBroker.Subscribe(handler);
            }

            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);           
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }

    private static async Task InvoiceEventListenerAsync(WebSocket webSocket, EventInvoiceData data)
    {
        if(webSocket.State == WebSocketState.Open)
        {
            var byteArray = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(data));

            await webSocket.SendAsync(byteArray, WebSocketMessageType.Text, true, CancellationToken.None);
        }        
    }
}
