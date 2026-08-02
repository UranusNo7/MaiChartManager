using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace MaiChartManager.Services;

public enum ResourceJunctionStatus
{
    Ready,
    Created,
    AlreadyLinked,
    Removed,
    SourceMissing,
    TargetRootMissing,
    Conflict,
    WrongTarget,
    Failed,
    Unsupported,
}

public record ResourceJunctionItem(
    string Name,
    string Source,
    string Target,
    ResourceJunctionStatus Status,
    string? Detail = null);

public class ResourceJunctionService
{
    public const string SourceRoot = @"F:\Game\maimai DX SDEZ 1.66\Package\Sinmai_Data\StreamingAssets\A000";
    public const string TargetRoot = @"F:\Game\maimai DX SDGB 1.56\Package\Sinmai_Data\StreamingAssets\A000";

    public static readonly string[] ResourceNames = ["AssetBundleImages", "MovieData", "SoundData"];

    private const uint IoReparseTagMountPoint = 0xA0000003;
    private readonly string sourceRoot;
    private readonly string targetRoot;

    public ResourceJunctionService() : this(SourceRoot, TargetRoot)
    {
    }

    public ResourceJunctionService(string sourceRoot, string targetRoot)
    {
        this.sourceRoot = Path.GetFullPath(sourceRoot);
        this.targetRoot = Path.GetFullPath(targetRoot);
    }

    public IReadOnlyList<ResourceJunctionItem> Inspect()
    {
        return ResourceNames.Select(Inspect).ToArray();
    }

    public IReadOnlyList<ResourceJunctionItem> CreateLinks()
    {
        return ResourceNames.Select(name =>
        {
            var item = Inspect(name);
            if (item.Status != ResourceJunctionStatus.Ready) return item;

            try
            {
                CreateJunction(item.Source, item.Target);
                var verified = Inspect(name);
                return verified.Status == ResourceJunctionStatus.AlreadyLinked
                    ? verified with { Status = ResourceJunctionStatus.Created }
                    : verified with { Status = ResourceJunctionStatus.Failed, Detail = "Junction was created but verification failed." };
            }
            catch (Exception e)
            {
                return item with { Status = ResourceJunctionStatus.Failed, Detail = e.Message };
            }
        }).ToArray();
    }

    public IReadOnlyList<ResourceJunctionItem> RemoveLinks()
    {
        return ResourceNames.Select(name =>
        {
            var item = Inspect(name);
            if (item.Status != ResourceJunctionStatus.AlreadyLinked) return item;

            try
            {
                Directory.Delete(item.Target, false);
                var verified = Inspect(name);
                return verified.Status == ResourceJunctionStatus.Ready
                    ? verified with { Status = ResourceJunctionStatus.Removed }
                    : verified with { Status = ResourceJunctionStatus.Failed, Detail = "Junction removal could not be verified." };
            }
            catch (Exception e)
            {
                return item with { Status = ResourceJunctionStatus.Failed, Detail = e.Message };
            }
        }).ToArray();
    }

    private ResourceJunctionItem Inspect(string name)
    {
        var source = Path.Combine(sourceRoot, name);
        var target = Path.Combine(targetRoot, name);

        if (!OperatingSystem.IsWindows())
            return new(name, source, target, ResourceJunctionStatus.Unsupported, "Junctions are only supported on Windows.");
        if (!Directory.Exists(source))
            return new(name, source, target, ResourceJunctionStatus.SourceMissing);
        if (!Directory.Exists(targetRoot))
            return new(name, source, target, ResourceJunctionStatus.TargetRootMissing);

        var entry = FindTargetEntry(name);
        if (entry is null)
            return new(name, source, target, ResourceJunctionStatus.Ready);
        if (!TryGetReparseTag(target, out var tag) || tag != IoReparseTagMountPoint)
            return new(name, source, target, ResourceJunctionStatus.Conflict, "The target exists and is not a Junction.");

        try
        {
            var destination = entry.ResolveLinkTarget(false)?.FullName;
            if (destination is not null && SamePath(destination, source))
                return new(name, source, target, ResourceJunctionStatus.AlreadyLinked);
            return new(name, source, target, ResourceJunctionStatus.WrongTarget, destination);
        }
        catch (Exception e)
        {
            return new(name, source, target, ResourceJunctionStatus.WrongTarget, e.Message);
        }
    }

    private FileSystemInfo? FindTargetEntry(string name)
    {
        return new DirectoryInfo(targetRoot)
            .EnumerateFileSystemInfos(name, SearchOption.TopDirectoryOnly)
            .FirstOrDefault(entry => string.Equals(entry.Name, name, StringComparison.OrdinalIgnoreCase));
    }

    private static bool SamePath(string left, string right)
    {
        return string.Equals(NormalizePath(left), NormalizePath(right), StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        if (path.StartsWith(@"\\?\UNC\", StringComparison.OrdinalIgnoreCase))
            path = @"\\" + path[8..];
        else if (path.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase))
            path = path[4..];
        return Path.TrimEndingDirectorySeparator(Path.GetFullPath(path));
    }

    private static void CreateJunction(string source, string target)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        startInfo.ArgumentList.Add("/d");
        startInfo.ArgumentList.Add("/c");
        startInfo.ArgumentList.Add("mklink");
        startInfo.ArgumentList.Add("/J");
        startInfo.ArgumentList.Add(target);
        startInfo.ArgumentList.Add(source);

        using var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start mklink.");
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
            throw new IOException((error.Length > 0 ? error : output).Trim());
    }

    private static bool TryGetReparseTag(string path, out uint tag)
    {
        tag = 0;
        using var handle = CreateFile(
            path,
            0,
            0x00000001 | 0x00000002 | 0x00000004,
            IntPtr.Zero,
            3,
            0x00200000 | 0x02000000,
            IntPtr.Zero);
        if (handle.IsInvalid) return false;

        if (!GetFileInformationByHandleEx(handle, 9, out var info, (uint)Marshal.SizeOf<FileAttributeTagInfo>()))
            return false;
        tag = info.ReparseTag;
        return true;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct FileAttributeTagInfo
    {
        public uint FileAttributes;
        public uint ReparseTag;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFile(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetFileInformationByHandleEx(
        SafeFileHandle file,
        int fileInformationClass,
        out FileAttributeTagInfo fileInformation,
        uint bufferSize);
}
