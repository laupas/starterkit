namespace Installer.Helper
{
    internal interface IKubernetesHelper
    {
        void CreateNameSpace(string nameSpace);

        void InstallResourceIfNotExists(string name, string resourceType, string nameSpace, string arguments = null);
        
        void InstallApplicationeIfNotExists(string name, string repo, string nameSpace, string arguments = null, params string[] checkRollout);
        
        void UnInstallApplicationeIfExists(string name, string nameSpace);
        
        bool CheckIfResourceExists(string name, string resourceType, string nameSpace);
        
        bool CheckIfApplicationExists(string name, string nameSpace);
        
        string ExecuteKubectlCommand(string command);
    }
}