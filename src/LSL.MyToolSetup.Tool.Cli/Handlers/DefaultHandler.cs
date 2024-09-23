using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace LSL.MyToolSetup.Tool.Cli.Handlers;

public class DefaultHandler : IAsyncHandler<Default>
{
    private readonly IConsole _console;
    private readonly ILogger<DefaultHandler> _logger;

    public DefaultHandler(IConsole console, ILogger<DefaultHandler> logger)
    {
        _console = console;
        _logger = logger;
    }

    public async Task<int> ExecuteAsync(Default _)
    {
        var copy = CopyResourceToOutputBuilder(Directory.GetCurrentDirectory());
        await copy("appveyor.yml", "appveyor.yml");
        await copy("LSL.snk.enc", "LSL.snk.enc");

        return 0;
    }

    private Func<string, string, Task<string>> CopyResourceToOutputBuilder(string basePath)
    {
        var resources = new EmbeddedFileProvider(typeof(DefaultHandler).Assembly);

        Stream GetStream(string name)
        {
            return resources.GetDirectoryContents("/")
                .First(f => f.Name.EndsWith(name))
                .CreateReadStream();            
        }

        return async (string name, string destinationPath) =>
        {
            var output = GetStream(name);
            
            var fullPath = Path.Combine(basePath, destinationPath);
            using var fs = File.OpenWrite(fullPath);

            await output.CopyToAsync(fs);

            return fullPath;
        };
    }
}