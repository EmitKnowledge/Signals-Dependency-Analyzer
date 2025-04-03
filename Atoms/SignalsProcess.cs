namespace Signals.Dependency.Analyzer.Atoms
{
    /// <summary>
    /// CTOR
    /// </summary>
    /// <param name="processName"></param>
    public class SignalsProcess(string processName, SignalsProcessType type = null, string filePath = "", int line = 0, int depth = 0)
    {
        /// <summary>
        /// Name of the process
        /// </summary>
        public string Name { get; set; } = processName;

        public SignalsProcessType Type { get; set; } = type;

        /// <summary>
        /// Represents the filepath to the source code of the process
        /// </summary>
        public string FilePath { get; set; } = filePath;

        /// <summary>
        /// Reprensets the line where this process is in use. Available in the subprocesses
        /// </summary>
        public int Line { get; set; } = line;

        /// <summary>
        /// Represents the depth level in the hierarchy tree
        /// </summary>
        public int Depth { get; set; } = depth;

        private object _subProcessLock = new();
        /// <summary>
        /// List of nested processes (subprocesses)
        /// </summary>
        public List<SignalsProcess> Subprocesses { get; set; } = [];

        /// <summary>
        /// Add a subprocess to the current process (nested SignalsProcess).
        /// </summary>
        /// <param name="subprocessName">The name of the subprocess to add</param>
        public SignalsProcess AddSubprocess(string subprocessName, SignalsProcessType type, string filePath, int line)
        {
            // create a new SignalsProcess for the subprocess and add it to the list
            var process = new SignalsProcess(subprocessName);
            process.Type = type;
            process.FilePath = filePath;
            process.Line = line;
            lock (_subProcessLock)
            {
                Subprocesses.Add(process);
            }
            return process;
        }

        /// <summary>
        /// Add a subprocess to the current process (nested SignalsProcess).
        /// </summary>
        /// <param name="subprocessName">The name of the subprocess to add</param>
        public SignalsProcess AddSubprocess(SignalsProcess process)
        {
            // create a new SignalsProcess for the subprocess and add it to the list
            lock (_subProcessLock)
            {
                Subprocesses.Add(process);
            }
            return process;
        }

        /// <summary>
        /// Recursively flattens the hierarchy of processes and orders them from leaf to root.
        /// Also, assigns a depth level (leaf processes start at depth 0, parent processes increment depth).
        /// </summary>
        private List<SignalsProcess> GetAllProcesses(List<SignalsProcess> processes, int depth = 0)
        {
            var allProcesses = new List<SignalsProcess>();

            foreach (var process in processes)
            {
                process.Depth = depth;
                allProcesses.Add(process);
                var subprocesses = GetAllProcesses(process.Subprocesses, depth + 1);
                allProcesses.AddRange(subprocesses);
            }

            return allProcesses;
        }


        /// <summary>
        /// Recursively flattens the hierarchy of processes and orders them from leaf to root.
        /// Also, assigns a depth level (leaf processes start at depth 0, parent processes increment depth).
        /// </summary>
        /// <returns></returns>
        public List<SignalsProcess> FlattenDesc()
        {
            return GetAllProcesses([this]);
        }
    }
}
