namespace API.EventHandling; 

using System.Diagnostics.CodeAnalysis;

[ExcludeFromCodeCoverage]
public class EventInvoiceData : EventArgs
{
    public decimal InvoiceValue { get; set; }

    public decimal TaxesValue { get; set; }

    public string InvoiceNumber { get; set; }

    public EventInvoiceData(decimal invoiceValue, decimal taxesValue, string invoiceNumber)
    {
        InvoiceValue = invoiceValue;
        TaxesValue = taxesValue;
        InvoiceNumber = invoiceNumber;
    }
}
