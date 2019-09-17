using Nuke.Common;
using System;
using System.Collections.Generic;
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

        protected Target FixWebConfig => _ => _
            .DependsOn(GenerateBlazorConfig)
            .Executes(() =>
            {
                var configPath = Path.Combine(ArtifactOutputDirectory, PublishOutputDirectoryName, "web.config");
                var lines = File.ReadAllText(configPath).Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                var acceptedLines = new List<string>();
                var inRewrite = false;
                foreach (var line in lines)
                {
                    if (inRewrite)
                    {
                        if (line.Trim().Equals("</rewrite>", StringComparison.OrdinalIgnoreCase))
                        {
                            inRewrite = false;
                        }
                    }
                    else
                    {
                        if (line.Trim().Equals("<rewrite>", StringComparison.OrdinalIgnoreCase))
                        {
                            inRewrite = true;
                        }
                        else
                        {
                            acceptedLines.Add(line);
                        }
                    }
                }

                var correctedContent = string.Join(Environment.NewLine, acceptedLines);
                File.WriteAllText(configPath, correctedContent);
            });

        protected Target BuildBlazorWebApp => _ => _.DependsOn(FixWebConfig);
    }
}
