using Nuke.Common;
using System;
using System.IO;

namespace Nuke.Useful.Builds
{
    public abstract class BlazorWebBuild : WebAppBuild
    {
        protected abstract string BlazorAssemblyName { get; }

        protected string HostingRootDirectory { get; set; } = @"D:\home\site\wwwroot";

        protected Target GenerateBlazorConfig => _ => _
            .DependsOn(SaveWebArtifacts)
            .Executes(() =>
            {
                var configPath = Path.Combine(ArtifactOutputDirectory, PublishOutputDirectoryName, $"{BlazorAssemblyName}.blazor.config");
                var content = $@"{HostingRootDirectory}\{BlazorAssemblyName}\fake.csproj{Environment.NewLine}fake.dll";
                File.WriteAllText(configPath, content);
            });
    }
}
