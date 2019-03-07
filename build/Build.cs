using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.PushPreRelease);

    public Build() => Packer = new NuGetPacker(this);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter] readonly string FeedUser;
    [Parameter] readonly string FeedSecret;

    [GitVersion] public readonly GitVersion GitVersion;
    public AbsolutePath SourceDirectory => RootDirectory / "source";
    public AbsolutePath OutputDirectory => RootDirectory / "output";

    [GitRepository] readonly GitRepository GitRepository;
    [Solution] readonly Solution Solution;
    readonly NuGetPacker Packer;

    Target Clean => _ => _
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .DependsOn(Clean)
        .Executes(() =>
        {
            DotNetRestore(s => s.SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.GetNormalizedAssemblyVersion())
                .SetFileVersion(GitVersion.GetNormalizedFileVersion())
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });

    Target PackPreRelease => _ => _
        .DependsOn(Compile)
        .Executes(() => DotNetPack(s => Packer.ConfigureForPreRelease(s)));

    Target PackProduction => _ => _
        .DependsOn(PackPreRelease)
        .Executes(() => DotNetPack(s => Packer.ConfigureForProduction(s)));

    Target PushPreRelease => _ => _
        .DependsOn(PackProduction)
        .Requires(() => FeedUser)
        .Requires(() => FeedSecret)
        .Executes(() =>
        {
            using var config = NuGetConfig.Create(Packer.PreReleaseOutput, FeedUser, FeedSecret);
            var pkg = GlobFiles(OutputDirectory, "*.nupkg").Single();
            DotNetNuGetPush(s => s
                .SetTargetPath(pkg)
                .SetWorkingDirectory(OutputDirectory)
                .SetForceEnglishOutput(true)
                .SetSource(config.FeedName)
                .SetApiKey("NuGet requires the key but Azure DevOps ignores it"));
        });
}
