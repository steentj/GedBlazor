using NUnit.Framework;
using GedBlazor.Models;
using GedBlazor.Parsers;

namespace GedBlazor.Tests.Parsers;

[TestFixture]
public class GedcomParserTests
{
    private IGedcomParser parser;
    private const string ValidGedcomContent = @"0 HEAD
1 GEDC
2 VERS 5.5.5
1 CHAR UTF-8
0 @I1@ INDI
1 NAME John /Smith/
1 BIRT
2 DATE 1 JAN 1950
0 @I2@ INDI
1 NAME Jane /Doe/
1 BIRT
2 DATE 1 JAN 1952
0 @F1@ FAM
1 HUSB @I1@
1 WIFE @I2@
0 TRLR";

    [SetUp]
    public void Setup()
    {
        parser = new GedcomParser();
    }

    [Test]
    public void Parse_ValidGedcom_ReturnsCorrectIndividuals()
    {
        // Act
        var (individuals, _) = parser.Parse(ValidGedcomContent);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(individuals.Count, Is.EqualTo(2));
            Assert.That(individuals["@I1@"].FullName, Is.EqualTo("John Smith"));
            Assert.That(individuals["@I2@"].FullName, Is.EqualTo("Jane Doe"));
        });
    }

    [Test]
    public void Parse_ValidGedcom_ReturnsCorrectFamilies()
    {
        // Act
        var (individuals, families) = parser.Parse(ValidGedcomContent);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(families.Count, Is.EqualTo(1));
            var family = families["@F1@"];
            Assert.That(family.Husband, Is.EqualTo(individuals["@I1@"]));
            Assert.That(family.Wife, Is.EqualTo(individuals["@I2@"]));
        });
    }

    [Test]
    public void Parse_InvalidGedcom_ThrowsFormatException()
    {
        // Arrange
        var invalidContent = "This is not a GEDCOM file";

        // Act & Assert
        Assert.Throws<FormatException>(() => parser.Parse(invalidContent));
    }

    [Test]
    public void Parse_NullContent_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => parser.Parse(null!));
    }

    [Test]
    public void Parse_EmptyContent_ThrowsFormatException()
    {
        // Arrange
        var emptyContent = "";

        // Act & Assert
        Assert.Throws<FormatException>(() => parser.Parse(emptyContent));
    }

    [Test]
    public void Parse_MissingHeader_ThrowsFormatException()
    {
        // Arrange
        var contentWithoutHeader = @"0 @I1@ INDI
1 NAME John /Smith/
0 TRLR";

        // Act & Assert
        Assert.Throws<FormatException>(() => parser.Parse(contentWithoutHeader));
    }

    [Test]
    public void Parse_MissingTrailer_ThrowsFormatException()
    {
        // Arrange
        var contentWithoutTrailer = @"0 HEAD
1 GEDC
2 VERS 5.5.5
0 @I1@ INDI
1 NAME John /Smith/";

        // Act & Assert
        Assert.Throws<FormatException>(() => parser.Parse(contentWithoutTrailer));
    }

    [Test]
    public void Parse_InvalidIndividualRecord_SkipsRecord()
    {
        // Arrange
        var contentWithInvalidRecord = @"0 HEAD
1 GEDC
2 VERS 5.5.5
0 @I1@ INDI
1 NAME Invalid Name Format
0 @I2@ INDI
1 NAME John /Smith/
0 TRLR";

        // Act
        var (individuals, _) = parser.Parse(contentWithInvalidRecord);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(individuals.Count, Is.EqualTo(2));
            Assert.That(individuals["@I1@"].FullName, Is.Empty);
            Assert.That(individuals["@I2@"].FullName, Is.EqualTo("John Smith"));
        });
    }

    [Test]
    public void Parse_MalformedDate_SkipsDateParsing()
    {
        // Arrange
        var contentWithInvalidDate = @"0 HEAD
1 GEDC
2 VERS 5.5.5
0 @I1@ INDI
1 NAME John /Smith/
1 BIRT
2 DATE Invalid Date
0 TRLR";

        // Act
        var (individuals, _) = parser.Parse(contentWithInvalidDate);

        // Assert
        Assert.That(individuals["@I1@"].BirthDate, Is.Null);
    }

    [Test]
    public void ParseLevelZeroRecord_ShouldParseIndividualsWithNonStandardIds()
    {
        // Arrange
        string gedcomContent = @"
0 HEAD
1 GEDC
2 VERS 5.5.1
0 @ABCD@ INDI
1 NAME John /Doe/
0 TRLR
";

        // Act
        var (individuals, _) = parser.Parse(gedcomContent);

        // Assert
        Assert.That(individuals.ContainsKey("@ABCD@"), Is.True);
        Assert.That(individuals["@ABCD@"].GivenName, Is.EqualTo("John"));
        Assert.That(individuals["@ABCD@"].Surname, Is.EqualTo("Doe"));
    }

    [Test]
    public void Parse_IndividualWithBirthAndDeathData_ParsesDatesAndPlaces()
    {
        const string gedcom = @"0 HEAD
1 GEDC
2 VERS 5.5.5
1 CHAR UTF-8
0 @I1@ INDI
1 NAME John /Smith/
1 BIRT
2 DATE 1 JAN 1950
2 PLAC Copenhagen, Denmark
1 DEAT
2 DATE 2 FEB 2000
2 PLAC Aarhus, Denmark
0 TRLR";

        var (individuals, _) = parser.Parse(gedcom);
        var ind = individuals["@I1@"];

        Assert.Multiple(() =>
        {
            Assert.That(ind.BirthDate, Is.Not.Null);
            Assert.That(ind.BirthDate.ToString(), Is.EqualTo("1 JAN 1950").IgnoreCase);
            Assert.That(ind.BirthPlace, Is.EqualTo("Copenhagen, Denmark"));
            Assert.That(ind.DeathDate, Is.Not.Null);
            Assert.That(ind.DeathDate.ToString(), Is.EqualTo("2 FEB 2000").IgnoreCase);
            Assert.That(ind.DeathPlace, Is.EqualTo("Aarhus, Denmark"));
        });
    }

    [Test]
    public void Parse_IndividualWithBirthAndDeath_AgncAsPlace_ParsesAgncIfPlacMissing()
    {
        const string gedcom = @"0 HEAD
1 GEDC
2 VERS 5.5.5
0 @I1@ INDI
1 NAME Test /Person/
1 BIRT
2 DATE 1 JAN 1900
2 AGNC TestBirthPlace
1 DEAT
2 DATE 2 FEB 2000
2 AGNC TestDeathPlace
0 TRLR";
        var (individuals, _) = parser.Parse(gedcom);
        var ind = individuals["@I1@"];
        Assert.Multiple(() =>
        {
            Assert.That(ind.BirthDate, Is.Not.Null);
            Assert.That(ind.BirthDate.ToString(), Is.EqualTo("1 JAN 1900").IgnoreCase);
            Assert.That(ind.BirthPlace, Is.EqualTo("TestBirthPlace"));
            Assert.That(ind.DeathDate, Is.Not.Null);
            Assert.That(ind.DeathDate.ToString(), Is.EqualTo("2 FEB 2000").IgnoreCase);
            Assert.That(ind.DeathPlace, Is.EqualTo("TestDeathPlace"));
        });
    }

    [Test]
    public void Parse_IndividualWithBirthAndDeath_PlacePrefersPlacOverAgnc()
    {
        const string gedcom = @"0 HEAD
1 GEDC
2 VERS 5.5.5
0 @I1@ INDI
1 NAME Test /Person/
1 BIRT
2 DATE 1 JAN 1900
2 PLAC RealBirthPlace
2 AGNC IgnoredBirthPlace
1 DEAT
2 DATE 2 FEB 2000
2 PLAC RealDeathPlace
2 AGNC IgnoredDeathPlace
0 TRLR";
        var (individuals, _) = parser.Parse(gedcom);
        var ind = individuals["@I1@"];
        Assert.Multiple(() =>
        {
            Assert.That(ind.BirthPlace, Is.EqualTo("RealBirthPlace"));
            Assert.That(ind.DeathPlace, Is.EqualTo("RealDeathPlace"));
        });
    }

    [Test]
    public void Parse_FamilyRelations_SetsFatherAndMotherIdOnChildren()
    {
        const string gedcom = @"0 HEAD
1 GEDC
2 VERS 5.5.5
0 @I1@ INDI
1 NAME John /Smith/
0 @I2@ INDI
1 NAME Jane /Doe/
0 @I3@ INDI
1 NAME Junior /Smith/
0 @F1@ FAM
1 HUSB @I1@
1 WIFE @I2@
1 CHIL @I3@
0 TRLR";
        var parser = new GedcomParser();
        var (individuals, families) = parser.Parse(gedcom);
        var child = individuals["@I3@"];
        Assert.Multiple(() =>
        {
            Assert.That(child.FatherId, Is.EqualTo("@I1@"));
            Assert.That(child.MotherId, Is.EqualTo("@I2@"));
        });
    }

    [Test]
    public void Parse_FamilyRelations_MissingHusbandOrWife_SetsOnlyAvailableParentId()
    {
        const string gedcom = @"0 HEAD
1 GEDC
2 VERS 5.5.5
0 @I1@ INDI
1 NAME Jane /Doe/
0 @I2@ INDI
1 NAME Junior /Smith/
0 @F1@ FAM
1 WIFE @I1@
1 CHIL @I2@
0 TRLR";
        var parser = new GedcomParser();
        var (individuals, families) = parser.Parse(gedcom);
        var child = individuals["@I2@"];
        Assert.Multiple(() =>
        {
            Assert.That(child.FatherId, Is.Null.Or.Empty);
            Assert.That(child.MotherId, Is.EqualTo("@I1@"));
        });
    }

    [Test]
    public void Parse_FamilyRelations_MultipleChildren_AllGetParentIds()
    {
        const string gedcom = @"0 HEAD
1 GEDC
2 VERS 5.5.5
0 @I1@ INDI
1 NAME John /Smith/
0 @I2@ INDI
1 NAME Jane /Doe/
0 @I3@ INDI
1 NAME Junior /Smith/
0 @I4@ INDI
1 NAME Jenny /Smith/
0 @F1@ FAM
1 HUSB @I1@
1 WIFE @I2@
1 CHIL @I3@
1 CHIL @I4@
0 TRLR";
        var parser = new GedcomParser();
        var (individuals, families) = parser.Parse(gedcom);
        var child1 = individuals["@I3@"];
        var child2 = individuals["@I4@"];
        Assert.Multiple(() =>
        {
            Assert.That(child1.FatherId, Is.EqualTo("@I1@"));
            Assert.That(child1.MotherId, Is.EqualTo("@I2@"));
            Assert.That(child2.FatherId, Is.EqualTo("@I1@"));
            Assert.That(child2.MotherId, Is.EqualTo("@I2@"));
        });
    }

    [Test]
    public void Parse_FamilyRelations_ChildNotInIndividuals_DoesNotThrow()
    {
        const string gedcom = @"0 HEAD
1 GEDC
2 VERS 5.5.5
0 @I1@ INDI
1 NAME John /Smith/
0 @I2@ INDI
1 NAME Jane /Doe/
0 @F1@ FAM
1 HUSB @I1@
1 WIFE @I2@
1 CHIL @I3@
0 TRLR";
        var parser = new GedcomParser();
        Assert.DoesNotThrow(() => parser.Parse(gedcom));
    }

    [Test]
    public void Parse_FamilyRelations_FamilyWithoutChildren_DoesNotThrow()
    {
        const string gedcom = @"0 HEAD
1 GEDC
2 VERS 5.5.5
0 @I1@ INDI
1 NAME John /Smith/
0 @I2@ INDI
1 NAME Jane /Doe/
0 @F1@ FAM
1 HUSB @I1@
1 WIFE @I2@
0 TRLR";
        var parser = new GedcomParser();
        Assert.DoesNotThrow(() => parser.Parse(gedcom));
    }

    [Test]
    public void AssignAnenummer_AssignsCorrectNumbersToAncestors()
    {
        const string gedcom = @"0 HEAD
1 GEDC
2 VERS 5.5.5
0 @I1@ INDI
1 NAME Proband /Test/
0 @I2@ INDI
1 NAME Father /Test/
0 @I3@ INDI
1 NAME Mother /Test/
0 @I4@ INDI
1 NAME PaternalFather /Test/
0 @I5@ INDI
1 NAME PaternalMother /Test/
0 @I6@ INDI
1 NAME MaternalFather /Test/
0 @I7@ INDI
1 NAME MaternalMother /Test/
0 @F1@ FAM
1 HUSB @I2@
1 WIFE @I3@
1 CHIL @I1@
0 @F2@ FAM
1 HUSB @I4@
1 WIFE @I5@
1 CHIL @I2@
0 @F3@ FAM
1 HUSB @I6@
1 WIFE @I7@
1 CHIL @I3@
0 TRLR";
        var parser = new GedcomParser();
        var (individuals, families) = parser.Parse(gedcom);
        parser.AssignAnenummer(individuals, "@I1@");
        Assert.Multiple(() =>
        {
            Assert.That(individuals["@I1@"].Anenummer, Is.EqualTo(1));
            Assert.That(individuals["@I2@"].Anenummer, Is.EqualTo(2));
            Assert.That(individuals["@I3@"].Anenummer, Is.EqualTo(3));
            Assert.That(individuals["@I4@"].Anenummer, Is.EqualTo(4));
            Assert.That(individuals["@I5@"].Anenummer, Is.EqualTo(5));
            Assert.That(individuals["@I6@"].Anenummer, Is.EqualTo(6));
            Assert.That(individuals["@I7@"].Anenummer, Is.EqualTo(7));
        });
    }

    [Test]
    public void AssignAnenummer_HandlesMissingAncestorsAndNonAncestors()
    {
        const string gedcom = @"0 HEAD
1 GEDC
2 VERS 5.5.5
0 @I1@ INDI
1 NAME Proband /Test/
0 @I2@ INDI
1 NAME Father /Test/
0 @I3@ INDI
1 NAME Mother /Test/
0 @I4@ INDI
1 NAME Sibling /Test/
0 @F1@ FAM
1 HUSB @I2@
1 WIFE @I3@
1 CHIL @I1@
1 CHIL @I4@
0 TRLR";
        var parser = new GedcomParser();
        var (individuals, families) = parser.Parse(gedcom);
        parser.AssignAnenummer(individuals, "@I1@");
        Assert.Multiple(() =>
        {
            Assert.That(individuals["@I1@"].Anenummer, Is.EqualTo(1));
            Assert.That(individuals["@I2@"].Anenummer, Is.EqualTo(2));
            Assert.That(individuals["@I3@"].Anenummer, Is.EqualTo(3));
            Assert.That(individuals["@I4@"].Anenummer, Is.EqualTo(-1)); // Sibling, not in direct line
        });
    }

    [Test]
    public void AssignAnenummer_InvalidProbandId_DoesNotThrowAndNoNumbersAssigned()
    {
        const string gedcom = @"0 HEAD
1 GEDC
2 VERS 5.5.5
0 @I1@ INDI
1 NAME Proband /Test/
0 TRLR";
        var parser = new GedcomParser();
        var (individuals, families) = parser.Parse(gedcom);
        parser.AssignAnenummer(individuals, "@NONEXISTENT@");
        Assert.That(individuals["@I1@"].Anenummer, Is.EqualTo(-1));
    }
}