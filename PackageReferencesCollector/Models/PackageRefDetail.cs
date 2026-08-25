namespace PackageReferencesCollector.Models;

public class PackageRefDetail
{
    public string ProjectLocation { get; set; } = string.Empty;

    public string ProjectName
    {
        get
        {
            return Analyzer.GetName(ProjectLocation);
        }
    }

    public int Index { get; set; }

    public int Count { get; set; }

    public Version Version { get; set; } = new Version(0, 0, 0, 0);
}
