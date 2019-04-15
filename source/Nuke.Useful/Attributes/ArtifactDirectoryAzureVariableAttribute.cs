namespace Nuke.Useful.Attributes
{
    public class ArtifactDirectoryAzureVariableAttribute : AzureVariableAttribute
    {
        public ArtifactDirectoryAzureVariableAttribute() : base("BUILD_ARTIFACTSTAGINGDIRECTORY") { }
    }
}
