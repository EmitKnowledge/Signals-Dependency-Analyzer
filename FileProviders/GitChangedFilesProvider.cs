using System.Diagnostics;

namespace Signals.Dependency.Analyzer.FileProviders
{
    public class GitChangedFilesProvider(string gitProjectRootPath)
    {
        public string Path { get; private set; } = gitProjectRootPath;

        /// <summary>
        /// Return the current active branch
        /// </summary>
        /// <returns></returns>
        private string GetCurrentBranch()
        {
            string gitCommand = "git";
            string gitArgs = "rev-parse --abbrev-ref HEAD";

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = gitCommand,
                Arguments = gitArgs,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path
            };

            using Process process = Process.Start(processStartInfo);
            using var reader = process.StandardOutput;
            string currentBranch = reader.ReadLine()?.Trim();
            return currentBranch;
        }

        /// <summary>
        /// Return the path to the *.sln project
        /// </summary>
        /// <returns></returns>
        public string GetSolutionPath()
        {
            if (!Directory.Exists(Path)) return null;

            string[] solutionFiles = Directory.GetFiles(Path, "*.sln", SearchOption.AllDirectories);
            var solutionFile = solutionFiles.FirstOrDefault();
            return solutionFile;
        }

        /// <summary>
        /// Return all changed files within the git repo
        /// </summary>
        public List<string> GetFiles()
        {
            List<string> changedFiles = new List<string>();

            string currentBranch = GetCurrentBranch();
            string remoteBranch = "origin/" + currentBranch;
            // run 'git diff --name-only HEAD..origin/{branch_name}' to get files changed between the local branch and remote branch
            string gitCommand = "git";
            string gitArgs = $"diff --name-only HEAD..{remoteBranch}";

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = gitCommand,
                Arguments = gitArgs,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Path
            };

            using Process process = Process.Start(processStartInfo);
            using var reader = process.StandardOutput;
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Replace('/', '\\').Trim();
                var fullFilePath = System.IO.Path.Combine(Path, line);
                changedFiles.Add(fullFilePath);
            }
            return changedFiles;
        }
    }
}
