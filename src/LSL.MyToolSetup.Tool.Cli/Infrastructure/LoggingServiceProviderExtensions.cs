using LSL.Tool.Logger;
using Microsoft.Extensions.DependencyInjection;

namespace LSL.MyToolSetup.Tool.Cli.Infrastructure;

public static class LoggingServiceProviderExtensions
{
    /// <summary>
    /// Adds logging to the host instance using `IConsole` for all logging
    /// </summary>
    /// <param name="source"></param>
    /// <param name="isVerbose"></param>
    /// <returns>The original <c>IServiceCollection</c></returns>
    public static IServiceCollection AddCliLogging(this IServiceCollection source, bool isVerbose)
    {        
        return source.AddLogging(c => 
        {
            c.ClearProviders();

            if (isVerbose)
            {
                c.Services.AddDotNetToolLogger();                
                c.SetMinimumLevel(LogLevel.Debug);
            }
        });
    }
}