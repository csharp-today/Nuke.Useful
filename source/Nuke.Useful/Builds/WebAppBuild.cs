using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Useful.Attributes;
using System;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

namespace Nuke.Useful.Builds
{
    public abstract class WebAppBuild : SimpleBuild
    {
        [ArtifactDirectoryAzureVariable] protected string ArtifactOutputDirectory { get; }
        protected AbsolutePath PublishOutput { get; private set; }
        protected abstract string PublishOutputDirectoryName { get; }

        protected override Target RunAllSteps => _ => _
            .DependsOn(Step_7_SaveWebArtifacts)
            .Executes(DoNothingAction);

        protected Target Step_6_PublishWeb => _ => _
            .DependsOn(Step_5_RunTests)
            .Executes(() => RunPublishWebTarget());

        protected Target Step_7_SaveWebArtifacts => _ => _
            .DependsOn(Step_6_PublishWeb)
            .Requires(() => ArtifactOutputDirectory)
            .Executes(RunSaveWebArtifactsTarget);

        protected Target BuildWebApp => _ => _.DependsOn(Step_7_SaveWebArtifacts);

        protected void RunPublishWebTarget(Project project = null)
        {
            project ??= Project;
            PublishOutput = OutputDirectory / PublishOutputDirectoryName;
            EnsureExistingDirectory(PublishOutput);
            DotNetPublish(p =>
            {
                var settings = p
                    .SetProject(project)
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
