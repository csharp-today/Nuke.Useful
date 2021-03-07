using Nuke.Common;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.ProjectModel;

namespace Nuke.Useful.Builds
{
    public class SimpleBuild : SolutionBuild
    {
        [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
        public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

        [GitVersion] public readonly GitVersion GitVersion;
        public AbsolutePath OutputDirectory => RootDirectory / "output";

        protected string Runtime { get; set; }

        protected Target Clean => _ => _
            .DependsOn(SwitchSolution)
            .Executes(RunCleanTarget);

        protected Target Restore => _ => _
            .DependsOn(Clean)
            .Executes(() => RunRestoreTarget());

        protected Target Compile => _ => _
            .DependsOn(Restore)
            .Executes(() => RunCompileTarget());

        protected Target Test => _ => _
            .DependsOn(Compile)
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
            project ??= Project;
            var settings = s.SetProjectFile(project?.ToString() ?? Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.GetNormalizedAssemblyVersion())
                .SetFileVersion(GitVersion.GetNormalizedFileVersion())
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore();
            if (!string.IsNullOrWhiteSpace(Runtime))
            {
                settings = settings.SetRuntime(Runtime);
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
