using System;
using System.Collections.Generic;
using Installer;
using installer.Helper;
using Installer.Helper;
using Microsoft.Extensions.Logging;

namespace installer
{
    internal class StarterkitTargets : ITargetsBase
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

        public IDictionary<int, Action> DefineTargetToExecute()
        {
            var targets = new Dictionary<int, Action>();
            
            if(this.options.InstallStarterKit || this.options.FullInstallation)
            {
                targets.Add(21, this.InstallStarterkitCommon);
                targets.Add(22, this.InstallStorage);
                targets.Add(23, this.InstallLdap);
                targets.Add(24, this.InstallJenkins);
            }

            if(this.options.UnInstallStarterKit)
            {
                targets.Add(71, this.UninstallStarterKit);
            }
            
            return targets;        
        }

        internal void InstallStarterkitCommon()
        {
            this.logger.LogInformation("Install Starterkit Commons");

            this.kubernetesHelper.InstallResourceIfNotExists("starterkit", "namespace");
            this.processHelper.Run("kubectl", "apply -f ./../components/common/cert.yaml");
        }

        internal void InstallLdap()
        {
            this.logger.LogInformation("Install LDAP");
                
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
        }

        internal void InstallStorage()
        {
            this.logger.LogInformation("Install Storage");
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

        internal void InstallJenkins()
        {
            this.logger.LogInformation("Install Jenkins");
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
        }

        internal void UninstallStarterKit()
        {
            this.logger.LogInformation("Uninstall Starterkit");
            this.kubernetesHelper.UnInstallApplicationeIfExists("jenkins", "starterkit");
            this.kubernetesHelper.UnInstallApplicationeIfExists("ldap-ui", "starterkit");
            this.kubernetesHelper.UnInstallApplicationeIfExists("ldap", "starterkit");
        }
    }
}