using Nuke.Common;
using Nuke.Common.Tools.DotNet;
using Nuke.Useful.Attributes;
using System.IO;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Nuke.Useful.Builds
{
    public class AzureDevOpsLibraryBuild : SimpleBuild
    {
        [ArtifactDirectoryAzureVariable] protected string ArtifactOutputDirectory { get; }
        [AzureVariable] protected string FeedSecret { get; }
        [AzureVariable] protected string FeedUrl { get; }
        [AzureVariable] protected string FeedUser { get; }

        protected readonly NuGetPacker Packer;

        protected override Target RunAllSteps => _ => _
            .DependsOn(Step_9_SaveArtifacts)
            .Executes(DoNothingAction);

        protected Target Step_6_PackPreRelease => _ => _
            .DependsOn(Step_5_RunTests)
            .Executes(() => DotNetPack(s => Packer.ConfigureForPreRelease(s)));

        protected Target Step_7_PackProduction => _ => _
            .DependsOn(Step_6_PackPreRelease)
            .Executes(() => DotNetPack(s => Packer.ConfigureForProduction(s)));

        protected Target Step_8_PushPreRelease => _ => _
            .DependsOn(Step_7_PackProduction)
            .Requires(() => FeedUser)
            .Requires(() => FeedSecret)
            .Executes(() =>
            {
                var startingDirectory = Directory.GetCurrentDirectory();
                try
                {
                    Directory.SetCurrentDirectory(Packer.PreReleaseOutput);
                    using var config = NuGetConfig.Create(Packer.PreReleaseOutput, FeedUrl, FeedUser, FeedSecret);
                    var packages = GlobFiles(Packer.PreReleaseOutput, "*.nupkg");
                    foreach (var pkg in packages)
                    {
                        DotNetNuGetPush(s => s
                            .SetTargetPath(pkg)
                            .SetSource(config.FeedName)
                            .SetApiKey("NuGet requires the key but Azure DevOps ignores it"));
                    }
                }
                finally
                {
                    Directory.SetCurrentDirectory(startingDirectory);
                }
            });

        protected Target Step_9_SaveArtifacts => _ => _
            .DependsOn(Step_8_PushPreRelease)
            .Requires(() => ArtifactOutputDirectory)
            .Executes(() => ArtifactStorage
                .Create(ArtifactOutputDirectory)
                .AddFiles(Directory.GetFiles(Packer.ProductionOutput, "*.nupkg")));

        public AzureDevOpsLibraryBuild() => Packer = new NuGetPacker(this);
    }
}
