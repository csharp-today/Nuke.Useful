﻿using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Useful.Attributes;
using System.IO;
using System.Linq;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Nuke.Useful.Builds
{
    public abstract class AzureDevOpsLibraryBuild : SimpleBuild
    {
        [ArtifactDirectoryAzureVariable] protected abstract string ArtifactOutputDirectory { get; }
        [AzureVariable] protected abstract string FeedSecret { get; }
        [AzureVariable] protected abstract string FeedUrl { get; }
        [AzureVariable] protected abstract string FeedUser { get; }

        protected readonly NuGetPacker Packer;

        public AzureDevOpsLibraryBuild() => Packer = new NuGetPacker(this);

        protected Target PackPreRelease => _ => _
            .DependsOn(Compile)
            .Executes(() => DotNetPack(s => Packer.ConfigureForPreRelease(s)));

        protected Target PackProduction => _ => _
            .DependsOn(PackPreRelease)
            .Executes(() => DotNetPack(s => Packer.ConfigureForProduction(s)));

        protected Target PushPreRelease => _ => _
            .DependsOn(PackProduction)
            .Requires(() => FeedUser)
            .Requires(() => FeedSecret)
            .Executes(() =>
            {
                using var config = NuGetConfig.Create(Packer.PreReleaseOutput, FeedUrl, FeedUser, FeedSecret);
                var pkg = GlobFiles(Packer.PreReleaseOutput, "*.nupkg").Single();
                DotNetNuGetPush(s => s
                    .SetTargetPath(pkg)
                    .SetWorkingDirectory(Packer.PreReleaseOutput)
                    .SetForceEnglishOutput(true)
                    .SetSource(config.FeedName)
                    .SetApiKey("NuGet requires the key but Azure DevOps ignores it"));
            });

        protected Target SaveArtifacts => _ => _
            .DependsOn(PushPreRelease)
            .Requires(() => ArtifactOutputDirectory)
            .Executes(() => ArtifactStorage.Create(ArtifactOutputDirectory)
                .AddFile(Directory.GetFiles(Packer.ProductionOutput, "*.nupkg").Single()));

        protected Target BuildAzureDevOpsLibrary => _ => _
            .DependsOn(SaveArtifacts);
    }
}