using Nuke.Common;
using System;
using System.Collections.Generic;

namespace Nuke.Useful
{
    public class FlexBuilder : NukeBuild
    {
        private List<Func<int>> _builds = new List<Func<int>>();

        public FlexBuilder Add<T>() where T : NukeBuild, IFlexBuild
        {
            _builds.Add(() => Execute<T>(x => x.RunAll));
            return this;
        }

        public int Run()
        {
            foreach (var buildFunc in _builds)
            {
                int exitCode = buildFunc();
                if (exitCode != 0)
                {
                    return exitCode;
                }
            }

            return 0;
        }
    }
}
