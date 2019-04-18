using Nuke.Common.Execution;
using Nuke.Useful.Builds;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : AzureDevOpsLibraryBuild
{
    protected override string ArtifactOutputDirectory { get; }
    protected override string FeedSecret { get; }
    protected override string FeedUrl => "https://pkgs.dev.azure.com/mariuszbojkowski/_packaging/OpenSourceTest/nuget/v3/index.json";
    protected override string FeedUser { get; }

    public static int Main() => Execute<Build>(x => x.BuildAzureDevOpsLibrary);
}
