using System.Xml.Linq;
using Baseline;
using Diamond.Core.System.TemporaryFolder;
using LSL.MyToolSetup.Tool.Cli.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace LSL.MyToolSetup.Tool.Cli.Tests.Handlers;

public class DefaultHandlerTests : BaseCliTest
{
    [Test]
    public async Task GivenAValidFolderStructure_ThenItShouldUpdateAsExpected()
    {
        // Arrange
        using var tempFolder = new TemporaryFolderFactory().Create();
        
        var sut = BuildTestHostRunner(
            [],
            s => s.Configure<CommandLineOptions>(o => o.CurrentDirectory = tempFolder.FullPath));
            
        var (projectFile, testProjectFile) = await BuildProjectStructure(tempFolder.FullPath);

        // Act (run twice to ensure we still get the same output for multiple uses)
        await sut();
        var (result, _) = await sut();

        // Assert

        using var _ = new AssertionScope();

        result.Should().Be(0);
        await AssertFileExpectations(tempFolder.FullPath, projectFile, testProjectFile);
        File.Exists(Path.Combine(tempFolder.FullPath, "appveyor.yml")).Should().BeTrue();
        File.Exists(Path.Combine(tempFolder.FullPath, "LSL.snk.enc")).Should().BeTrue();
    }

    private static async Task AssertFileExpectations(string basePath, params string[] files)
    {
        foreach (var file in files)
        {
            var content = await File.ReadAllTextAsync(file);
            var expectedContent = await GetEmbeddedFileTextAsync($"Expected-{Path.GetFileName(file)}");

            XDocument.Parse(content).Should().BeEquivalentTo(
                XDocument.Parse(
                    expectedContent.Replace("${WorkingFolder}", Path.GetFileName(basePath))
                )
            );
        }
    }

    private static async Task<(string projectFilePath, string testFilePath)> BuildProjectStructure(string targetPath)
    {
        var filesToPlace = new[]
        {
            new { Name = "HandlerTests.Cli.csproj", Destination = "src/HandlerTests.Cli", MainProject = true },
            new { Name = "HandlerTests.Cli.Tests.csproj", Destination = "test/HandlerTests.Cli.Tests", MainProject = false }
        };

        var projectFile = "";
        var testProjectFile = "";

        foreach (var file in filesToPlace)
        {
            Directory.CreateDirectory(Path.Combine(targetPath, file.Destination));
            var fullPath = Path.Combine(targetPath, file.Destination, file.Name);

            projectFile = file.MainProject ? fullPath : projectFile;
            testProjectFile = !file.MainProject ? fullPath : testProjectFile;

            using var stream = GetEmbeddedFileStream(file.Name);            
            using var outputStream = File.Create(fullPath);

            await stream.CopyToAsync(outputStream);
        }

        return (projectFile, testProjectFile);
    }

    private static Stream GetEmbeddedFileStream(string name)
    {
        return new EmbeddedFileProvider(typeof(DefaultHandlerTests).Assembly)
            .GetDirectoryContents("/")
            .Where(f => f.Name.EndsWith($".{name}"))
            .Single()
            .CreateReadStream();
    }

    private static async Task<string> GetEmbeddedFileTextAsync(string name) 
    {
        using var stream = GetEmbeddedFileStream(name);

        return await stream.ReadAllTextAsync();
    }
}