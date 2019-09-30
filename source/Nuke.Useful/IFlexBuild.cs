using Nuke.Common;

namespace Nuke.Useful
{
    public interface IFlexBuild
    {
        Target RunAll { get; }
    }
}
