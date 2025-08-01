using NUnit.Framework;
using GedBlazor.Models;

namespace GedBlazor.Tests.Models;

[TestFixture]
public class GedcomDateTests
{
    [Test]
    public void ParseExactDate_ValidFormat_ReturnsCorrectDate()
    {
        // Arrange
        var dateString = "12 MAY 1984";

        // Act
        var gedcomDate = GedcomDate.Parse(dateString);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(gedcomDate.Day, Is.EqualTo(12));
            Assert.That(gedcomDate.Month, Is.EqualTo(5));
            Assert.That(gedcomDate.Year, Is.EqualTo(1984));
            Assert.That(gedcomDate.IsApproximate, Is.False);
        });
    }

    [Test]
    public void ParseApproximateDate_WithAbt_ReturnsApproximateDate()
    {
        // Arrange
        var dateString = "ABT 1984";

        // Act
        var gedcomDate = GedcomDate.Parse(dateString);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(gedcomDate.Year, Is.EqualTo(1984));
            Assert.That(gedcomDate.IsApproximate, Is.True);
            Assert.That(gedcomDate.Day, Is.Null);
            Assert.That(gedcomDate.Month, Is.Null);
        });
    }

    [Test]
    public void ToString_CompleteDate_ReturnsFormattedString()
    {
        // Arrange
        var gedcomDate = new GedcomDate(12, 5, 1984);

        // Act
        var result = gedcomDate.ToString();

        // Assert
        Assert.That(result, Is.EqualTo("12-05-1984"));
    }

    [Test]
    public void ToString_ApproximateDate_IncludesAbt()
    {
        // Arrange
        var gedcomDate = new GedcomDate(null, null, 1984, true);

        // Act
        var result = gedcomDate.ToString();

        // Assert
        Assert.That(result, Is.EqualTo("Ca 1984"));
    }
    
    [Test]
    public void ToString_Formats_Date_And_Month_With_2_Digits()
    {
        // Arrange
        var gedcomDate = new GedcomDate(1, 1, 2023);

        // Act
        var result = gedcomDate.ToString();

        // Assert
        Assert.That(result, Is.EqualTo("01-01-2023"));
    }
}