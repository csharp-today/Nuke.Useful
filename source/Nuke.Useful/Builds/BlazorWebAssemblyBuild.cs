using Nuke.Common;
using Nuke.Common.ProjectModel;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Nuke.Useful.Builds
{
    public abstract class BlazorWebAssemblyBuild : WebAppBuild
    {
        protected Project BlazorProject => Solution.GetProject(BlazorProjectName);
        protected abstract string BlazorDistSubdirectory { get; }
        protected abstract string BlazorProjectName { get; }

        protected Target CompileBlazorProject => _ => _
            .DependsOn(Restore)
            .Executes(() => RunCompileTarget(BlazorProject));

        protected Target BuildBlazorWebApp => _ => _.DependsOn(CompileBlazorProject);

        protected Target PublishBlazorProject => _ => _
            .DependsOn(CompileBlazorProject)
            .Executes(() => RunPublishWebTarget(BlazorProject));

        protected Target FixPublishedBlazorProject => _ => _
            .DependsOn(PublishBlazorProject)
            .Executes(() =>
            {
                string parent = PublishOutput;
                Log($"{nameof(BlazorDistSubdirectory)} = {BlazorDistSubdirectory}");
                var distDirectory = Path.Combine(parent, BlazorDistSubdirectory);
                MoveDirectory(distDirectory);
                Directory.Delete(distDirectory);

                string GetNewPath(string path) => Path.Combine(parent, Path.GetFileName(path));

                void Log(string msg) => Console.WriteLine(msg);

                void MoveDirectory(string dir)
                {
                    foreach (var d in Directory.GetDirectories(dir))
                    {
                        Log(" Directory: " + d);
                        if (Path.GetFileName(d) == "dist")
                        {
                            Log("Found dist - moving content");
                            MoveDirectory(d);
                            Directory.Delete(d);
                        }
                        else
                        {
                            var newDirectoryPath = GetNewPath(d);
                            SafeDirectoryMove(d, newDirectoryPath);

                            void SafeDirectoryMove(string source, string target, string prefix = " ")
                            {
                                if (Directory.Exists(target))
                                {
                                    PrefixLog("Target directory exist - move content.");
                                    foreach (var subDir in Directory.GetDirectories(source))
                                    {
                                        PrefixLog($" Directory: {subDir}");
                                        SafeDirectoryMove(subDir, Path.Combine(target, Path.GetFileName(subDir)), $" {prefix}");
                                    }
                                    foreach (var file in Directory.GetFiles(source))
                                    {
                                        PrefixLog($" File: {file}");
                                        SafeFileMove(file, Path.Combine(target, Path.GetFileName(file)));
                                    }
                                    PrefixLog("Clean up the source directory");
                                    Directory.Delete(source);
                                }
                                else
                                {
                                    Directory.Move(source, target);
                                }

                                void PrefixLog(string message) => Log($"{prefix}{message}");

                                void SafeFileMove(string inputFile, string outputFile)
                                {
                                    if (File.Exists(outputFile))
                                    {
                                        var inputSize = new FileInfo(inputFile).Length;
                                        if (inputSize > int.MaxValue)
                                        {
                                            throw new ApplicationException($"Too big input file: {inputSize}");
                                        }
                                        var outputSize = new FileInfo(outputFile).Length;
                                        if (outputSize > int.MaxValue)
                                        {
                                            throw new ApplicationException($"Too big output file: {outputSize}");
                                        }

                                        if (inputSize != outputSize)
                                        {
                                            throw new ApplicationException($"File conflict - different size: {inputSize} vs {outputSize}");
                                        }

                                        using (var inputStream = new FileStream(inputFile, FileMode.Open, FileAccess.Read))
                                        using (var outputStream = new FileStream(outputFile, FileMode.Open, FileAccess.Read))
                                        using (var md5 = MD5.Create())
                                        {
                                            var buffer = new byte[inputSize];
                                            inputStream.Read(buffer, 0, (int)inputSize);
                                            var inputHash = Convert.ToBase64String(md5.ComputeHash(buffer));

                                            buffer = new byte[outputSize];
                                            outputStream.Read(buffer, 0, (int)outputSize);
                                            var outputHash = Convert.ToBase64String(md5.ComputeHash(buffer));

                                            if (inputHash != outputHash)
                                            {
                                                throw new ApplicationException($"File conflick - different hash: {inputHash} vs {outputHash}");
                                            }
                                        }

                                        PrefixLog(" Ignoring - The same file is already present in the destination.");
                                        File.Delete(inputFile);
                                    }
                                    else
                                    {
                                        File.Move(inputFile, outputFile);
                                    }
                                }
                            }
                        }
                    }

                    foreach (var f in Directory.GetFiles(dir))
                    {
                        Log(" File: " + f);
                        File.Move(f, GetNewPath(f));
                    }
                }
            });

        protected Target PublishBlazorWebApp => _ => _.DependsOn(FixPublishedBlazorProject);
    }
}
