using System;
using System.IO;

class NuGetConfig : IDisposable
{
    private readonly string _file;

    public string FeedName { get; } = "FeedName";

    public NuGetConfig(string path) => _file = path + "\\nuget.config";

    public void Dispose() => File.Delete(_file);

    private void Create(string user, string secret)
    {
        File.WriteAllText(_file, $@"<?xml version=""1.0"" encoding=""utf-8""?>
<configuration>
  <packageSources>
    <clear />
    <add key=""{FeedName}"" value=""https://pkgs.dev.azure.com/mariuszbojkowski/_packaging/OpenSourceTest/nuget/v3/index.json"" />
  </packageSources>
  <packageSourceCredentials>
    <{FeedName}>
      <add key=""Username"" value=""{user}"" />
      <add key=""ClearTextPassword"" value=""{secret}"" />
    </{FeedName}>
  </packageSourceCredentials>
</configuration>");
    }

    public static NuGetConfig Create(string outputDirectory, string user, string secret)
    {
        var config = new NuGetConfig(outputDirectory);
        config.Create(user, secret);
        return config;
    }
}