using Microsoft.Build.Evaluation;
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
        protected AbsolutePath PublishOutput { get; private set; }
        protected abstract string PublishOutputDirectoryName { get; }

        protected Target PublishWeb => _ => _
            .DependsOn(Test)
            .Executes(() => RunPublishWebTarget());

        protected Target SaveWebArtifacts => _ => _
            .DependsOn(PublishWeb)
            .Requires(() => ArtifactOutputDirectory)
            .Executes(RunSaveWebArtifactsTarget);

        protected Target BuildWebApp => _ => _.DependsOn(SaveWebArtifacts);

        protected void RunPublishWebTarget(Project project = null)
        {
            PublishOutput = OutputDirectory / PublishOutputDirectoryName;
            EnsureExistingDirectory(PublishOutput);
            DotNetPublish(p =>
            {
                var settings = p.SetWorkingDirectory(project?.DirectoryPath ?? SourceDirectory)
                    .SetConfiguration(Configuration)
                    .EnableNoBuild()
                    .SetOutput(PublishOutput);
                if (!string.IsNullOrWhiteSpace(Runtime))
                {
                    settings = settings.SetRuntime(Runtime);
                }
                return settings;
            });
        }

        protected void RunSaveWebArtifactsTarget()
        {
            if (PublishOutput is null)
            {
                throw new ArgumentNullException(nameof(PublishOutput));
            }

            ArtifactStorage.Create(ArtifactOutputDirectory).AddDirectory(PublishOutput);
        }
    }
}
