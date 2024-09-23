namespace LSL.MyToolSetup.Tool.Cli.Infrastructure;

/// <summary>
/// Helper interface to derive a class from for a synchronous command handler
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IHandler<T> : IExecuteCommandLineOptions<T, int>
    where T : ICommandLineOptions {}