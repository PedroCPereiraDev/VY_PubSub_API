namespace API.EventHandling.Interface;

using API.Model;

// To follow ISP (Interface Segregation Principle) I have this interface to be used by publishing classes
// that do not need to have knowledge of the implementations of subscription 
public interface IEventInvoiceBrokerPublisher
{
    void Publish(EventInvoiceData data);
}
