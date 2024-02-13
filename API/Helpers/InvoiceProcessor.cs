namespace API.Helpers;

internal static class InvoiceHelper
{
    // In this demo I assume the normal generic Portuguese VAT tax of 23%
    private const decimal TaxPercentageAmount = 23;

    // Here I normalize the Invoice Numbers to the "standard" requested by the company
    public static string NormalizeInvoiceNumber(string originalInvoiceNumber) => originalInvoiceNumber.ToUpperInvariant().Trim();

    public static decimal CalculateTax(decimal totalValue) => Math.Round(totalValue / GetTaxAsDivisionable() * GetTaxAsDecimalPoint(), 2, MidpointRounding.AwayFromZero);

    private static decimal GetTaxAsDivisionable() => GetTaxAsDecimalPoint() + 1;

    private static decimal GetTaxAsDecimalPoint() => (TaxPercentageAmount / 100);
}
