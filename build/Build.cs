using Nuke.Common.Execution;
using Nuke.Useful.Builds;

[UnsetVisualStudioEnvironmentVariables]
internal class Build : AzureDevOpsLibraryBuild
{
    public static int Main() => Execute<Build>(x => x.RunAllSteps);
}
