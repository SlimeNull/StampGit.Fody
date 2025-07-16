
using SourceControlSummary;

internal class Program
{
    [GitCommit]
    static string? CommitID { get; }

    [GitBranch]
    static string? Branch { get; }

    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        Console.WriteLine($"Git info, CommitID: {CommitID}, Branch: {Branch}");
    }
}