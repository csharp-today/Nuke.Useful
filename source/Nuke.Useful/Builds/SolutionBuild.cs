using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Useful.Builds.Switcher;
using System;
using System.Linq;

namespace Nuke.Useful.Builds
{
    public class SolutionBuild : NukeBuild
    {
        private readonly SolutionSwitcher _solutionSwitcher = new SolutionSwitcher();

        protected string CustomProjectName { get; set; }
        protected string CustomSolutionName { get; set; }

        protected Action DoNothingAction => () => { };

        protected virtual bool ForceEnglishDotNetLanguage => true;

        protected Project Project { get; set; }

        [Solution] protected Solution Solution { get; private set; }

        public AbsolutePath SourceDirectory => RootDirectory / "source";

        protected virtual Target RunAllSteps => _ => _
            .DependsOn(Step_1_SwitchSolution)
            .Executes(DoNothingAction);

        protected Target Step_1_SwitchSolution => _ => _
            .Executes(() => RunSwitchSolution(CustomSolutionName, CustomProjectName));

        public SolutionBuild()
        {
            if (ForceEnglishDotNetLanguage)
            {
                Environment.SetEnvironmentVariable("DOTNET_CLI_UI_LANGUAGE", "en");
            }
        }

        protected void RunSwitchSolution(string solutionName, string projectName = null)
        {
            if (solutionName is { } && projectName is { })
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

            if (projectName is { })
            {
                Console.WriteLine($"Switching to custom project: {projectName}");
                Project = Solution.GetProject(projectName);
                return;
            }

            Console.WriteLine("Taking default project");
            Project = Solution.AllProjects.FirstOrDefault();
            Console.WriteLine($"Default project: {Project?.Name}");
        }
    }
}
