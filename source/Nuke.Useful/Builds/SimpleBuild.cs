using Nuke.Common;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.ProjectModel;
using Nuke.Common.IO;
using System;

namespace Nuke.Useful.Builds
{
    public class SimpleBuild : SolutionBuild
    {
        [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
        public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

        [GitVersion] public readonly GitVersion GitVersion;

        public AbsolutePath OutputDirectory => RootDirectory / "output";

        public string Platform { get; set; }
        public string Runtime { get; set; }

        protected override Target RunAllSteps => _ => _
            .DependsOn(Step_5_RunTests)
            .Executes(DoNothingAction);

        protected Target Step_2_Clean => _ => _
            .DependsOn(Step_1_SwitchSolution)
            .Executes(RunCleanTarget);

        protected Target Step_3_Restore => _ => _
            .DependsOn(Step_2_Clean)
            .Executes(() => RunRestoreTarget());

        protected Target Step_4_Compile => _ => _
            .DependsOn(Step_3_Restore)
            .Executes(() => RunCompileTarget());

        protected Target Step_5_RunTests => _ => _
            .DependsOn(Step_4_Compile)
            .Executes(() => RunTestTarget());

        protected void CopyNukeTo(string destination)
        {
            ArtifactStorage.Create(destination)
                .AddDirectory(RootDirectory / "build")
                .AddFile(RootDirectory / "build.ps1");
        }

        protected void RunCleanTarget()
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
        }

        protected void RunRestoreTarget(Project project = null) => DotNetRestore(s =>
        {
            project ??= Project;
            var settings = s.SetProjectFile(project?.ToString() ?? Solution);
            if (!string.IsNullOrWhiteSpace(Runtime))
            {
                settings = settings.SetRuntime(Runtime);
            }
            return settings;
        });

        protected void RunCompileTarget(Project project = null, string outputDirectory = null) => DotNetBuild(s =>
        {
            Console.WriteLine($"{nameof(Configuration)} = {Configuration}");

            project ??= Project;
            var settings = s.SetProjectFile(project?.ToString() ?? Solution)
                .SetConfiguration(Configuration)
                .EnableNoRestore();

            if (GitVersion is null)
            {
                Console.WriteLine($"{nameof(GitVersion)} is not supported (null)");
            }
            else
            {
                settings = settings
                    .SetAssemblyVersion(GitVersion.AssemblySemVer)
                    .SetFileVersion(GitVersion.AssemblySemFileVer)
                    .SetInformationalVersion(GitVersion.InformationalVersion);
            }

            if (!string.IsNullOrWhiteSpace(Runtime))
            {
                settings = settings.SetRuntime(Runtime);
            }

            if (!string.IsNullOrWhiteSpace(Platform))
            {
                settings = settings.SetPlatform(Platform);
            }

            if (!string.IsNullOrWhiteSpace(outputDirectory))
            {
                settings = settings.SetOutputDirectory(outputDirectory);
            }

            return settings;
        });

        protected void RunTestTarget(Project project = null) => DotNetTest(s =>
        {
            var settings = s.SetProjectFile(project?.ToString() ?? Solution);
            if (!string.IsNullOrWhiteSpace(Runtime))
            {
                settings = settings.SetRuntime(Runtime);
            }
            return settings;
        });
    }
}
