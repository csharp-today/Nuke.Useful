using Nuke.Common.IO;
using Nuke.Common.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.IO;

namespace Nuke.Useful
{
    public class ArtifactStorage
    {
        private readonly string _directory;

        private ArtifactStorage(string directory) => _directory = directory;

        public static ArtifactStorage Create(string outputDirectory) => new ArtifactStorage(outputDirectory);

        public ArtifactStorage AddDirectory(AbsolutePath path, string overrideName = null)
        {
            if (!Directory.Exists(path.ToString()))
            {
                throw new ArgumentException("The directory doesn't exist: " + path.ToString());
            }

            CopyDirectory(path.ToString(), _directory, overrideName);
            return this;
        }

        public ArtifactStorage AddFile(string filePath)
        {
            CopyFile(filePath, _directory);
            return this;
        }

        public ArtifactStorage AddFiles(IEnumerable<string> files)
        {
            foreach (var filePath in files)
            {
                CopyFile(filePath, _directory);
            }
            return this;
        }

        public ArtifactStorage AddFile(string name, string content)
        {
            File.WriteAllText(Path.Combine(_directory, name), content);
            return this;
        }

        private void CopyDirectory(string source, string target, string overrideName = null)
        {
            var name = overrideName ?? Path.GetFileName(source);
            var newDirectory = Path.Combine(target, name);
            Directory.CreateDirectory(newDirectory);

            Directory.GetDirectories(source).ForEach(d => CopyDirectory(d, newDirectory));
            Directory.GetFiles(source).ForEach(file => CopyFile(file, newDirectory));
        }

        private void CopyFile(string sourceFile, string targetDirectory)
        {
            var fileName = Path.GetFileName(sourceFile);
            var newFile = Path.Combine(targetDirectory, fileName);
            File.Copy(sourceFile, newFile);
        }
    }
}
