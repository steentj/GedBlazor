using NUnit.Framework;
using GedBlazor.Models;

namespace GedBlazor.Tests.Models;

[TestFixture]
public class IndividualTests
{
    [Test]
    public void Constructor_WithValidId_SetsIdCorrectly()
    {
        // Arrange & Act
        var individual = new Individual("@I123@");

        // Assert
        Assert.That(individual.Id, Is.EqualTo("@I123@"));
    }

    [Test]
    public void SetName_WithValidName_SetsNameCorrectly()
    {
        // Arrange
        var individual = new Individual("@I123@");

        // Act
        individual.SetName("John", "Smith");

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(individual.GivenName, Is.EqualTo("John"));
            Assert.That(individual.Surname, Is.EqualTo("Smith"));
            Assert.That(individual.FullName, Is.EqualTo("John Smith"));
        });
    }

    [Test]
    public void SetBirthDate_WithValidDate_SetsBirthDateCorrectly()
    {
        // Arrange
        var individual = new Individual("@I123@");
        var birthDate = new GedcomDate(1, 1, 1980);

        // Act
        individual.SetBirth(birthDate);

        // Assert
        Assert.That(individual.BirthDate, Is.EqualTo(birthDate));
    }

    [Test]
    public void SetDeathDate_WithValidDate_SetsDeathDateCorrectly()
    {
        // Arrange
        var individual = new Individual("@I123@");
        var deathDate = new GedcomDate(31, 12, 2020);

        // Act
        individual.SetDeath(deathDate);

        // Assert
        Assert.That(individual.DeathDate, Is.EqualTo(deathDate));
    }

    [Test]
    public void ToString_WithCompleteInformation_ReturnsFormattedString()
    {
        // Arrange
        var individual = new Individual("@I123@");
        individual.SetName("John", "Smith");
        individual.SetBirth(new GedcomDate(1, 1, 1980));
        individual.SetDeath(new GedcomDate(31, 12, 2020));

        // Act
        var result = individual.ToString();

        // Assert
        Assert.That(result, Is.EqualTo("John Smith (01-01-1980 - 31-12-2020)"));
    }
}