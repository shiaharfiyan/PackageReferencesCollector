using Spectre.Console.Cli;

namespace PackageReferencesCollector;

public class Program
{
    static async Task<int> Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(config =>
        {
            config.UseStrictParsing();
            config.AddCommand<AnalyzeCommand>("scan")
                  .WithDescription("Scan folder for Solution/Project dependencies version")
                  .WithAlias("analyze");
        });

        return await app.RunAsync(args);
    }
}

//var dir = @"D:\Midas.Migration\";
//var csprojs = new List<string>();
//var packagesConfigs = new List<string>();

//var list = Analyzer.Collect(dir, AnalyzeOptions.Both);
//var packages = Analyzer.Analyze(list);

//var table = new Table().Centered();

//AnsiConsole.Live(table)
//    .Start(ctx =>
//    {
//        table.AddColumn("#");
//        table.AddColumn("Package Name");
//        table.AddColumn("Project");
//        table.AddColumn("Version");
//        var index = 0;
//        var rendered = false;
//        var packageName = "";

//        foreach (var item in packages.Packages)
//        {
//            if(packageName != item.Name)
//            {
//                rendered = false;
//                packageName = item.Name;
//            }
//            var sorted = item.RefDetail.OrderBy(x => x.ProjectLocation);
//            var highestVersion = packages.GetHighestVersion(item.Name);
//            foreach (var detail in sorted)
//            {
//                var isProject = detail.ProjectLocation.EndsWith(".csproj");
//                var projectName = isProject
//                    ? Path.GetFileNameWithoutExtension(detail.ProjectLocation) + " [#87ff87](csproj)[/]"
//                    : Path.GetFileNameWithoutExtension(Path.GetDirectoryName(detail.ProjectLocation)) + " [#ffffd7](packages)[/]";

//                var lowOrEqual = packages.ToVersion(detail.Version).CompareTo(highestVersion) < 0;

//                if (!rendered)
//                {
//                    table.AddRow($"[#ff8700]{++index}[/]", item.Name, projectName ?? "", $"[{(lowOrEqual ? "red" : "green")}]{detail.Version}[/]");
//                    rendered = true;
//                }
//                else
//                {
//                    table.AddRow($"", "", projectName ?? "", $"[{(lowOrEqual ? "red" : "green")}]{detail.Version}[/]");

//                }
//            }
//            ctx.Refresh();
//        }
//    });

//StringBuilder sb = new();

//CollectCsprojs(dir, csprojs);
//CollectPackagesConfig(dir, packagesConfigs);

//sb.AppendLine("From *.csproj files");
//Console.WriteLine("From *.csproj files");
//AnalyzeCsprojs(csprojs, sb);
//Console.WriteLine();
//sb.AppendLine();
//sb.AppendLine("From packages.config files");
//Console.WriteLine("From packages.config files");
//AnalyzePackageConfigs(packagesConfigs, sb);

//File.WriteAllText(Path.Combine(dir, "dependency_scanner"), sb.ToString());

//static void AnalyzeCsprojs(IEnumerable<string> csprojs, StringBuilder sb)
//{
//    var references = new Dictionary<string, Reference>();
//    foreach (var csproj in csprojs)
//    {
//        var text = File.ReadAllText(csproj);
//        var xmlDocument = new XmlDocument();
//        xmlDocument.LoadXml(text);
//        var xDocument = xmlDocument.ToXDocument();
//        var xElements = xDocument.GetNodeAndDescendants();

//        var refs = new List<Project>();
//        foreach (var item in xElements)
//        {
//            if (item.Name.LocalName == "Reference")
//            {
//                var attr = item.Attribute("Include")?.Value;
//                if (attr != null)
//                {
//                    var libDetails = attr.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
//                    var libName = "";
//                    var libVer = "";
//                    var libPubToken = "";
//                    var libprocessorArchitecture = "";
//                    switch (libDetails.Length)
//                    {
//                        case 2:
//                            libName = libDetails[0].Trim();
//                            libVer = libDetails[1].Trim();
//                            break;
//                        case 3:
//                            libName = libDetails[0].Trim();
//                            libVer = libDetails[1].Trim();
//                            libPubToken = libDetails[2].Trim();
//                            break;
//                        case 4:
//                            libName = libDetails[0].Trim();
//                            libVer = libDetails[1].Trim();
//                            libPubToken = libDetails[2].Trim();
//                            libprocessorArchitecture = libDetails[3].Trim();
//                            break;
//                        case 1:
//                        default:
//                            libName = libDetails[0];
//                            break;
//                    }

//                    //Ignore reference without specific version
//                    if (libDetails.Length == 1 || string.IsNullOrWhiteSpace(libVer.Trim()))
//                        continue;

//                    if (references.TryGetValue(libName.Trim(), out var reference))
//                    {
//                        var proj = reference.Projects.FirstOrDefault(x => x.Version == libVer.Trim());
//                        if (proj == null)
//                        {
//                            reference.Projects.Add(new Project() { Name = csproj, Version = libVer.Trim() });
//                        }
//                        else
//                        {
//                            proj.Name = csproj;
//                        }
//                    }
//                    else
//                    {
//                        var @ref = new Reference()
//                        {
//                            Architecture = libprocessorArchitecture.Trim(),
//                            Token = libPubToken.Trim(),
//                            Name = libName.Trim(),
//                        };
//                        @ref.Projects.Add(new Project() { Name = csproj, Version = libVer.Trim() });
//                        references.Add(libName.Trim(), @ref);
//                    }
//                }
//            }
//        }

//        //var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
//        //foreach (var line in lines)
//        //{
//        //    if (line.Trim().StartsWith("<Reference"))
//        //    {
//        //        refs.Add(new References() { ProjectName = csproj, Reference = line.Trim() });
//        //    }
//        //}
//    }
//    foreach (var item in references.Values.Where(x => x.Projects.Count >= 2))
//    {
//        sb.AppendLine($"Reference Name: {item.Name}");
//        Console.WriteLine($"Reference Name: {item.Name}");
//        foreach (var proj in item.Projects)
//        {
//            sb.AppendLine($"\tProject: {Path.GetFileNameWithoutExtension(proj.Name)} => {proj.Version}");
//            Console.WriteLine($"\tProject: {Path.GetFileNameWithoutExtension(proj.Name)} => {proj.Version}");
//        }
//    }
//}

//static void AnalyzePackageConfigs(IEnumerable<PackageLocation> packagesConfigs, StringBuilder sb)
//{
//    var packages = new Dictionary<string, Package>();
//    foreach (var pc in packagesConfigs)
//    {
//        var text = File.ReadAllText(pc.Location);
//        var xmlDocument = new XmlDocument();
//        xmlDocument.LoadXml(text);
//        var xDocument = xmlDocument.ToXDocument();
//        var xElements = xDocument.GetNodeAndDescendants();

//        var refs = new List<Project>();
//        foreach (var item in xElements)
//        {
//            if (item.Name.LocalName.ToLower() == "package")
//            {
//                var id = item.Attribute("id")?.Value;
//                var version = item.Attribute("version")?.Value;
//                var targetFramework = item.Attribute("targetFramework")?.Value;

//                var libName = id == null ? "" : id.Trim();
//                var libVer = version == null ? "" : version.Trim();
//                var libTargetFramework = targetFramework == null ? "" : targetFramework.Trim();

//                if (string.IsNullOrWhiteSpace(libName))
//                    continue;

//                if (packages.TryGetValue(libName.Trim(), out var reference))
//                {
//                    var proj = reference.Projects.FirstOrDefault(x => x.Version == libVer.Trim());
//                    if (proj == null)
//                    {
//                        reference.Projects.Add(new Project() { Name = pc.DirectoryName, Version = libVer.Trim() });
//                    }
//                    else
//                    {
//                        proj.Name = pc.DirectoryName;
//                    }
//                }
//                else
//                {
//                    var @ref = new Package()
//                    {
//                        TargetFramework = libTargetFramework.Trim(),
//                        Id = libName.Trim()
//                    };
//                    @ref.Projects.Add(new Project() { Name = pc.DirectoryName, Version = libVer.Trim() });
//                    packages.Add(libName.Trim(), @ref);
//                }
//            }
//        }
//    }

//    foreach (var item in packages.Values.Where(x => x.Projects.Count >= 2))
//    {
//        sb.AppendLine($"Package Name: {item.Id}");
//        Console.WriteLine($"Package Name: {item.Id}");
//        foreach (var proj in item.Projects)
//        {
//            sb.AppendLine($"\tProject: {proj.Name} => {proj.Version}");
//            Console.WriteLine($"\tProject: {proj.Name} => {proj.Version}");
//        }
//    }
//}

//static void CollectCsprojs(string dir, List<string> csprojs)
//{
//    var files = Directory.GetFiles(dir, "*.csproj");
//    var dirs = Directory.GetDirectories(dir);
//    csprojs.AddRange(files);

//    foreach (var item in dirs)
//    {
//        CollectCsprojs(item, csprojs);
//    }
//}

//static void CollectPackagesConfig(string dir, List<PackageLocation> packagesConfigs)
//{
//    var files = Directory.GetFiles(dir, "packages.config");
//    var dirs = Directory.GetDirectories(dir);
//    packagesConfigs.AddRange(files.Select(x => new PackageLocation()
//    {
//        Directory = Path.GetDirectoryName(x),
//        Location = x
//    }));

//    foreach (var item in dirs)
//    {
//        CollectPackagesConfig(item, packagesConfigs);
//    }
//}

//class Project
//{
//    public string Name { get; set; }

//    public string Version { get; set; }
//}

//class Reference
//{
//    public string Name { get; set; }
//    public string Token { get; set; }
//    public string Architecture { get; set; }
//    public List<Project> Projects { get; set; } = new();
//}

//class Package
//{
//    public string Id { get; set; }
//    public string TargetFramework { get; set; }
//    public List<Project> Projects { get; set; } = new();
//}

//class PackageLocation
//{
//    public string Directory { get; set; }
//    public string DirectoryName => Path.GetFileName(Directory) ?? "";
//    public string Location { get; set; }
//}