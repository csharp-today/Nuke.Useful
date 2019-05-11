using AzureUploader;
using FluentFTP;
using Microsoft.Azure.Management.AppService.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Nuke.Common;
using Nuke.Useful.Attributes;
using System.IO;
using System.Net;

namespace Nuke.Useful.Builds
{
    public abstract class AzureWebAppRelease : NukeBuild
    {
        [AzureVariable] protected string AzureClientId { get; }
        [AzureVariable] protected string AzureSecret { get; }
        [AzureVariable] protected string AzureSubscriptionId { get; }
        [AzureVariable] protected string AzureTenantId { get; }

        [AzureVariable] protected string AzureDeploySecret { get; }
        [AzureVariable] protected string AzureDeployUrl { get; }
        [AzureVariable] protected string AzureDeployUser { get; }

        protected abstract string AzureResourceGroupName { get; }
        protected abstract string AzureWebAppName { get; }
        protected abstract string PublishOutputDirectoryName { get; }

        IWebApp WebApp;
        protected Target ConnectToAzureWebApp => _ => _
            .Requires(() => AzureClientId)
            .Requires(() => AzureSecret)
            .Requires(() => AzureSubscriptionId)
            .Requires(() => AzureTenantId)
            .Executes(() =>
            {
                var credentials = new AzureCredentialsFactory()
                    .FromServicePrincipal(AzureClientId, AzureSecret, AzureTenantId, AzureEnvironment.AzureGlobalCloud);
                Logger.Info("Azure credentials prepared");
                var azure = Azure.Authenticate(credentials).WithSubscription(AzureSubscriptionId);
                Logger.Info("Connected to Azure");
                WebApp = azure.WebApps.GetByResourceGroup(AzureResourceGroupName, AzureWebAppName);
                Logger.Info("Connected to web app");
            });

        protected Target StopWebSite => _ => _
            .DependsOn(ConnectToAzureWebApp)
            .Executes(() =>
            {
                WebApp.Stop();
                Logger.Info("Web app stopped");
            });

        protected Target Publish => _ => _
            .DependsOn(StopWebSite)
            .Requires(() => AzureDeployUrl)
            .Requires(() => AzureDeployUser)
            .Requires(() => AzureDeploySecret)
            .Executes(() =>
            {
                new AzureFtpUploader(() => new FtpClient(AzureDeployUrl)
                {
                    Credentials = new NetworkCredential(AzureDeployUser, AzureDeploySecret)
                }, new LogAdapter()).Deploy(PublishOutputDirectoryName);
            });

        protected Target StartWebSite => _ => _
            .DependsOn(Publish)
            .Executes(() =>
            {
                WebApp.Start();
                Logger.Info("Web app started");
            });

        protected Target DeployAzureWebApp => _ => _
            .DependsOn(StartWebSite);

        protected static void EnsureNukeConfigFileIsPresent()
        {
            const string ConfigFile = ".nuke";
            if (!File.Exists(ConfigFile))
            {
                File.WriteAllText(ConfigFile, "");
            }
        }
    }
}
