namespace Signals.Dependency.Analyzer.Atoms
{
    public class SignalsProcessType
    {
        /// <summary>
        /// Assigned value from the enum
        /// </summary>
        public string Value { get; set; }

        public static readonly string BusinessProcess = "BusinessProcess";
        public static readonly string DistributedProcess = "DistributedProcess";
        public static readonly string BaseFileExportProcess = "BaseFileExportProcess";
        public static readonly string BaseFileImportProcess = "BaseFileImportProcess";
        public static readonly string RecurringProcess = "RecurringProcess";
        public static readonly string NoOverlapRecurringProcess = "NoOverlapRecurringProcess";
        public static readonly string ApiProcess = "ApiProcess";
        public static readonly string ProxyApiProcess = "ProxyApiProcess";
        public static readonly string AutoApiProcess = "AutoApiProcess";
        private SignalsProcessType(string value)
        {
            Value = value;
        }

        public static readonly List<SignalsProcessType> ProcessTypes =
        [
            new SignalsProcessType(BusinessProcess),
            new SignalsProcessType(DistributedProcess),
            new SignalsProcessType(BaseFileExportProcess),
            new SignalsProcessType(BaseFileImportProcess),
            new SignalsProcessType(RecurringProcess),
            new SignalsProcessType(NoOverlapRecurringProcess),
            new SignalsProcessType(ApiProcess),
            new SignalsProcessType(ProxyApiProcess),
            new SignalsProcessType(AutoApiProcess)
        ];
    }
}
