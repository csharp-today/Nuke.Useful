using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;

class NuGetPacker
{
    private Build Build { get; }
    public NuGetPacker(Build build) => Build = build;

    public DotNetPackSettings ConfigureForPreRelease(DotNetPackSettings settings)
    {
        return settings
            .EnableNoBuild()
            .SetConfiguration(Build.Configuration)
            .SetWorkingDirectory(Build.SourceDirectory)
            .SetOutputDirectory(Build.OutputDirectory)
            .SetVersion(Build.GitVersion.NuGetVersionV2);
    }
}
