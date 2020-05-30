using System.Linq;
using Installer.Helper;
using Microsoft.Extensions.Logging;

namespace Installer
{
    internal class StarterkitTargets : IStarterkitTargets
    {
        private readonly Options options;
        private readonly IProcessHelper processHelper;
        private readonly IKubernetesHelper kubernetesHelper;
        private ILogger logger;

        public StarterkitTargets(ILoggerFactory loggerFactory, Options options, IProcessHelper processHelper, IKubernetesHelper kubernetesHelper)
        {
            this.options = options;
            this.processHelper = processHelper;
            this.kubernetesHelper = kubernetesHelper;
            this.logger = loggerFactory.CreateLogger(this.GetType().Name);
        }

        public void InstallStarterkitCommon(InstallerProcess installerProcess)
        {
            this.logger.LogInformation("Install Starterkit Commons");

            this.kubernetesHelper.CreateNameSpace("starterkit");
            this.processHelper.Run("kubectl", "apply -f ./../components/common/cert.yaml");
            installerProcess.AddExecutedTask("InstallStarterkitCommon");
        }

        public void InstallLdap(InstallerProcess installerProcess)
        {
            this.logger.LogInformation("Install LDAP");
            if (!installerProcess.ExecutedActions.Contains("InstallStarterkitCommon"))
            {
                InstallStarterkitCommon(installerProcess);
            }
                
            this.kubernetesHelper.InstallApplicationeIfNotExists(
                "ldap",
                "stable/openldap",
                "starterkit",
                $"-f ./../components/openldap/values.yaml --set adminPassword=admin --set configPassword=admin",
                "deploy/ldap-openldap");
            this.kubernetesHelper.InstallApplicationeIfNotExists(
                "ldap-ui",
                "",
                "starterkit",
                $"./../components/openldapui --set ingress.hosts[0].host=ldap.{this.options.Dns} --set ingress.hosts[0].paths[0]=/",
                "deploy/ldap-ui-openldapui");      
            installerProcess.AddExecutedTask("InstallLdap");
        }

        public void InstallStorage(InstallerProcess installerProcess)
        {
            this.logger.LogInformation("Install Storage");
            if (!installerProcess.ExecutedActions.Contains("InstallStarterkitCommon"))
            {
                InstallStarterkitCommon(installerProcess);
            }

            // this.KubernetesHelper.InstallResourceIfNotExists("longhorn-system", "namespace");
            // this.KubernetesHelper.InstallApplicationeIfNotExists(
            //     "longhorn",
            //     "",
            //     "longhorn-system",
            //     $"./../components/longhorn --set ingress.host=storage.{this.options.Dns}",
            //     "deployment.apps/longhorn-driver-deployer", 
            //     "deployment.apps/longhorn-ui"
            //     );
        }

        public void InstallJenkins(InstallerProcess installerProcess)
        {
            this.logger.LogInformation("Install Jenkins");
            if (!installerProcess.ExecutedActions.Contains("InstallStarterkitCommon"))
            {
                InstallStarterkitCommon(installerProcess);
            }

            this.kubernetesHelper.InstallResourceIfNotExists(
                "starterkit",
                "configmap",
                "starterkit",
                "--from-file=./../components/jenkins/config");
            this.kubernetesHelper.InstallApplicationeIfNotExists(
                "jenkins",
                "stable/jenkins",
                "starterkit",
                $"-f ./../components/jenkins/values.yaml --set master.ingress.hostName=jenkins.{this.options.Dns}",
                "deploy/jenkins");        
            
            installerProcess.AddExecutedTask("InstallJenkins");

        }

        public void UninstallStarterKit(InstallerProcess installerProcess)
        {
            this.logger.LogInformation("Uninstall Starterkit");
            this.kubernetesHelper.UnInstallApplicationeIfExists("jenkins", "starterkit");
            this.kubernetesHelper.UnInstallApplicationeIfExists("ldap-ui", "starterkit");
            this.kubernetesHelper.UnInstallApplicationeIfExists("ldap", "starterkit");
        }

    }
}