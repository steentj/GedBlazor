using NUnit.Framework;
using GedBlazor.Models;

namespace GedBlazor.Tests.Models;

[TestFixture]
public class FamilyTests
{
    private Individual father;
    private Individual mother;
    private Individual child;
    private Family family;

    [SetUp]
    public void Setup()
    {
        father = new Individual("@I1@");
        father.SetName("John", "Smith");
        
        mother = new Individual("@I2@");
        mother.SetName("Jane", "Smith");
        
        child = new Individual("@I3@");
        child.SetName("Junior", "Smith");

        family = new Family("@F1@");
    }

    [Test]
    public void Constructor_WithValidId_SetsIdCorrectly()
    {
        Assert.That(family.Id, Is.EqualTo("@F1@"));
    }

    [Test]
    public void SetHusband_ValidIndividual_SetsHusbandCorrectly()
    {
        // Act
        family.SetHusband(father);

        // Assert
        Assert.That(family.Husband, Is.EqualTo(father));
    }

    [Test]
    public void SetWife_ValidIndividual_SetsWifeCorrectly()
    {
        // Act
        family.SetWife(mother);

        // Assert
        Assert.That(family.Wife, Is.EqualTo(mother));
    }

    [Test]
    public void AddChild_ValidChild_AddsToChildren()
    {
        // Act
        family.AddChild(child);

        // Assert
        Assert.That(family.Children, Does.Contain(child));
    }

    [Test]
    public void ToString_CompleteFamily_ReturnsFormattedString()
    {
        // Arrange
        family.SetHusband(father);
        family.SetWife(mother);
        family.AddChild(child);

        // Act
        var result = family.ToString();

        // Assert
        Assert.That(result, Is.EqualTo("Family @F1@: John Smith + Jane Smith, Children: Junior Smith"));
    }

    [Test]
    public void ToString_NoChildren_ReturnsFormattedStringWithoutChildren()
    {
        // Arrange
        family.SetHusband(father);
        family.SetWife(mother);

        // Act
        var result = family.ToString();

        // Assert
        Assert.That(result, Is.EqualTo("Family @F1@: John Smith + Jane Smith"));
    }
}