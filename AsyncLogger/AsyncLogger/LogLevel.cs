
/// <summary>
/// Author : https://github.com/SwarajKetan/
/// </summary>
namespace AsyncLogger
{
    public enum LogLevel
    {
        //
        // Summary:
        //     Fatal error or application crash.
        Critical = 1,
        //
        // Summary:
        //     Recoverable error.
        Error = 2,
        //
        // Summary:
        //     Noncritical problem.
        Warning = 4,
        //
        // Summary:
        //     Informational message.
        Information = 8,
        //
        // Summary:
        //     Debugging trace.
        Verbose = 16,
    }
}
