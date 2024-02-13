namespace API.Tests.InvoiceHelpers;

using FluentAssertions;
using API.Helpers;
using NUnit.Framework;

internal class InvoiceProcessorTests
{
    [Test]
    public void NormalizeInvoiceNumber_ReceiveString_ReturnNormalizedString()
    {
        //Arrange
        var data = "    fr/01 INV2222    ";

        //Act
        var result = InvoiceHelper.NormalizeInvoiceNumber(data);

        //Assert
        result.Should().NotBeNullOrWhiteSpace();
        result.Should().BeEquivalentTo("FR/01 INV2222");
    }

    [Test]
    public void CalculateTax_ReceiveTotalValue_ReturnTaxAmmount()
    {
        //Arrange
        var data = 123.00M;

        //Act
        var result = InvoiceHelper.CalculateTax(data);

        //Assert
        result.Should().Be(23);
    }
}
