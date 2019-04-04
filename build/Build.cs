using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Nuke.Useful;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : SimpleBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.SaveArtifacts);

    public Build() => Packer = new NuGetPacker(this);

    [AzureVariable("BUILD_ARTIFACTSTAGINGDIRECTORY")] readonly string ArtifactOutputDirectory;
    [AzureVariable] readonly string FeedUser;
    [AzureVariable] readonly string FeedSecret;

    [GitRepository] readonly GitRepository GitRepository;
    readonly NuGetPacker Packer;

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
            using var config = NuGetConfig.Create(Packer.PreReleaseOutput, "https://pkgs.dev.azure.com/mariuszbojkowski/_packaging/OpenSourceTest/nuget/v3/index.json", FeedUser, FeedSecret);
            var pkg = GlobFiles(Packer.PreReleaseOutput, "*.nupkg").Single();
            DotNetNuGetPush(s => s
                .SetTargetPath(pkg)
                .SetWorkingDirectory(Packer.PreReleaseOutput)
                .SetForceEnglishOutput(true)
                .SetSource(config.FeedName)
                .SetApiKey("NuGet requires the key but Azure DevOps ignores it"));
        });

    Target SaveArtifacts => _ => _
        .DependsOn(PushPreRelease)
        .Requires(() => ArtifactOutputDirectory)
        .Executes(() => ArtifactStorage.Create(ArtifactOutputDirectory)
            .AddFile(Directory.GetFiles(Packer.ProductionOutput, "*.nupkg").Single()));
}
