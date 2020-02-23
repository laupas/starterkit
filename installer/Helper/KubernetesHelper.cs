using System.Linq;
using Microsoft.Extensions.Logging;

namespace Installer.Helper
{
    internal class KubernetesHelper : IKubernetesHelper
    {
        private readonly IProcessHelper processHelper;
        private readonly ILogger logger;

        public KubernetesHelper(ILoggerFactory loggerFactory, IProcessHelper processHelper)
        {
            this.processHelper = processHelper;
            this.logger = loggerFactory.CreateLogger(this.GetType().Name);
        }

        public void CreateNameSpace(string nameSpace)
        {
            if (!this.CheckIfResourceExists(nameSpace, "namespace", string.Empty))
            {
                this.logger.LogInformation($"Create namespace {nameSpace}");
                this.processHelper.Run($"kubectl", $"create namespace {nameSpace}");
            }
        }

        public void InstallResourceIfNotExists(string name, string resourceType, string nameSpace, string arguments = null)
        {
            if (!this.CheckIfResourceExists(name, resourceType.Split(' ').First(), nameSpace))
            {
                this.logger.LogInformation($"Create {resourceType} {name}");
                if (string.IsNullOrEmpty(nameSpace))
                {
                    this.processHelper.Run("kubectl", $"create {resourceType} {name} {arguments}".Trim());
                }
                else
                {
                    this.processHelper.Run("kubectl", $"create {resourceType} {name} --namespace {nameSpace} {arguments}".Trim());
                }
            }
        }
        
        public void InstallApplicationeIfNotExists(string name, string repo, string nameSpace = null, string arguments = null, params string[] checkRollout)
        {
            if (!this.CheckIfApplicationExists(name, nameSpace))
            {
                this.logger.LogInformation($"Create {name} {repo}");
                this.processHelper.Run("helm", $"install {name} {repo} --namespace {nameSpace} {arguments}".Trim());
                checkRollout?.ToList().ForEach(check =>
                    {
                        
                        this.processHelper.Run("kubectl", $"rollout status {check} --namespace {nameSpace}");
                });
            }
        }

        public void UnInstallApplicationeIfExists(string name, string nameSpace)
        {
            if (this.CheckIfApplicationExists(name, nameSpace))
            {
                this.logger.LogInformation($"Uninstall {name}");
                this.processHelper.Run("helm", $"uninstall {name} --namespace {nameSpace}");
            }
        }
        
        public bool CheckIfResourceExists(string name, string resourceType, string nameSpace)
        {
            if (!string.IsNullOrEmpty(nameSpace))
            {
                nameSpace = $"--namespace {nameSpace}";
            }
            
            var ressource = this.processHelper.Read("kubectl", $"get {resourceType} {nameSpace}".Trim(), noEcho: true);
            if(ressource.Contains(name))
            {
                this.logger.LogInformation($"{resourceType} {name} exists.");
                return true;
            }
            return false;
        }

        public bool CheckIfApplicationExists(string name, string nameSpace)
        {
            var result = this.processHelper.Read("helm", $"list --namespace {nameSpace}".Trim(), noEcho: true);
            if(result.Contains(name))
            {
                this.logger.LogInformation($"Application {name} exists.");
                return true;
            }
            return false;
        }

        public string ExecuteKubectlCommand(string command)
        {
            return this.processHelper.Read($"kubectl", command);
        }
    }
}