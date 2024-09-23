using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace LSL.MyToolSetup.Tool.Cli.Handlers;

public class DefaultHandler : IAsyncHandler<Default>
{
    private readonly IConsole _console;
    private readonly ILogger<DefaultHandler> _logger;
    private readonly string _currentDirectory;

    public DefaultHandler(
        IConsole console,
        ILogger<DefaultHandler> logger,
        IOptions<CommandLineOptions> options)
    {
        _console = console;
        _logger = logger;
        _currentDirectory = options.Value.CurrentDirectory;
    }

    public async Task<int> ExecuteAsync(Default _)
    {
        string ToAbolsutePath(string path) => Path.Join(_currentDirectory, path);
        string ToRelativePath(string path) => path.Replace(_currentDirectory, string.Empty).TrimStart('\\');

        var copy = CopyResourceToOutputBuilder(_currentDirectory);
        await copy("appveyor.yml", "appveyor.yml");
        await copy("LSL.snk.enc", "LSL.snk.enc");
        var projectFolder = ToRelativePath(Directory.GetDirectories(ToAbolsutePath("src"), "*.Cli").First());
        var projectFile = Directory.GetFiles(ToAbolsutePath(projectFolder), "*.csproj").First();
        var testProjectFolder = ToRelativePath(Directory.GetDirectories(ToAbolsutePath("test"), "*.Tests").First());
        var testProjectFile = Directory.GetFiles(ToAbolsutePath(testProjectFolder), "*.csproj").First();

        // Update project file
        var name = new DirectoryInfo(_currentDirectory).Name;
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
        
        // Update Test File
        var testContent = await File.ReadAllTextAsync(testProjectFile);
        testContent = testContent.IndexOf("appveyor") < 0 ? testContent.Replace("<PackageReference Include=\"AutoFixture\" Version=\"4.18.1\" />",
            """
            <PackageReference Include="AutoFixture" Version="4.18.1" />
                <PackageReference Include="appveyor.testlogger" Version="2.0.0" />
                <PackageReference Include="coverlet.msbuild" Version="2.9.0">
                  <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
                  <PrivateAssets>all</PrivateAssets>    
                </PackageReference>            
            """.ReplaceLineEndings())
            : testContent;

        await File.WriteAllTextAsync(testProjectFile, testContent);

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