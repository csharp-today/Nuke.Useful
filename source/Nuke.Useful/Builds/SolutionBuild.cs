using Nuke.Common;
using Nuke.Common.ProjectModel;
using Nuke.Useful.Builds.Switcher;
using System;
using static Nuke.Common.IO.PathConstruction;

namespace Nuke.Useful.Builds
{
    public class SolutionBuild : NukeBuild
    {
        private readonly SolutionSwitcher _solutionSwitcher = new SolutionSwitcher();

        protected string CustomProjectName { get; set; }
        protected string CustomSolutionName { get; set; }

        protected Project Project { get; set; }

        [Solution] protected Solution Solution { get; private set; }

        public AbsolutePath SourceDirectory => RootDirectory / "source";

        protected Target SwitchSolution => _ => _
            .Executes(() => RunSwitchSolution(CustomSolutionName, CustomProjectName));

        protected void RunSwitchSolution(string solutionName, string projectName = null)
        {
                if (solutionName is null && projectName is null)
                {
                    Console.WriteLine("Not switched to custom solution/project");
                    return;
                }

                if (solutionName is { })
                {
                    Console.WriteLine($"Switching to custom solution: {solutionName}");
                    (Solution, Project) = _solutionSwitcher.RunSwitchSolution(Solution?.Directory ?? SourceDirectory, solutionName, projectName);
                    return;
                }

                if (Solution is null)
                {
                    Console.WriteLine("Can't switch to custom project, solution is empty");
                    return;
                }

                Console.WriteLine($"Switching to custom project: {projectName}");
                Project = Solution.GetProject(projectName);
        }
    }
}
