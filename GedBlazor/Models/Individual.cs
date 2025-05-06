using System.Globalization;

namespace GedBlazor.Models;

public class Individual
{
    public string Id { get; }
    public string? GivenName { get; private set; }
    public string? Surname { get; private set; }
    public string FullName => $"{GivenName} {Surname}".Trim();
    
    public GedcomDate? BirthDate { get; private set; }
    public GedcomDate? DeathDate { get; private set; }

    public Individual(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));
        
        Id = id;
    }

    public void SetName(string givenName, string surname)
    {
        GivenName = givenName ?? throw new ArgumentNullException(nameof(givenName));
        Surname = surname ?? throw new ArgumentNullException(nameof(surname));
    }

    public void SetBirth(GedcomDate date)
    {
        BirthDate = date;
    }

    public void SetDeath(GedcomDate date)
    {
        DeathDate = date;
    }

    public override string ToString()
    {
        var lifespan = "";
        if (BirthDate.HasValue || DeathDate.HasValue)
        {
            var birth = BirthDate?.ToString() ?? "";
            var death = DeathDate?.ToString() ?? "";
            lifespan = $" ({birth} - {death})";
        }

        return $"{FullName}{lifespan}";
    }
}