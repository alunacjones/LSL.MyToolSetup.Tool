using CommandLine;
using CommandLineParser.DependencyInjection.Interfaces;
using Diamond.Core.System.TemporaryFolder;
using LSL.AbstractConsole;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace LSL.MyToolSetup.Tool.Cli.Tests.Handlers;

public class DefaultHandlingTests : BaseCliTest
{
    [TestCase(new string[] { "--help" }, null)]
    //[TestCase(new string[] { "help" }, null)]
    public async Task GivenACallWithTheHelpOption_ItShouldOutputTheHelpText(string[] args, string dummy)
    {
        // Arrange
        var sut = BuildTestHostRunner(args);

        // Act
        var (result, output) = await sut();

        // Assert
        using var _ = new AssertionScope();

        result.Should().Be(0);
        output.Should().Contain("help");
    }

    [TestCase(new string[] { "--version" }, null)]
    //[TestCase(new string[] { "version" }, null)]
    public async Task GivenACallWithTheVersionOption_ItShouldOutputTheVersionText(string[] args, string dummy)
    {        
        // Arrange
        var sut = BuildTestHostRunner(args);

        // Act
        var (result, output) = await sut();

        // Assert
        using var _ = new AssertionScope();

        result.Should().Be(0);
        output.Should().MatchRegex(@"\d+\.\d+\.\d+");
    }

    [TestCase(new string[] { "test", "--verbose" }, 
        """
        a message with more
        and again
        [CRT] log
        [DBG] log
        [ERR] log
        [INF] log
        [WRN] log

        """)
    ]
    [TestCase(new string[] { "test" }, 
        """
        a message with more
        and again

        """)
    ]    
    public async Task GivenACallWithATestVerb_ItShouldOutputTheTheExpectedResult(string[] args, string expectedOutput)
    {
        // Arrange
        var sut = BuildTestHostRunner(
            args,
            s =>
            {
                s.AddSingleton<ICommandLineOptions, Test>();
                s.AddSingleton<IExecuteCommandLineOptionsAsync<Test, int>, TestHandler>();
            });

        // Act
        var (result, output) = await sut();

        // Assert
        using var _ = new AssertionScope();

        result.Should().Be(0);
        output.Should().Be(expectedOutput.ReplaceLineEndings());
    }    

    [Test]
    public async Task GivenACallThatThrows_ItShouldOutputTheTheExpectedResult()
    {
        // Arrange
        var sut = BuildTestHostRunner(
            ["exec"],
            s =>
            {
                var mock = Substitute.For<ICommandLineParser<int>>();
                mock.ParseArgumentsAsync(Arg.Any<string[]>(), Arg.Any<Action<ParserSettings>>()).ThrowsAsync<ArgumentOutOfRangeException>();
                s.RemoveAll(typeof(ICommandLineParser<>));
                s.RemoveAll<ICommandLineOptions>();

                s.AddSingleton(mock);
            });

        // Act
        var (result, output) = await sut();

        // Assert
        using var _ = new AssertionScope();

        result.Should().Be(1);
        output.Should().StartWith("It looks like this project has not yet added any Options or Handlers for the CLI");
    }

    [Verb("test")]
    public class Test : ICommandLineOptions
    {
        [Option('n', "name")]
        public string Name { get; set; } = default!;
    }

    public class TestHandler(ILogger<TestHandler> logger, IConsole console) : IExecuteCommandLineOptionsAsync<Test, int>
    {
        public Task<int> ExecuteAsync(Test options)
        {
            console.Write("a")
                .Write(" {0}", "message")
                .WriteLine(" with {0}", "more")
                .WriteLine("and again");

            logger.LogCritical("log");
            logger.LogDebug("log");
            logger.LogError("log");
            logger.LogInformation("log");
            logger.LogTrace("log");
            logger.LogWarning("log");

            return Task.FromResult(0);
        }
    }    
}