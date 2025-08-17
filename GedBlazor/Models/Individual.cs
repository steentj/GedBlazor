using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

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

    // Completeness evaluation helpers
    private bool HasName => !string.IsNullOrWhiteSpace(GivenName) || !string.IsNullOrWhiteSpace(Surname);
    private bool HasBirthOrDeathDate => BirthDate.HasValue || DeathDate.HasValue;
    private bool HasBirthAndDeathPlaces => !string.IsNullOrWhiteSpace(BirthPlace) && !string.IsNullOrWhiteSpace(DeathPlace);
    private bool HasBirthOrDeathSource =>
        (RawPersonalData.TryGetValue("BIRT.SOUR", out var bSour) && bSour.Count > 0) ||
        (RawPersonalData.TryGetValue("DEAT.SOUR", out var dSour) && dSour.Count > 0);
    private bool HasResidency => RawPersonalData.ContainsKey("RESI") || RawPersonalData.Keys.Any(k => k.StartsWith("RESI.", StringComparison.OrdinalIgnoreCase));

    // Status: 0 = no name; 1 = name; 2 = name + date; 3 = status 2 + source for birth/death + both birth/death places; 4 = status 3 + one or more residencies
    public int CompletionStatus
    {
        get
        {
            if (!HasName) return 0;
            if (!HasBirthOrDeathDate) return 1;
            var hasBase3 = HasBirthOrDeathSource && HasBirthAndDeathPlaces;
            if (!hasBase3) return 2;
            return HasResidency ? 4 : 3;
        }
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