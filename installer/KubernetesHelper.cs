using System;
using System.Linq;
using installer.Helper;
using Microsoft.Extensions.Logging;

namespace installer
{
    public class KubernetesHelper
    {
        private readonly IProcessHelper processHelper;
        private readonly ILogger logger;

        public KubernetesHelper(ILoggerFactory loggerFactory, IProcessHelper processHelper)
        {
            this.processHelper = processHelper;
            this.logger = loggerFactory.CreateLogger(this.GetType().Name);
        }
        public void InstallResourceIfNotExists(string name, string resourceType, string nameSpace = null, string arguments = null)
        {
            var nameSpaceCommand = CreateNameSpaceCommand(nameSpace);

            if (!CheckIfResourceExists(resourceType.Split(' ').First(), name, nameSpace))
            {
                this.logger.LogInformation($"Create {resourceType} {name}");
                this.processHelper.Run("kubectl", $"create {resourceType} {name} {arguments} {nameSpaceCommand}");
            }
        }
        
        public void InstallApplicationeIfNotExists(string name, string repo, string nameSpace = null, string arguments = null, params string[] checkRollout)
        {
            var nameSpaceCommand = CreateNameSpaceCommand(nameSpace);

            if (!CheckIfApplicationExists(name, nameSpace))
            {
                this.logger.LogInformation($"Create {name} {repo}");
                this.processHelper.Run("helm", $"install {name} {repo} {arguments} {nameSpaceCommand}");
                checkRollout?.ToList().ForEach(check =>
                    {
                        
                        this.processHelper.Run("kubectl", $"rollout status {check} {nameSpaceCommand}");
                });
                
            }
        }

        private string CreateNameSpaceCommand(string nameSpace)
        {
            var nameSpaceCommand = String.Empty;
            if (!String.IsNullOrEmpty(nameSpace))
            {
                nameSpaceCommand = $"--namespace {nameSpace}";
            }

            return nameSpaceCommand;
        }

        private bool CheckIfResourceExists(string resourceType, string name, string nameSpace = null)
        {
            var nameSpaceCommand = CreateNameSpaceCommand(nameSpace);
            var namespaces = this.processHelper.Read("kubectl", $"{nameSpaceCommand} get {resourceType}", noEcho: true);
            if(namespaces.Contains(name))
            {
                this.logger.LogInformation($"{resourceType} {name} exists.");
                return true;
            }
            return false;
        }

        private bool CheckIfApplicationExists(string name, string nameSpace = null)
        {
            var nameSpaceCommand = CreateNameSpaceCommand(nameSpace);
            var namespaces = this.processHelper.Read("helm", $"list {nameSpaceCommand}", noEcho: true);
            if(namespaces.Contains(name))
            {
                this.logger.LogInformation($"Application {name} exists.");
                return true;
            }
            return false;
        }
    }
}