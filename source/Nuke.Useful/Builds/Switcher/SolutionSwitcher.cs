using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Nuke.Useful.Builds.Switcher
{
    internal class SolutionSwitcher
    {
        private readonly MethodInfo _deserializeMethod;

        public SolutionSwitcher()
        {
            var nukeAssembly = typeof(Solution).Assembly;
            var solutionSerializerType = nukeAssembly.GetTypes().First(t => t.Name == "SolutionSerializer");
            _deserializeMethod = solutionSerializerType
                .GetMethods()
                .First(m => m.Name == "DeserializeFromFile" && m.GetParameters().Length == 1)
                .MakeGenericMethod(typeof(Solution));
        }

        public (Solution, Project) RunSwitchSolution(AbsolutePath solutionDirectory, string solutionName, string projectName = null)
        {
            if (projectName is null)
            {
                projectName = solutionName;
            }

            var path = Path.Combine(solutionDirectory, solutionName + ".sln");
            Console.WriteLine("Solution path = " + path);
            var solution = (Solution)_deserializeMethod.Invoke(null, new object[] { path });
            Console.WriteLine("Solution loaded");
            var project = solution.GetProject(projectName);
            Console.WriteLine("Project path = " + project.Path);

            return (solution, project);
        }
    }
}
