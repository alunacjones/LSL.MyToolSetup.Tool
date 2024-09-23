namespace LSL.MyToolSetup.Tool.Cli.Infrastructure;

/// <summary>
/// Provides a helper to consume the <c>--verbose</c> flag external to <c>CommandLineParser</c>
/// </summary>
/// <remarks>This is used as we need to setup logging more eagerly than parsing the command line options</remarks>
public static class ArgumentsPreprocessor
{
    /// <summary>
    /// Consumes the --verbose flag if present in the command line arguments
    /// </summary>
    /// <remarks>This is used as we need to setup logging more eagerly than parsing the command line options</remarks>
    /// <param name="IsVerbose"></param>
    /// <param name="args"></param>
    /// <returns>A flag indicating that logging should be enabled and a filtered set of the arguments (removes --verbose if it was present)</returns>
    public static (bool IsVerbose, string[] FilteredArguments) ProcessArguments(string[] args)
    {       
        var filteredResult = args.Aggregate(
            new { IsVerbose = false, FilteredArgs = new List<string>() },
            (agg, i) =>
            {
                if (i == "--verbose")
                {
                    return new 
                    {
                        IsVerbose = true,
                        agg.FilteredArgs,
                    };
                }

                agg.FilteredArgs.Add(i);

                return agg;
            });

        return (filteredResult.IsVerbose, filteredResult.FilteredArgs.ToArray());
    }
}
