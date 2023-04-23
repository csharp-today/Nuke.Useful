using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
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
            return settings
                .EnableNoBuild()
                .EnableNoRestore()
                .SetConfiguration(Build.Configuration)
                .SetProject(Build.ProjectPath)
                .SetVersion(version);
        }
    }
}
