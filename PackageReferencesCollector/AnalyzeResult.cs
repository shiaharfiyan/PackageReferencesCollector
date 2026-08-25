using System.Text.RegularExpressions;

namespace PackageReferencesCollector;

public class AnalyzeResult
{
    private Dictionary<string, Package> _packages = new();

    internal AnalyzeResult(Dictionary<string, Package> packages)
    {
        _packages = packages;
    }

    public string[] PackageNames
    {
        get
        {
            return _packages.Keys.OrderBy(x => x).ToArray();
        }
    }

    public Package[] Packages
    {
        get
        {
            return _packages.Values.OrderBy(x => x.Name).ToArray();
        }
    }

    public Version? GetHighestVersion(string packageName)
    {
        if (_packages.TryGetValue(packageName, out var package))
            return package.RefDetail.Max(x => x.Version);

        return null;
    }

    public Dictionary<string, Package> GetPackageDifferences()
    {
        var zero = new Version(0, 0, 0, 0);
        return _packages.Where(x => x.Value.RefDetail
                .Where(x => x.Version.CompareTo(zero) != 0)
                .Select(x => x.Version).Distinct().Count() > 1)
                .OrderBy(x => x.Value.Name)
                .ToDictionary(x => x.Key, y => y.Value);
    }

    public Package[] PackageDifferences
    {
        get
        {
            var zero = new Version(0, 0, 0, 0);
            return _packages.Values
                .Where(x => x.RefDetail.Where(x => x.Version.CompareTo(zero) != 0).Select(x => x.Version).Distinct().Count() > 1)
                .OrderBy(x => x.Name)
                .ToArray();
        }
    }

    public Package[] TrimmedPackages
    {
        get
        {
            var zero = new Version(0, 0, 0, 0);
            return _packages.Values
                .Where(x => x.RefDetail.Where(x => x.Version.CompareTo(zero) != 0).Max(x => x.Count) >= 1)
                .Where(x => x.RefDetail.Where(x => x.Version.CompareTo(zero) != 0).Select(x => x.Version).Distinct().Count() > 1)
                .OrderBy(x => x.Name)
                .ToArray();
        }
    }

    public PackageRefDetail[] GetPackageReferences(string packageName)
    {
        if (_packages.TryGetValue(packageName, out var package))
            return package.RefDetail.ToArray();

        return Array.Empty<PackageRefDetail>();
    }

    private string RegexVersion(string version)
    {
        var regex = new Regex("\\d+(?:\\.\\d+)+");
        var match = regex.Match(version);
        return match.Groups[0].Value;
    }

    public Version ToVersion(string version)
    {
        if (Version.TryParse(RegexVersion(version), out var ver))
            return ver;

        return new Version(0, 0, 0, 0);
    }
}
