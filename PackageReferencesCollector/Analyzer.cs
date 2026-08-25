using System.Text.RegularExpressions;
using System.Xml;

namespace PackageReferencesCollector;

public static class Analyzer
{
    private const string Both = "*.csproj;packages.config";
    private const string Project = "*.csproj";
    private const string Package = "packages.config";

    private static Dictionary<AnalyzeOptions, string> _options;

    static Analyzer()
    {
        _options = new()
        {
            { AnalyzeOptions.Both, Both },
            { AnalyzeOptions.Project, Project },
            { AnalyzeOptions.Package, Package },
        };
    }

    public static IEnumerable<string> Collect(string dir, string[] excludes, string[] onlyProjects, AnalyzeOptions opt)
    {
        excludes = excludes.Select(x => x.ToLower()).ToArray();
        onlyProjects = onlyProjects.Select(x => x.ToLower()).ToArray();

        var files = GetFiles(dir, _options[opt])
               .Where(x => !excludes.Contains(GetName(x).ToLower()));

        if (onlyProjects.Length > 0)
            files = files.Where(x => onlyProjects.Contains(GetName(x).ToLower()));

        return files;
    }

    public static AnalyzeResult Analyze(IEnumerable<string> list)
    {
        var packages = new Dictionary<string, Package>();

        foreach (var item in list)
        {
            var isProject = item.ToLower().EndsWith(".csproj");
            if (isProject)
                HandleCsprojs(item, packages);
            else
                HandlePackage(item, packages);

        }

        return new AnalyzeResult(packages);
    }

    private static void HandleCsprojs(string csproj, Dictionary<string, Package> packages)
    {
        var text = File.ReadAllText(csproj);
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(text);
        var xDocument = xmlDocument.ToXDocument();
        var xElements = xDocument.GetNodeAndDescendants();

        var refXElements = xElements.Where(x => x.Name.LocalName == "Reference" || x.Name.LocalName == "PackageReference").ToArray();
        foreach (var item in refXElements)
        {
            var libName = "";
            var libVer = "";

            var attr = item.Attribute("Include")?.Value;
            if (attr != null)
            {
                if (item.Name.LocalName == "Reference")
                {
                    var libDetails = attr.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    switch (libDetails.Length)
                    {
                        case 2:
                        case 3:
                        case 4:
                            libName = libDetails[0].Trim();
                            libVer = libDetails[1].Trim();
                            break;
                        case 1:
                        default:
                            libName = libDetails[0];
                            break;
                    }

                    //Ignore reference without specific version
                    if (libDetails.Length == 1 || string.IsNullOrWhiteSpace(libVer.Trim()))
                        continue;
                }
                else if (item.Name.LocalName == "PackageReference")
                {
                    libName = attr.Trim();
                    var ver = item.Attribute("Version")?.Value;
                    if (ver != null) libVer = ver.Trim();
                }
            }

            if (string.IsNullOrWhiteSpace(libName))
                continue;

            Set(libName, libVer, csproj, packages);
        }
    }

    private static void HandlePackage(string pc, Dictionary<string, Package> packages)
    {
        var text = File.ReadAllText(pc);
        var xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(text);
        var xDocument = xmlDocument.ToXDocument();
        var xElements = xDocument.GetNodeAndDescendants();

        foreach (var item in xElements)
        {
            if (item.Name.LocalName.ToLower() == "package")
            {
                var id = item.Attribute("id")?.Value;
                var version = item.Attribute("version")?.Value;

                var libName = id == null ? "" : id.Trim();
                var libVer = version == null ? "" : version.Trim();

                if (string.IsNullOrWhiteSpace(libName))
                    continue;

                Set(libName, libVer, pc, packages);
            }
        }
    }

    private static void Set(string libName, string libVer, string location, Dictionary<string, Package> packages)
    {
        if (!string.IsNullOrWhiteSpace(libName))
        {
            if (packages.TryGetValue(libName.Trim(), out var package))
            {
                var proj = package.RefDetail.FirstOrDefault(x => x.ProjectName == GetName(location));
                if (proj != null)
                {
                    proj.Count++;
                    if (proj.Version.CompareTo(ToVersion(libVer.Replace("Version=", ""))) < 0)
                    {
                        proj.Index++;
                        proj.ProjectLocation = location;
                        proj.Version = ToVersion(libVer.Replace("Version=", ""));
                    }
                }
                else
                {
                    package.RefDetail.Add(new PackageRefDetail()
                    {
                        ProjectLocation = location,
                        Count = 1,
                        Index = 1,
                        Version = ToVersion(libVer.Replace("Version=", ""))
                    });
                }
            }
            else
            {
                var newPackage = new Package()
                {
                    Name = libName.Trim(),
                };
                newPackage.RefDetail.Add(new PackageRefDetail()
                {
                    ProjectLocation = location,
                    Count = 1,
                    Index = 1,
                    Version = ToVersion(libVer.Replace("Version=", "").Trim())
                });
                packages.Add(libName.Trim(), newPackage);
            }
        }
    }

    public static bool IsDirectory(string path)
    {
        return (File.GetAttributes(path) & FileAttributes.Directory)
             == FileAttributes.Directory;
    }

    private static string[] GetFiles(string sourceFolder, string filters, SearchOption searchOption = SearchOption.AllDirectories)
    {
        return filters.Split(';').SelectMany(filter => Directory.GetFiles(sourceFolder, filter, searchOption)).ToArray();
    }

    private static string RegexVersion(string version)
    {
        var regex = new Regex("\\d+(?:\\.\\d+)+");
        var match = regex.Match(version);
        return match.Groups[0].Value;
    }

    public static Version ToVersion(string version)
    {
        if (Version.TryParse(RegexVersion(version), out var ver))
            return ver;

        return new Version(0, 0, 0, 0);
    }

    public static string GetName(string location)
    {
        var isProject = location.EndsWith(".csproj");
        return isProject
            ? $"{Path.GetFileNameWithoutExtension(location)}"
            : $"{Path.GetFileName(Path.GetDirectoryName(location))}";
    }
}