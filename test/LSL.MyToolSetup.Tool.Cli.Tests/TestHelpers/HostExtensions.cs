using LSL.AbstractConsole.ServiceProvider;
using LSL.MyToolSetup.Tool.Cli.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace LSL.MyToolSetup.Tool.Cli.Tests.TestHelpers;

public static class HostExtensions
{
    public static async Task<TestHostResult> RunTestCliAsync(this IHost host)
    {
        var result = await host.RunCliAsync();
        var writer = host.Services.GetRequiredService<IOptions<ConsoleOptions>>().Value.TextWriter;
        writer.Flush();

        return new TestHostResult(result, writer.ToString()!);
    }
}