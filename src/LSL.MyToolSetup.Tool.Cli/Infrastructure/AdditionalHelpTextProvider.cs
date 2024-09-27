namespace LSL.MyToolSetup.Tool.Cli.Infrastructure;

/// <summary>
/// Provides additional help text showing the <c>--verbose</c>
/// </summary>
public class AdditionalHelpTextProvider : IExecuteParsingFailure<int>
{
    private readonly IConsole _console;

    public AdditionalHelpTextProvider(IConsole console)
    {
        _console = console;
    }

    public int Execute(string[] args, IEnumerable<Error> errors)
    {
        if (!errors.IsVersion()) _console.WriteLine("NOTE: a global --verbose flag can be used to provide debug logging");

        return 0;
    }
}
