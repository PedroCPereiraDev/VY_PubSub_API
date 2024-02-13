namespace API.Controllers;

using System.Diagnostics.CodeAnalysis;
using System.Net.WebSockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using API.EventHandling;
using API.EventHandling.Interface;
using API.Helpers;
using API.Model;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("/salary/publisher")]
public class SalaryPublisherController : ControllerBase
{
    private readonly ILogger<SalaryPublisherController> logger;
    private readonly IEventInvoiceBrokerPublisher eventBroker;
    private readonly JsonSerializerOptions SerialOptions = new()
    {
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public SalaryPublisherController(
        ILogger<SalaryPublisherController> logger,
        IEventInvoiceBrokerPublisher eventBroker)
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

        await SocketLoop(webSocket);
    }

    private async Task SocketLoop(WebSocket webSocket)
    {
        var buffer = new byte[1024 * 4];

        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            InvoiceDataInput? data;

            try
            {
                data = JsonSerializer.Deserialize<InvoiceDataInput>(System.Text.Encoding.UTF8.GetString(buffer, 0, receiveResult.Count), SerialOptions);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Parsing of published Invoice data failed");
                data = default;
            }

            if (InputDataIsValid(data))
            {
                var output = new EventInvoiceData(
                    data.Value,
                    InvoiceHelper.CalculateTax(data.Value),
                    InvoiceHelper.NormalizeInvoiceNumber(data.Number));

                eventBroker.Publish(output);
            }

            receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(
            receiveResult.CloseStatus.Value,
            receiveResult.CloseStatusDescription,
            CancellationToken.None);
    }

    private static bool InputDataIsValid([NotNullWhen(true)] InvoiceDataInput? data)
    {
        return data != default && data.Value > 0 && !string.IsNullOrWhiteSpace(data.Number);
    }
}
