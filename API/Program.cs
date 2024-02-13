using API.EventHandling.Interface;
using API.EventHandling;

namespace API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services
            .AddSingleton<IEventInvoiceBroker, EventInvoiceBroker>()
            .AddSingleton<IEventInvoiceBrokerPublisher>(x => x.GetRequiredService<IEventInvoiceBroker>())
            .AddSingleton<IEventInvoiceBrokerSubscriber>(x => x.GetRequiredService<IEventInvoiceBroker>());


        var app = builder.Build();

        // Configure the HTTP request pipeline.

        app.UseHttpsRedirection();

        var webSocketOptions = new WebSocketOptions
        {
            KeepAliveInterval = TimeSpan.FromMinutes(2)
        };

        app.UseWebSockets(webSocketOptions);        

        app.UseAuthorization();

        app.MapControllers();

        app.UseWebSockets();

        app.UseStaticFiles();

        app.Run();
    }
}
