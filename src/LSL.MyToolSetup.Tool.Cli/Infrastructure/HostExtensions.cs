using LSL.AbstractConsole.ServiceProvider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace LSL.MyToolSetup.Tool.Cli.Infrastructure;

public static class HostExtensions
{
    /// <summary>
    /// Runs the CLI
    /// </summary>
    /// <param name="host">The host instance to run</param>
    /// <returns>The exit code from the executed handler</returns>
    public static async Task<int> RunCliAsync(this IHost host)
    {
        var args = host.Services.GetRequiredService<IOptions<CommandLineOptions>>().Value.Arguments;
        var consoleWriter = host.Services.GetRequiredService<IOptions<ConsoleOptions>>().Value.TextWriter;

        var services = host.Services;

        try
        {
            return await services
                .GetRequiredService<ICommandLineParser<int>>()
                .ParseArgumentsAsync(
                    args,
                    c => c.HelpWriter = consoleWriter);
        }
        catch (ArgumentOutOfRangeException)
        {
            // This is thrown when the cli project has not yet had Options or Handlers added yet
            services.GetRequiredService<IConsole>().WriteLine("It looks like this project has not yet added any Options or Handlers for the CLI");
            return 1;
        }
    }
}