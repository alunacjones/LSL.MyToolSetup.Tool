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
        var basePath = Directory.GetCurrentDirectory();

        string ToAbolsutePath(string path) => Path.Join(basePath, path);
        string ToRelativePath(string path) => path.Replace(basePath, string.Empty).TrimStart('\\');

        var copy = CopyResourceToOutputBuilder(basePath);
        await copy("appveyor.yml", "appveyor.yml");
        await copy("LSL.snk.enc", "LSL.snk.enc");
        var projectFolder = ToRelativePath(Directory.GetDirectories(ToAbolsutePath("src"), "*.Cli").First());
        var projectFile = Directory.GetFiles(ToAbolsutePath(projectFolder), "*.csproj").First();

        var name = new DirectoryInfo(basePath).Name;
        var content = await File.ReadAllTextAsync(projectFile);
        content = content.IndexOf("LSL.snk") < 0 ? content.Replace("<Project Sdk=\"Microsoft.NET.Sdk\">",
            """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <SnkFile>../../LSL.snk</SnkFile>
              </PropertyGroup>

              <PropertyGroup Condition="Exists('$(SnkFile)')">
                <AssemblyOriginatorKeyFile>$(SnkFile)</AssemblyOriginatorKeyFile>
                <SignAssembly>True</SignAssembly>
              </PropertyGroup>
            
            """.ReplaceLineEndings()
        )
        : content;

        content = content.Replace("<Authors>authors-here</Authors>", "<Authors>alunacjones</Authors>")
            .Replace("<RepositoryUrl>https://github.com/your/repo-url</RepositoryUrl>", $"<RepositoryUrl>https://github.com/alunacjones/{name}</RepositoryUrl>")
            .Replace("<PackageProjectUrl>https://github.com/your/project-url</PackageProjectUrl>", $"<PackageProjectUrl>https://github.com/alunacjones/{name}</PackageProjectUrl>");

        await File.WriteAllTextAsync(projectFile, content);
        
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
            using var fs = File.Create(fullPath);

            await output.CopyToAsync(fs);

            return fullPath;
        };
    }
}