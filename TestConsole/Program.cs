
using MetaStamp;

internal class Program
{
    [GitCommitID]
    static string? CommitID { get; }

    [GitBranch]
    static string? Branch { get; }

    [BuildDateTime]
    static string? BuildDateTime { get; }

    [BuildPlatformID]
    static PlatformID PlatformID { get; }

    [BuildOperationSystem]
    static string? BuildOS { get; }

    private static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        Console.WriteLine($"Git info, CommitID: {CommitID}, Branch: {Branch}");
        Console.WriteLine($"Build DateTime, {BuildDateTime}");
        Console.WriteLine($"Platform, {PlatformID}");
        Console.WriteLine($"OSVersion, {BuildOS}");
    }
}