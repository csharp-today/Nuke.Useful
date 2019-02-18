using System.IO;

namespace Nuke.Useful
{
    public class NuGetConfig
    {
        private readonly string _file;

        public string FeedName { get; } = "FeedName";

        public NuGetConfig(string path) => _file = path + "\\nuget.config";

        public void Dispose() => File.Delete(_file);

        private void Create(string url, string user, string secret)
        {
            File.WriteAllText(_file, $@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <clear />
    <add key=""{FeedName}"" value=""{url}"" />
  </packageSources>
  <packageSourceCredentials>
    <{FeedName}>
      <add key=""Username"" value=""{user}"" />
      <add key=""ClearTextPassword"" value=""{secret}"" />
    </{FeedName}>
  </packageSourceCredentials>
</configuration>");
        }

        public static NuGetConfig Create(string outputDirectory, string url, string user, string secret)
        {
            var config = new NuGetConfig(outputDirectory);
            config.Create(url, user, secret);
            return config;
        }
    }
}
