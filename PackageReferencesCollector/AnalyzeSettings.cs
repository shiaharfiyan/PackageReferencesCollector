using Spectre.Console.Cli;
using System.ComponentModel;

namespace PackageReferencesCollector;

public class AnalyzeSettings : CommandSettings
{
    [CommandArgument(0, "<PROJECT_DIRECTORY>")]
    [Description("Solution/Project directory contains *.csproj or packages.config")]
    public string Directory { get; set; } = string.Empty;

    [CommandOption("-o|--option <ANALYZE_OPTION>")]
    [Description("Analyze Options, Both *.csproj and packages.config, Project for *.csproj and Package for packages.config")]
    public string Options { get; set; } = "both";

    [CommandOption("-e|--echo")]
    [Description("Display analysis on screen")]
    public bool Echo { get; set; } = false;

    [CommandOption("-a|--all")]
    [Description("Display all library, both synced and unsynced (different versions)")]
    public bool All { get; set; } = false;

    [CommandOption("-d|--dump")]
    [Description("Dump analysis to file specified using -f option")]
    public bool DumpToFile { get; set; } = false;

    [CommandOption("--exclude <PROJECT_NAME>")]
    [Description("Exclude project name delimited by semicolon (;)")]
    public string Exclude { get; set; } = "";

    [CommandOption("--onlyLibs <LIB_NAME>")]
    [Description("Only display lib name delimited by semicolon (;)")]
    public string OnlyLibs { get; set; } = "";

    [CommandOption("--onlyProjects <PROJECT_NAME>")]
    [Description("Only display project name delimited by semicolon (;)")]
    public string OnlyProjects { get; set; } = "";

    [CommandOption("-f|--file <FILEPATH>")]
    [Description("File location for dump")]
    public string FilePath { get; set; } = string.Empty;
}
