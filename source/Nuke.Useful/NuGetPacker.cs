using Nuke.Common.IO;
using Nuke.Common.Tools.DotNet;
using Nuke.Useful.Builds;
using System.Linq;

namespace Nuke.Useful
{
    public class NuGetPacker
    {
        public AbsolutePath PreReleaseOutput => Build.OutputDirectory / "preRelease";
        public AbsolutePath ProductionOutput => Build.OutputDirectory / "production";

        private SimpleBuild Build { get; }

        public NuGetPacker(SimpleBuild build) => Build = build;

        public DotNetPackSettings ConfigureForPreRelease(DotNetPackSettings settings) => CommonConfiguration(settings)
            .SetOutputDirectory(PreReleaseOutput)
            .SetVersion(Build.GitVersion.NuGetVersionV2);

        public DotNetPackSettings ConfigureForProduction(DotNetPackSettings settings) => CommonConfiguration(settings)
            .SetOutputDirectory(ProductionOutput)
            .SetVersion(Build.GitVersion.NuGetVersionV2.Split('-').First());

        private DotNetPackSettings CommonConfiguration(DotNetPackSettings settings) => settings
            .EnableNoBuild()
            .SetConfiguration(Build.Configuration);
    }
}
