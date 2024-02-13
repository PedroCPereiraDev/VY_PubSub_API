namespace API.EventHandling; 

using API.EventHandling.Interface;
using API.Model;

internal class EventInvoiceBroker : IEventInvoiceBroker
{
    private event EventHandler<EventInvoiceData> Broker = delegate { };

    public void Subscribe(EventHandler<EventInvoiceData> subscriber)
    {
        Broker += subscriber;
    }

    public void Unsubscribe(EventHandler<EventInvoiceData> subscriber)
    {
        Broker -= subscriber;
    }

    // Here I added a fail-safe for the hippotetical case of one or
    // more of the subscribers throwing an exception. This allows all
    // the subscribers to receive the event before dealing with the
    // exception. In this case I throw it aggregated, but it could be a log.
    public void Publish(EventInvoiceData data)
    {
        var exceptions = new List<Exception>();

        foreach (var handler in Broker.GetInvocationList())
        {
            try
            {
                handler.DynamicInvoke(this, data);
            }
            catch (Exception e)
            {
                exceptions.Add(e);
            }
        }

        if (exceptions.Count != 0)
        {
            throw new AggregateException(exceptions);
        }
    }
}
