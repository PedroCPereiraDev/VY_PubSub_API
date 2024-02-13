namespace API.Extensions;

internal static class DecimalExtensions
{
    // Returns the value as custom euro currency representation
    public static string ParseAsCurrency(this decimal value)
    {
        return $"{value:0.00} EUR";
    }
}
