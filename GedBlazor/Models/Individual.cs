using System;
using System.Globalization;
using System.Collections.Generic;

namespace GedBlazor.Models;

public class Individual
{
    public string Id { get; }
    public string? GivenName { get; private set; }
    public string? Surname { get; private set; }
    public string FullName => $"{GivenName} {Surname}".Trim();
    
    public GedcomDate? BirthDate { get; private set; }
    public GedcomDate? DeathDate { get; private set; }
    public string? BirthPlace { get; set; }
    public string? DeathPlace { get; set; }
    public string? FatherId { get; set; }
    public string? MotherId { get; set; }
    public int Anenummer { get; set; } = -1;
    
    public List<Individual> Ancestors { get; set; } = new();
    
    public bool HasAncestors => FatherId != null || MotherId != null;

    // Raw GEDCOM personal data (excluding family link records),
    // e.g. NAME, SEX, BIRT.DATE, BIRT.PLAC, DEAT.DATE, etc.
    public Dictionary<string, List<string>> RawPersonalData { get; } = new(StringComparer.OrdinalIgnoreCase);
    
    public void AddRaw(string tag, string value)
    {
        if (string.IsNullOrWhiteSpace(tag) || string.IsNullOrWhiteSpace(value)) return;
        if (!RawPersonalData.TryGetValue(tag, out var list))
        {
            list = new List<string>();
            RawPersonalData[tag] = list;
        }
        list.Add(value);
    }
    
    

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