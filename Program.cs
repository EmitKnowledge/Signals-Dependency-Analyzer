using Signals.Dependency.Analyzer.Analyzers;
using Signals.Dependency.Analyzer.Atoms;
using Signals.Dependency.Analyzer.Extensions;
using Signals.Dependency.Analyzer.FileProviders;
using Signals.Dependency.Analyzer.Reporting;

"Paste the filepath to solution containing the .git folder.".ToConsole();
string gitPath = Console.ReadLine();
if (!Directory.Exists(gitPath))
{
    $"The provided path: {gitPath} does not exist! Bye bye...".ToConsole();
    return;
}

"Signals process dependencies analysis started.".ToConsole();
$"Analysing {gitPath}".ToConsole();

"Initialize the git file provider".ToConsole();
var provider = new GitChangedFilesProvider(gitPath);
var slnPath = provider.GetSolutionPath();
if (string.IsNullOrEmpty(slnPath))
{
    $"Solution file found can't be found!".ToConsole();
    return;
}
else
{
    $"Solution file found at: {slnPath}.".ToConsole();
}

"Get the changed files.".ToConsole();
var files = provider.GetFiles();
var processes = new List<SignalsProcess>();
var processesLock = new object();

"Init the analyzer.".ToConsole();
var analyzer = new SignalsDependencyAnalyzer(slnPath);
analyzer.Setup();
Parallel.ForEach(files, file =>
{
    $"Analyzing {file}".ToConsole();
    var process = analyzer.Analyze(file);
    if (process != null)
    {
        lock (processesLock)
        {
            processes.Add(process);
        }
    }
});

"Exporting the process and their dependencies to an Excel file.".ToConsole();
var reporting = new SignalsDependencyReporting();
var reportPath = reporting.Export(processes);
$"Report file: {reportPath}.".ToConsole();

"Signals process dependencies analysis completed.".ToConsole();