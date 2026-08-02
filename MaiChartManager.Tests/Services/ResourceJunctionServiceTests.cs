using MaiChartManager.Services;

namespace MaiChartManager.Tests.Services;

public sealed class ResourceJunctionServiceTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), $"mcm-resource-links-{Guid.NewGuid():N}");
    private readonly string sourceRoot;
    private readonly string targetRoot;

    public ResourceJunctionServiceTests()
    {
        sourceRoot = Path.Combine(root, "source");
        targetRoot = Path.Combine(root, "target");
        Directory.CreateDirectory(sourceRoot);
        Directory.CreateDirectory(targetRoot);
        foreach (var name in ResourceJunctionService.ResourceNames)
            Directory.CreateDirectory(Path.Combine(sourceRoot, name));
    }

    [Fact]
    public void FixedScopeContainsOnlyThreeResourceDirectories()
    {
        Assert.Equal(["AssetBundleImages", "MovieData", "SoundData"], ResourceJunctionService.ResourceNames);
    }

    [Fact]
    public void ExistingRealDirectoriesAreConflicts()
    {
        if (!OperatingSystem.IsWindows()) return;
        foreach (var name in ResourceJunctionService.ResourceNames)
            Directory.CreateDirectory(Path.Combine(targetRoot, name));

        var result = new ResourceJunctionService(sourceRoot, targetRoot).Inspect();

        Assert.All(result, item => Assert.Equal(ResourceJunctionStatus.Conflict, item.Status));
    }

    [Fact]
    public void CreateAndRemoveOnlyVerifiedJunctions()
    {
        if (!OperatingSystem.IsWindows()) return;
        var sourceFile = Path.Combine(sourceRoot, ResourceJunctionService.ResourceNames[0], "source.txt");
        File.WriteAllText(sourceFile, "source remains unchanged");
        var service = new ResourceJunctionService(sourceRoot, targetRoot);

        var created = service.CreateLinks();
        var inspected = service.Inspect();
        var removed = service.RemoveLinks();

        Assert.All(created, item => Assert.Equal(ResourceJunctionStatus.Created, item.Status));
        Assert.All(inspected, item => Assert.Equal(ResourceJunctionStatus.AlreadyLinked, item.Status));
        Assert.All(removed, item => Assert.Equal(ResourceJunctionStatus.Removed, item.Status));
        Assert.True(File.Exists(sourceFile));
        Assert.Equal("source remains unchanged", File.ReadAllText(sourceFile));
    }

    [Fact]
    public void WrongJunctionTargetIsNotRemoved()
    {
        if (!OperatingSystem.IsWindows()) return;
        var wrongSource = Path.Combine(root, "wrong-source");
        Directory.CreateDirectory(wrongSource);
        var target = Path.Combine(targetRoot, ResourceJunctionService.ResourceNames[0]);
        CreateJunction(wrongSource, target);
        var service = new ResourceJunctionService(sourceRoot, targetRoot);

        var result = service.RemoveLinks();

        Assert.Equal(ResourceJunctionStatus.WrongTarget, result[0].Status);
        Assert.True(Directory.Exists(target));
    }

    public void Dispose()
    {
        if (!Directory.Exists(root)) return;
        var service = new ResourceJunctionService(sourceRoot, targetRoot);
        service.RemoveLinks();
        var wrongTarget = Path.Combine(targetRoot, ResourceJunctionService.ResourceNames[0]);
        if (Directory.Exists(wrongTarget) && (File.GetAttributes(wrongTarget) & FileAttributes.ReparsePoint) != 0)
            Directory.Delete(wrongTarget, false);
        Directory.Delete(root, true);
    }

    private static void CreateJunction(string source, string target)
    {
        var startInfo = new System.Diagnostics.ProcessStartInfo
        {
            FileName = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add("/d");
        startInfo.ArgumentList.Add("/c");
        startInfo.ArgumentList.Add("mklink");
        startInfo.ArgumentList.Add("/J");
        startInfo.ArgumentList.Add(target);
        startInfo.ArgumentList.Add(source);
        using var process = System.Diagnostics.Process.Start(startInfo)!;
        process.WaitForExit();
        Assert.Equal(0, process.ExitCode);
    }
}
