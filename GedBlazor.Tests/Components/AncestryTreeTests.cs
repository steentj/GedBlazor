using Bunit;
using GedBlazor.Components;
using GedBlazor.Models;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Collections.Generic;
using Radzen;

namespace GedBlazor.Tests.Components;

[TestFixture]
public class AncestryTreeTests
{
    private Bunit.TestContext ctx;

    [SetUp]
    public void Setup()
    {
        ctx = new Bunit.TestContext();
        ctx.Services.AddRadzenComponents();
    }

    [TearDown]
    public void TearDown()
    {
        ctx.Dispose();
    }

    [Test]
    public void Renders_Info_If_No_Proband()
    {
        var cut = ctx.RenderComponent<AncestryTree>(parameters =>
            parameters.Add(p => p.Proband, null)
                      .Add(p => p.Individuals, null));
        Assert.That(cut.Markup, Does.Contain("Vælg venligst en proband for at se slægtstræet."));
    }
}
