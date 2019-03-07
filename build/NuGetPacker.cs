﻿using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using System.Linq;
using static Nuke.Common.IO.PathConstruction;

class NuGetPacker
{
    public AbsolutePath PreReleaseOutput => Build.OutputDirectory / "preRelease";
    public AbsolutePath ProductionOutput => Build.OutputDirectory / "production";

    private Build Build { get; }
    public NuGetPacker(Build build) => Build = build;

    public DotNetPackSettings ConfigureForPreRelease(DotNetPackSettings settings) =>
        CommonConfiguration(settings)
        .SetOutputDirectory(PreReleaseOutput)
        .SetVersion(Build.GitVersion.NuGetVersionV2);

    public DotNetPackSettings ConfigureForProduction(DotNetPackSettings settings) =>
        CommonConfiguration(settings)
        .SetOutputDirectory(ProductionOutput)
        .SetVersion(Build.GitVersion.NuGetVersionV2.Split('-').First());

    private DotNetPackSettings CommonConfiguration(DotNetPackSettings settings) => settings
            .EnableNoBuild()
            .SetConfiguration(Build.Configuration)
            .SetWorkingDirectory(Build.SourceDirectory);
}
