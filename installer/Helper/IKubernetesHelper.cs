namespace Installer.Helper
{
    internal interface IKubernetesHelper
    {
        void InstallResourceIfNotExists(string name, string resourceType, string nameSpace = null, string arguments = null);
        void InstallApplicationeIfNotExists(string name, string repo, string nameSpace = null, string arguments = null, params string[] checkRollout);
        void UnInstallApplicationeIfExists(string name, string nameSpace = null);
        string CreateNameSpaceCommand(string nameSpace);
        bool CheckIfResourceExists(string resourceType, string name, string nameSpace = null);
        bool CheckIfApplicationExists(string name, string nameSpace = null);
    }
}