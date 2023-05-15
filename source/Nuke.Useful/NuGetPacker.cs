using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Tools.MSBuild;
using Nuke.Useful.Builds;
using System;
using System.Linq;

namespace Nuke.Useful
{
    public class NuGetPacker
    {
        public AbsolutePath PreReleaseOutput => Build.OutputDirectory / "preRelease";
        public AbsolutePath ProductionOutput => Build.OutputDirectory / "production";

        private SimpleBuild Build { get; }

        public NuGetPacker(SimpleBuild build) => Build = build;

        public DotNetPackSettings ConfigureForPreRelease(DotNetPackSettings settings) =>
            CommonConfiguration(
                settings,
                Build.GitVersion.NuGetVersionV2)
            .SetOutputDirectory(PreReleaseOutput);

        public DotNetPackSettings ConfigureForProduction(DotNetPackSettings settings) =>
            CommonConfiguration(
                settings,
                Build.GitVersion.NuGetVersionV2.Split('-').First())
            .SetOutputDirectory(ProductionOutput);

        private DotNetPackSettings CommonConfiguration(DotNetPackSettings settings, string version)
        {
            Console.WriteLine($"Version = {version}");
            settings = settings
                .EnableNoBuild()
                .EnableNoRestore()
                .SetConfiguration(Build.Configuration)
                .SetProject(Build.ProjectPath)
                .SetVersion(version);

            if (!string.IsNullOrWhiteSpace(Build.Runtime))
            {
                settings = settings.SetRuntime(Build.Runtime);
            }

            if (!string.IsNullOrWhiteSpace(Build.Platform))
            {
                settings = settings.SetPlatform(Build.Platform);
            }

            return settings;
        }
    }
}
