namespace PackageReferencesCollector.Models;

public class Package
{
    public string Name { get; set; } = string.Empty;
    public List<PackageRefDetail> RefDetail { get; set; } = new();
}
