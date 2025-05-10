using Bunit;
using GedBlazor.Components;
using GedBlazor.Models;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using System.Collections.Generic;

namespace GedBlazor.Tests.Components;

[TestFixture]
public class AncestryTreeTests
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
    public void Renders_Info_If_No_Proband()
    {
        var cut = ctx.RenderComponent<AncestryTree>(parameters =>
            parameters.Add(p => p.Proband, null)
                      .Add(p => p.Individuals, null));
        Assert.That(cut.Markup, Does.Contain("Please select a proband"));
    }

    [Test]
    public void Renders_Proband_And_Ancestors()
    {
        var ind1 = new Individual("@I1@") { Anenummer = 1 };
        ind1.SetName("Proband", "Test");
        var ind2 = new Individual("@I2@") { Anenummer = 2 };
        ind2.SetName("Father", "Test");
        var ind3 = new Individual("@I3@") { Anenummer = 3 };
        ind3.SetName("Mother", "Test");
        ind1.FatherId = ind2.Id;
        ind1.MotherId = ind3.Id;
        var individuals = new Dictionary<string, Individual> { [ind1.Id] = ind1, [ind2.Id] = ind2, [ind3.Id] = ind3 };
        var cut = ctx.RenderComponent<AncestryTree>(parameters =>
            parameters.Add(p => p.Proband, ind1)
                      .Add(p => p.Individuals, individuals));
        Assert.That(cut.Markup, Does.Contain("1: Proband Test"));
        Assert.That(cut.Markup, Does.Contain("2: Father Test"));
        Assert.That(cut.Markup, Does.Contain("3: Mother Test"));
    }

    [Test]
    public void Collapsible_Nodes_Work()
    {
        var ind1 = new Individual("@I1@") { Anenummer = 1 };
        ind1.SetName("Proband", "Test");
        var ind2 = new Individual("@I2@") { Anenummer = 2 };
        ind2.SetName("Father", "Test");
        var ind3 = new Individual("@I3@") { Anenummer = 3 };
        ind3.SetName("Mother", "Test");
        ind1.FatherId = ind2.Id;
        ind1.MotherId = ind3.Id;
        var individuals = new Dictionary<string, Individual> { [ind1.Id] = ind1, [ind2.Id] = ind2, [ind3.Id] = ind3 };
        var cut = ctx.RenderComponent<AncestryTree>(parameters =>
            parameters.Add(p => p.Proband, ind1)
                      .Add(p => p.Individuals, individuals));
        // Children are shown by default
        Assert.That(cut.Markup, Does.Contain("2: Father Test"));
        Assert.That(cut.Markup, Does.Contain("3: Mother Test"));
        // Collapse proband node
        var toggle = cut.Find("button.toggle-btn");
        toggle.Click();
        Assert.That(cut.Markup, Does.Not.Contain("2: Father Test"));
        Assert.That(cut.Markup, Does.Not.Contain("3: Mother Test"));
        // Expand again
        toggle.Click();
        Assert.That(cut.Markup, Does.Contain("2: Father Test"));
        Assert.That(cut.Markup, Does.Contain("3: Mother Test"));
    }

    [Test]
    public void Only_Ancestors_With_Anenummer_Are_Displayed()
    {
        var ind1 = new Individual("@I1@") { Anenummer = 1 };
        ind1.SetName("Proband", "Test");
        var ind2 = new Individual("@I2@") { Anenummer = -1 };
        ind2.SetName("Father", "Test");
        ind1.FatherId = ind2.Id;
        var individuals = new Dictionary<string, Individual> { [ind1.Id] = ind1, [ind2.Id] = ind2 };
        var cut = ctx.RenderComponent<AncestryTree>(parameters =>
            parameters.Add(p => p.Proband, ind1)
                      .Add(p => p.Individuals, individuals));
        // If expand button exists, click it, else just assert ancestor is not present
        var toggles = cut.FindAll("button.toggle-btn");
        if (toggles.Count > 0)
        {
            toggles[0].Click();
        }
        Assert.That(cut.Markup, Does.Not.Contain("Father Test"));
    }
}
