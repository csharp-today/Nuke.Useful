﻿using Nuke.Common;
using System;
using System.IO;
using System.Security.Cryptography;

namespace Nuke.Useful.Builds
{
    public abstract class BlazorWebAssemblyBuild : WebAppBuild
    {
        protected virtual string BlazorDistSubdirectory => null;

        protected override Target RunAllSteps => _ => _
            .DependsOn(Step_7_SaveBlazorArtifacts)
            .Executes(DoNothingAction);

        protected Target Step_4_CompileBlazorProject => _ => _
            .DependsOn(Step_3_Restore)
            .Executes(() =>
            {
                if (Project is null)
                {
                    throw new ApplicationException($"{nameof(Project)} can't be empty, please set {nameof(CustomSolutionName)}/{nameof(CustomProjectName)}/both");
                }

                RunCompileTarget();
            });

        protected Target Step_5_PublishBlazorProject => _ => _
            .DependsOn(Step_4_CompileBlazorProject)
            .Executes(() => RunPublishWebTarget());

        protected Target Step_6_FixPublishedBlazorProject => _ => _
            .DependsOn(Step_5_PublishBlazorProject)
            .Executes(() =>
            {
                string parent = PublishOutput;
                var blazorDistSubdirectory = BlazorDistSubdirectory;
                Log($"{nameof(BlazorDistSubdirectory)} = {blazorDistSubdirectory}");
                if (blazorDistSubdirectory is null)
                {
                    Log($"{nameof(BlazorDistSubdirectory)} is null, detecting...");

                    const string DistDirectory = "wwwroot";
                    if (Directory.Exists(Path.Combine(parent, DistDirectory)))
                    {
                        blazorDistSubdirectory = DistDirectory;
                    }
                }

                Log($"{nameof(BlazorDistSubdirectory)} = {blazorDistSubdirectory}");
                if (blazorDistSubdirectory is null)
                {
                    Log($"Skipping the fix as the {nameof(BlazorDistSubdirectory)} is null");
                    return;
                }

                var webConfigPath = Path.Combine(parent, "web.config");
                if (File.Exists(webConfigPath))
                {
                    Log($"Detected web.config file: {webConfigPath}");
                    File.Delete(webConfigPath);
                    Log("web.config removed");
                }

                var distDirectory = Path.Combine(parent, blazorDistSubdirectory);
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

        protected Target Step_7_SaveBlazorArtifacts => _ => _
            .DependsOn(Step_6_FixPublishedBlazorProject)
            .Requires(() => ArtifactOutputDirectory)
            .Executes(RunSaveWebArtifactsTarget);
    }
}
