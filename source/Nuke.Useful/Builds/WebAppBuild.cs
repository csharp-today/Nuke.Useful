using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Useful.Attributes;
using System;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Nuke.Useful.Builds
{
    public abstract class WebAppBuild : SimpleBuild
    {
        [ArtifactDirectoryAzureVariable] protected string ArtifactOutputDirectory { get; }

        protected abstract string PublishOutputDirectoryName { get; }

        AbsolutePath PublishOutput;
        protected Target PublishWeb => _ => _
            .DependsOn(Compile)
            .Executes(() =>
            {
                PublishOutput = OutputDirectory / PublishOutputDirectoryName;
                EnsureExistingDirectory(PublishOutput);
                DotNetPublish(p => p
                    .SetWorkingDirectory(SourceDirectory)
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .SetOutput(PublishOutput));
            });

        protected Target SaveWebArtifacts => _ => _
            .DependsOn(PublishWeb)
            .Requires(() => ArtifactOutputDirectory)
            .Executes(() =>
            {
                if (PublishOutput is null)
                {
                    throw new ArgumentNullException(nameof(PublishOutput));
                }

                ArtifactStorage.Create(ArtifactOutputDirectory).AddDirectory(PublishOutput);
            });
    }
}
