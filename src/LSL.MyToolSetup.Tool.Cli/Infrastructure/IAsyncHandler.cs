namespace LSL.MyToolSetup.Tool.Cli.Infrastructure;

/// <summary>
/// Helper interface to derive a class from for an async command handler
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IAsyncHandler<T> : IExecuteCommandLineOptionsAsync<T, int>
    where T : ICommandLineOptions {}
