using LSL.AbstractConsole.ServiceProvider;
using LSL.MyToolSetup.Tool.Cli.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace LSL.MyToolSetup.Tool.Cli.Tests.TestHelpers;

/// <summary>
/// A helper base class to inherit from for creating unit tests
/// </summary>
public abstract class BaseCliTest
{
    /// <summary>
    /// Builds a delegate that be used to unit test the CLI
    /// </summary>
    /// <param name="args">The command line arguments that the test host instance should receive</param>
    /// <param name="servicesConfigurator">Further setup of the host's service collection e.g. for adding mocks</param>
    /// <returns>A delegate to invoke the test CLI</returns>
    protected static Func<Task<TestHostResult>> BuildTestHostRunner(
        string[] args,
        Action<IServiceCollection>? servicesConfigurator = null)
    {
        var writer = new StringWriter();

        return async () => await HostBuilderFactory.Create(args)
            .ConfigureServices((context, services) =>
            {
                services.Configure<ConsoleOptions>(s =>
                {
                    s.TextWriter = writer;
                });

                servicesConfigurator?.Invoke(services);
            })
            .Build()
            .RunTestCliAsync();
    }
}