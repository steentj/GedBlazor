using Bunit;
using GedBlazor.Components;
using GedBlazor.Models;
using GedBlazor.Parsers;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using System.Text;

namespace GedBlazor.Tests.Components;

[TestFixture]
public class GedcomUploaderTests
{
    private Bunit.TestContext ctx;
    private Mock<IGedcomParser> mockParser;

    [SetUp]
    public void Setup()
    {
        ctx = new Bunit.TestContext();
        mockParser = new Mock<IGedcomParser>();
        ctx.Services.AddSingleton<IGedcomParser>(mockParser.Object);
    }

    [TearDown]
    public void TearDown()
    {
        ctx.Dispose();
    }

    [Test]
    public void InitialRender_ShowsUploadPrompt()
    {
        // Act
        var cut = ctx.RenderComponent<GedcomUploader>();

        // Assert
        Assert.That(cut.Find("p").TextContent, Does.Contain("Træk GEDCOM-fil hertil"));
    }

    [Test]
    public async Task WhenFileUploaded_ParsesAndDisplaysIndividuals()
    {
        // Arrange
        var individuals = new Dictionary<string, Individual>
        {
            ["@I1@"] = new Individual("@I1@"),
            ["@I2@"] = new Individual("@I2@")
        };
        individuals["@I1@"].SetName("John", "Smith");
        individuals["@I2@"].SetName("Jane", "Doe");

        var families = new Dictionary<string, Family>();
        mockParser.Setup(p => p.Parse(It.IsAny<string>()))
                 .Returns((individuals, families));

        var cut = ctx.RenderComponent<GedcomUploader>();

        // Act - simulate file upload
        var content = "0 HEAD\n1 CHAR UTF-8\n0 TRLR";
        var file = new MockBrowserFile("test.ged", content);
        var inputFile = cut.FindComponent<InputFile>();
        await inputFile.InvokeAsync(() => inputFile.Instance.OnChange.InvokeAsync(
            new InputFileChangeEventArgs(new[] { file })));

        // Assert
        var rows = cut.FindAll("tbody tr");
        Assert.Multiple(() =>
        {
            Assert.That(rows.Count, Is.EqualTo(2));
            // Verify content exists regardless of order
            Assert.That(rows.Any(r => r.TextContent.Contains("John Smith")), Is.True, "Should contain John Smith");
            Assert.That(rows.Any(r => r.TextContent.Contains("Jane Doe")), Is.True, "Should contain Jane Doe");
            // Verify alphabetical order
            Assert.That(rows[0].TextContent.Contains("Jane Doe"), Is.True, "Jane Doe should be first alphabetically");
            Assert.That(rows[1].TextContent.Contains("John Smith"), Is.True, "John Smith should be second alphabetically");
        });
    }

    [Test]
    public async Task WhenInvalidFileUploaded_ShowsError()
    {
        // Arrange
        mockParser.Setup(p => p.Parse(It.IsAny<string>()))
                 .Throws<FormatException>();

        var cut = ctx.RenderComponent<GedcomUploader>();

        // Act
        var file = new MockBrowserFile("invalid.ged", "invalid content");
        var inputFile = cut.FindComponent<InputFile>();
        await inputFile.InvokeAsync(() => inputFile.Instance.OnChange.InvokeAsync(
            new InputFileChangeEventArgs(new[] { file })));

        // Assert
        var error = cut.Find(".alert-danger");
        Assert.That(error.TextContent, Does.Contain("Ugyldigt GEDCOM-filformat"));
    }

    [Test]
    public async Task StartAnenummerInput_ReassignsWithCustomStart()
    {
        // Arrange
        var individuals = new Dictionary<string, Individual>
        {
            ["@I1@"] = new Individual("@I1@")
        };
        individuals["@I1@"].SetName("John", "Smith");
        var families = new Dictionary<string, Family>();
        mockParser.Setup(p => p.Parse(It.IsAny<string>()))
                  .Returns((individuals, families));

        var cut = ctx.RenderComponent<GedcomUploader>();

        // Upload a file to populate individuals
        var content = "0 HEAD\n1 CHAR UTF-8\n0 TRLR";
        var file = new MockBrowserFile("test.ged", content);
        var inputFile = cut.FindComponent<InputFile>();
        await inputFile.InvokeAsync(() => inputFile.Instance.OnChange.InvokeAsync(
            new InputFileChangeEventArgs(new[] { file })));

        // Select proband
        var select = cut.Find("#probandSelect");
        select.Change("@I1@");

        // Enter custom start anenummer
        var input = cut.Find("#startAnenummer");
        input.Input("7");

        // Verify AssignAnenummer called with custom start
        mockParser.Verify(p => p.AssignAnenummer(It.IsAny<Dictionary<string, Individual>>(), "@I1@", 7), Times.AtLeastOnce());
    }

    private class MockBrowserFile : IBrowserFile
    {
        private readonly string content;

        public MockBrowserFile(string name, string content)
        {
            Name = name;
            this.content = content;
            LastModified = DateTimeOffset.Now;
            Size = Encoding.UTF8.GetByteCount(content);
        }

        public string Name { get; }
        public DateTimeOffset LastModified { get; }
        public long Size { get; }
        public string ContentType => "text/plain";

        public Stream OpenReadStream(long maxAllowedSize = 512000, CancellationToken cancellationToken = default)
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(content));
        }
    }
}