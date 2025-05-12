// filepath: /Users/steen/GedBlazor/GedBlazor.Tests/Components/AnestavleCellTests.cs
using Bunit;
using GedBlazor.Components;
using GedBlazor.Models;
using NUnit.Framework;

namespace GedBlazor.Tests.Components;

[TestFixture]
public class AnetavleCellTests
{
    private Bunit.TestContext ctx;

    [SetUp]
    public void Setup()
    {
        ctx = new Bunit.TestContext();
    }

    [TearDown]
    public void TearDown()
    {
        ctx.Dispose();
    }

    [Test]
    public void WhenNoIndividualProvided_ShowsEmpty()
    {
        // Act
        var cut = ctx.RenderComponent<AnetavleCell>(parameters => parameters
            .Add(p => p.AnenNumber, 1));

        // Assert
        Assert.That(cut.Find(".empty").TextContent, Does.Contain("No ancestor (1)"));
    }

    [Test]
    public void WhenIndividualProvided_DetailMode_ShowsFullInfo()
    {
        // Arrange
        var individual = new Individual("@I1@");
        individual.SetName("John", "Doe");
        var birthDate = new GedcomDate(1900, 1, 1);
        individual.SetBirth(birthDate);
        individual.BirthPlace = "New York";
        var deathDate = new GedcomDate(1980, 12, 31);
        individual.SetDeath(deathDate);
        individual.DeathPlace = "Los Angeles";

        // Act
        var cut = ctx.RenderComponent<AnetavleCell>(parameters => parameters
            .Add(p => p.Individual, individual)
            .Add(p => p.AnenNumber, 1)
            .Add(p => p.ShowMinimal, false));

        // Assert
        Assert.That(cut.Find(".name").TextContent, Is.EqualTo("John Doe"));
        Assert.That(cut.Find(".anenummer").TextContent, Is.EqualTo("1"));
        Assert.That(cut.FindAll(".date").Count, Is.EqualTo(2));
        Assert.That(cut.FindAll(".place").Count, Is.EqualTo(2));
    }

    [Test]
    public void WhenIndividualProvided_MinimalMode_ShowsCompactInfo()
    {
        // Arrange
        var individual = new Individual("@I1@");
        individual.SetName("John", "Doe");
        var birthDate = new GedcomDate(1900, 1, 1);
        individual.SetBirth(birthDate);
        var deathDate = new GedcomDate(1980, 12, 31);
        individual.SetDeath(deathDate);

        // Act
        var cut = ctx.RenderComponent<AnetavleCell>(parameters => parameters
            .Add(p => p.Individual, individual)
            .Add(p => p.AnenNumber, 4)
            .Add(p => p.ShowMinimal, true));

        // Assert
        Assert.That(cut.Find(".minimal").TextContent, Does.Contain("John Doe"));
        Assert.That(cut.Find(".minimal").TextContent, Does.Contain("4"));
        Assert.That(cut.Find(".minimal").TextContent, Does.Contain("1900 Jan 1"));
        Assert.That(cut.Find(".minimal").TextContent, Does.Contain("1980 Dec 31"));
    }

    [Test]
    public void AnetavleCell_HandlesPartialDates()
    {
        // Arrange - Individual with only birth date
        var individual = new Individual("@I1@");
        individual.SetName("John", "Doe");
        individual.SetBirth(new GedcomDate(1900, 1, 1));

        // Act
        var cut = ctx.RenderComponent<AnetavleCell>(parameters => parameters
            .Add(p => p.Individual, individual)
            .Add(p => p.AnenNumber, 1)
            .Add(p => p.ShowMinimal, false));

        // Assert
        Assert.That(cut.FindAll(".date").Count, Is.EqualTo(1));
    }
}
