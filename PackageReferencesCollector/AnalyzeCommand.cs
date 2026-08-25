using Spectre.Console;
using Spectre.Console.Cli;
using System.Text;

namespace PackageReferencesCollector;

public class AnalyzeCommand : Command<AnalyzeSettings>
{
    protected override int Execute(CommandContext context, AnalyzeSettings settings, CancellationToken cancellationToken)
    {
        AnsiConsole.Status()
        .Spinner(Spinner.Known.Star)
        .Start("Analyzing...", ctx =>
        {
            if (!Path.Exists(settings.Directory))
                throw new ArgumentException($"{nameof(settings.Directory)} is exists");

            if (!Analyzer.IsDirectory(settings.Directory))
                throw new ArgumentException($"{nameof(settings.Directory)} is not directory");

            AnalyzeOptions options = settings.Options.ToLower() switch
            {
                "project" => AnalyzeOptions.Project,
                "package" => AnalyzeOptions.Package,
                "both" => AnalyzeOptions.Both,
                _ => AnalyzeOptions.Both
            };

            static string GetName(string location)
            {
                var isProject = location.EndsWith(".csproj");
                return isProject
                    ? Path.GetFileNameWithoutExtension(location) + " (csproj)"
                    : Path.GetFileName(Path.GetDirectoryName(location)) + " (packages)";
            }

            var files = Analyzer.Collect(settings.Directory,
                string.IsNullOrWhiteSpace(settings.Exclude) ?
                Array.Empty<string>() : settings.Exclude.Split(';', StringSplitOptions.RemoveEmptyEntries),
                string.IsNullOrWhiteSpace(settings.OnlyProjects) ?
                Array.Empty<string>() : settings.OnlyProjects.Split(';', StringSplitOptions.RemoveEmptyEntries),
                options);

            var result = Analyzer.Analyze(files);

            if (!settings.DumpToFile || settings.Echo)
            {
                var table = new Table();
                table.AddColumn("#");
                table.AddColumn("Package Name");
                table.AddColumn("Project");
                table.AddColumn("Version");
                var index = 0;
                var rendered = false;
                var packageName = "";

                var packages = settings.All ? result.Packages : result.TrimmedPackages;

                if (!string.IsNullOrWhiteSpace(settings.OnlyLibs))
                {
                    var libs = settings.OnlyLibs.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(x => x.ToLower());
                    packages = packages.Where(x => libs.Contains(x.Name.ToLower())).ToArray();
                }

                var maxPackageNameLength = packages.Max(x => x.Name.Length);

                for (int j = 0; j < packages.Length; j++)
                {
                    var item = packages[j];
                    if (packageName != item.Name)
                    {
                        rendered = false;
                        packageName = item.Name;
                    }

                    var sorted = item.RefDetail.OrderBy(x => x.ProjectLocation).ToList();
                    var highestVersion = result.GetHighestVersion(item.Name);
                    var highestCount = sorted.Count(x => x.Version.CompareTo(highestVersion) >= 0);
                    var allHighest = highestCount == sorted.Count;

                    //Console.WriteLine($"{item.Name} => HighestVer: {highestVersion}, Equal Highest Count: {highestCount}, Total Ref: {sorted.Count()}");
                    for (int i = 0; i < sorted.Count; i++)
                    {
                        var detail = sorted[i];
                        var isProject = detail.ProjectLocation.EndsWith(".csproj");
                        var projectName = isProject
                            ? Path.GetFileNameWithoutExtension(detail.ProjectLocation) + " [#87ff87](csproj)[/]"
                            : Path.GetFileName(Path.GetDirectoryName(detail.ProjectLocation)) + " [#ffffd7](packages)[/]";

                        var isLowerVer = detail.Version.CompareTo(highestVersion) < 0;

                        if (!rendered)
                        {
                            table.AddRow($"[#ff8700]{++index}[/]", item.Name, projectName ?? "", $"[{(isLowerVer ? "red" : $"{(!allHighest ? "green" : "white")}")}]{detail.Version} ({detail.Index}/{detail.Count})[/]");
                            rendered = true;
                        }
                        else
                        {
                            table.AddRow($"", "", projectName ?? "", $"[{(isLowerVer ? "red" : $"{(!allHighest ? "green" : "white")}")}]{detail.Version} ({detail.Index}/{detail.Count})[/]");
                        }

                        if (i == sorted.Count - 1)
                            table.AddRow($"", "".PadRight(maxPackageNameLength, '-'), "".PadRight(50, '-'), "".PadRight(50, '-'));
                    }
                    ctx.Refresh();
                }

                AnsiConsole.Write(Align.Center(table));
            }
            if (settings.DumpToFile)
            {
                var path = "";
                try
                {
                    File.WriteAllText(settings.FilePath, "dump");
                    path = settings.FilePath;
                }
                catch (Exception)
                {
                    path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"analysis_result_{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.txt");
                }

                StringBuilder sb = new();
                var index = 0;
                var rendered = false;
                var packageName = "";
                var libCount = result.PackageNames.Length.ToString().Length;
                var libNameLength = result.PackageNames.Max(x => x.Length);
                var libProjectLocationLength = result.Packages.Max(x => x.RefDetail.Max(x => x.ProjectLocation.Length));
                var libVerLength = result.Packages.Max(x => x.RefDetail.Max(x => (x.Version + " (Highest)").Length));

                //Console.WriteLine($"{libCount},{libNameLength},{libProjectLocationLength},{libVerLength}");
                sb.Append("-".PadRight(libCount + libNameLength + libProjectLocationLength + libVerLength + 5, '-'));
                sb.Append(Environment.NewLine);
                sb.Append('|');
                sb.Append("#".PadLeft(libCount)).Append('|');
                sb.Append("Package Name".PadRight(libNameLength)).Append('|');
                sb.Append("Project Name".PadRight(libProjectLocationLength)).Append('|');
                sb.Append("Version".PadRight(libVerLength));
                sb.Append('|');
                sb.Append(Environment.NewLine);
                sb.Append("-".PadRight(libCount + libNameLength + libProjectLocationLength + libVerLength + 5, '-'));
                sb.Append(Environment.NewLine);

                var packages = settings.All ? result.Packages : result.PackageDifferences;

                foreach (var item in packages)
                {
                    if (packageName != item.Name)
                    {
                        rendered = false;
                        packageName = item.Name;
                    }
                    var sorted = item.RefDetail.OrderBy(x => x.ProjectLocation);
                    var highestVersion = result.GetHighestVersion(item.Name);
                    var allHighest = sorted.Count(x => x.Version.CompareTo(highestVersion) >= 0) == sorted.Count();
                    foreach (var detail in sorted)
                    {
                        var projectName = GetName(detail.ProjectLocation);
                        var isLowerVersion = detail.Version.CompareTo(highestVersion) < 0;

                        if (!rendered)
                        {
                            sb.Append('|')
                              .Append($"{++index}".PadLeft(libCount)).Append('|')
                              .Append(item.Name.PadRight(libNameLength)).Append('|')
                              .Append((projectName ?? "").PadRight(libProjectLocationLength)).Append('|')
                              .Append($"{(isLowerVersion ? detail.Version : $"{(allHighest ? detail.Version : detail.Version + " (Highest)")}")}".PadRight(libVerLength)).Append('|')
                              .Append(Environment.NewLine);
                            rendered = true;
                        }
                        else
                        {
                            sb.Append('|')
                              .Append($"".PadLeft(libCount)).Append('|')
                              .Append("".PadRight(libNameLength)).Append('|')
                              .Append((projectName ?? "").PadRight(libProjectLocationLength)).Append('|')
                              .Append($"{(isLowerVersion ? detail.Version : $"{(allHighest ? detail.Version : detail.Version + " (Highest)")}")}".PadRight(libVerLength)).Append('|')
                              .Append(Environment.NewLine);
                        }
                    }
                }
                ;
                sb.Append("-".PadRight(libCount + libNameLength + libProjectLocationLength + libVerLength + 5, '-'))
                  .AppendLine(); ;

                File.WriteAllText(path, sb.ToString());
            }

            return 0;
        });

        return 0;
    }
}
