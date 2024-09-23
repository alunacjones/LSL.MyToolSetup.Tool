namespace LSL.MyToolSetup.Tool.Cli.Infrastructure;

/// <summary>
/// Options for the CLI runner
/// </summary>
public class CommandLineOptions
{
    /// <summary>
    /// The command line arguments passed to the CLI
    /// </summary>
    /// <value></value>
    public string[] Arguments { get; set; } = default!;
}