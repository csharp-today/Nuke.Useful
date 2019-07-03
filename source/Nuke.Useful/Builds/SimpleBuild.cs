﻿using Nuke.Common;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.ProjectModel;

namespace Nuke.Useful.Builds
{
    public class SimpleBuild : NukeBuild
    {
        [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
        public readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

        [GitVersion] public readonly GitVersion GitVersion;
        public AbsolutePath SourceDirectory => RootDirectory / "source";
        public AbsolutePath OutputDirectory => RootDirectory / "output";

        [Solution] protected readonly Solution Solution;

        protected Target Clean => _ => _
            .Executes(() =>
            {
                SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
                EnsureCleanDirectory(OutputDirectory);
            });

        protected Target Restore => _ => _
            .DependsOn(Clean)
            .Executes(() => DotNetRestore(s => s.SetProjectFile(Solution)));

        protected Target Compile => _ => _
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

        protected Target Test => _ => _
            .DependsOn(Compile)
            .Executes(() =>
            {
                DotNetTest(s => s.SetProjectFile(Solution));
            });

        protected void CopyNukeTo(string destination)
        {
            ArtifactStorage.Create(destination)
                .AddDirectory(RootDirectory / "build")
                .AddFile(RootDirectory / "build.ps1");
        }
    }
}