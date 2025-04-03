namespace Signals.Dependency.Analyzer.Extensions
{
    internal static class LoggerExtensions
    {
        public static void ToConsole(this string message)
        {
            Console.WriteLine($"{DateTime.Now:yyyy-MM-dd hh:mm:ss} {message}");
        }
    }
}
