namespace GedBlazor.Models;

public class Family
{
    public string Id { get; }
    public Individual? Husband { get; private set; }
    public Individual? Wife { get; private set; }
    private readonly List<Individual> _children = new();
    public IReadOnlyList<Individual> Children => _children.AsReadOnly();

    public Family(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentNullException(nameof(id));
        
        Id = id;
    }

    public void SetHusband(Individual husband)
    {
        Husband = husband ?? throw new ArgumentNullException(nameof(husband));
    }

    public void SetWife(Individual wife)
    {
        Wife = wife ?? throw new ArgumentNullException(nameof(wife));
    }

    public void AddChild(Individual child)
    {
        if (child == null)
            throw new ArgumentNullException(nameof(child));
        
        if (!_children.Contains(child))
            _children.Add(child);
    }

    public override string ToString()
    {
        var spouses = $"{Husband?.FullName ?? ""} + {Wife?.FullName ?? ""}".Trim(" +".ToCharArray());
        var familyInfo = $"Family {Id}: {spouses}";
        
        if (_children.Count > 0)
        {
            var childrenNames = string.Join(", ", _children.Select(c => c.FullName));
            familyInfo += $", Children: {childrenNames}";
        }
        
        return familyInfo;
    }
}