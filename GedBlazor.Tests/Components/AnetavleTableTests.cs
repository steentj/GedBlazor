using Bunit;
using GedBlazor.Components;
using GedBlazor.Models;
using GedBlazor.Services;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace GedBlazor.Tests.Components;

[TestFixture]
public class AnetavleTableTests
{
    private Bunit.TestContext ctx;
    private Mock<IFileDownloadService> mockFileDownloadService;
    private Mock<IWordDocumentService> mockWordDocumentService;

    [SetUp]
    public void Setup()
    {
        ctx = new Bunit.TestContext();
        
        // Set up mocks
        mockFileDownloadService = new Mock<IFileDownloadService>();
        mockWordDocumentService = new Mock<IWordDocumentService>();
        
        // Register services
        ctx.Services.AddSingleton<IFileDownloadService>(mockFileDownloadService.Object);
        ctx.Services.AddSingleton<IWordDocumentService>(mockWordDocumentService.Object);
    }

    [TearDown]
    public void TearDown()
    {
        ctx.Dispose();
    }

    [Test]
    public void WhenNoProbandProvided_ShowsInfoMessage()
    {
        // Act
        var cut = ctx.RenderComponent<AnetavleTable>();

        // Assert
        Assert.That(cut.Find(".alert-info").TextContent, Does.Contain("Vælg venligst en proband for at se anetavlen."));
    }

    [Test]
    public void WhenProbandProvided_ShowsAnetavleTable()
    {
        // Arrange
        var proband = new Individual("@I1@");
        proband.SetName("John", "Doe");
        proband.Anenummer = 1;

        var individuals = new Dictionary<string, Individual>
        {
            ["@I1@"] = proband
        };

        // Act
        var cut = ctx.RenderComponent<AnetavleTable>(parameters => parameters
            .Add(p => p.Proband, proband)
            .Add(p => p.Individuals, individuals));

        // Assert
        Assert.That(cut.Find(".anetavle"), Is.Not.Null);
        Assert.That(cut.FindAll(".generation").Count, Is.EqualTo(5));
    }

    [Test]
    public void AnetavleTable_DisplaysCorrectGenerations()
    {
        // Arrange - Create a family with multiple generations
        var individuals = CreateThreeGenerationFamily();
        var proband = individuals["@I1@"];

        // Act
        var cut = ctx.RenderComponent<AnetavleTable>(parameters => parameters
            .Add(p => p.Proband, proband)
            .Add(p => p.Individuals, individuals));

        // Assert - Check that all generations are present
        Assert.That(cut.Find(".proband").TextContent, Does.Contain("John Doe"));
        Assert.That(cut.Find(".parents").TextContent, Does.Contain("Father Doe"));
        Assert.That(cut.Find(".parents").TextContent, Does.Contain("Mother Maiden"));
        Assert.That(cut.Find(".grandparents").TextContent, Does.Contain("Grandfather Doe"));
    }

    [Test]
    public void AnetavleTable_HandlesEmptyAncestors()
    {
        // Arrange - Create a family with missing ancestors
        var individuals = new Dictionary<string, Individual>
        {
            ["@I1@"] = new Individual("@I1@") { Anenummer = 1 }
        };
        individuals["@I1@"].SetName("John", "Doe");

        // Act
        var cut = ctx.RenderComponent<AnetavleTable>(parameters => parameters
            .Add(p => p.Proband, individuals["@I1@"])
            .Add(p => p.Individuals, individuals));

        // Assert - Check that empty placeholders are shown
        Assert.That(cut.FindAll(".empty").Count, Is.GreaterThan(0));
    }

    // Helper method to create test data
    private Dictionary<string, Individual> CreateThreeGenerationFamily()
    {
        var result = new Dictionary<string, Individual>();
        
        // Proband (Generation 1)
        var proband = new Individual("@I1@");
        proband.SetName("John", "Doe");
        proband.Anenummer = 1;
        proband.FatherId = "@I2@";
        proband.MotherId = "@I3@";
        result.Add(proband.Id, proband);
        
        // Parents (Generation 2)
        var father = new Individual("@I2@");
        father.SetName("Father", "Doe");
        father.Anenummer = 2;
        father.FatherId = "@I4@";
        father.MotherId = "@I5@";
        result.Add(father.Id, father);
        
        var mother = new Individual("@I3@");
        mother.SetName("Mother", "Maiden");
        mother.Anenummer = 3;
        mother.FatherId = "@I6@";
        mother.MotherId = "@I7@";
        result.Add(mother.Id, mother);
        
        // Grandparents (Generation 3)
        var grandfather1 = new Individual("@I4@");
        grandfather1.SetName("Grandfather", "Doe");
        grandfather1.Anenummer = 4;
        result.Add(grandfather1.Id, grandfather1);
        
        var grandmother1 = new Individual("@I5@");
        grandmother1.SetName("Grandmother", "Doe");
        grandmother1.Anenummer = 5;
        result.Add(grandmother1.Id, grandmother1);
        
        var grandfather2 = new Individual("@I6@");
        grandfather2.SetName("Grandfather", "Maiden");
        grandfather2.Anenummer = 6;
        result.Add(grandfather2.Id, grandfather2);
        
        var grandmother2 = new Individual("@I7@");
        grandmother2.SetName("Grandmother", "Maiden");
        grandmother2.Anenummer = 7;
        result.Add(grandmother2.Id, grandmother2);

        return result;
    }
}
