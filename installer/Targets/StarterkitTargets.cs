using System;
using System.Collections.Generic;
using Installer;
using Installer.Helper;
using Microsoft.Extensions.Logging;

namespace installer.Targets
{
    public class StarterkitTargets : TargetsBase
    {
        public StarterkitTargets(ILoggerFactory loggerFactory, Options options, IProcessHelper processHelper, KubernetesHelper kubernetesHelper, RancherHelper rancherHelper) : base(loggerFactory, options, processHelper, kubernetesHelper, rancherHelper)
        {
        }

        public override IDictionary<int, string> DefineTargetToExecute()
        {
            var targets = new Dictionary<int, string>();
            
            if(this.Options.InstallStarterKit || this.Options.FullInstallation)
            {
                targets.Add(21, "installStarterkitCommon");
                targets.Add(22, "installLdap");
                targets.Add(23, "installJenkins");
            }

            if(this.Options.UnInstallStarterKit)
            {
                targets.Add(71, "uninstallStarterKit");
            }
            
            return targets;        
        }
        
        public override void CreateTargets()
        {
            Bullseye.Targets.Target("installStarterkitCommon", Bullseye.Targets.DependsOn("checkKubeclt"), () =>
            {
                this.WriteHeader("Install Starterkit Commons");

                this.KubernetesHelper.InstallResourceIfNotExists("starterkit", "namespace");
                this.ProcessHelper.Run("kubectl", "apply -f ./../components/common/cert.yaml");
                
            });

            Bullseye.Targets.Target("installStorage", Bullseye.Targets.DependsOn("checkKubeclt"), () =>
            {
                this.WriteHeader("Install Storage");
                throw new NotImplementedException();
                // this.KubernetesHelper.InstallResourceIfNotExists("longhorn-system", "namespace");
                // this.KubernetesHelper.InstallApplicationeIfNotExists(
                //     "longhorn",
                //     "",
                //     "longhorn-system",
                //     $"./../components/longhorn --set ingress.host=storage.{this.Options.Dns}",
                //     "deployment.apps/longhorn-driver-deployer", 
                //     "deployment.apps/longhorn-ui"
                //     );
            });

            Bullseye.Targets.Target("installLdap", Bullseye.Targets.DependsOn("checkKubeclt"), () =>
            {
                this.WriteHeader("Install LDAP");
                
                this.KubernetesHelper.InstallApplicationeIfNotExists(
                    "ldap",
                    "stable/openldap",
                    "starterkit",
                    $"-f ./../components/openldap/values.yaml --set adminPassword=admin --set configPassword=admin",
                    "deploy/ldap-openldap");
                this.KubernetesHelper.InstallApplicationeIfNotExists(
                    "ldap-ui",
                    "",
                    "starterkit",
                    $"./../components/openldapui --set ingress.hosts[0].host=ldap.{this.Options.Dns} --set ingress.hosts[0].paths[0]=/",
                    "deploy/ldap-ui-openldapui");
            });

            Bullseye.Targets.Target("installJenkins", Bullseye.Targets.DependsOn("checkKubeclt"), () =>
            {
                this.WriteHeader("Install Jenkins");
                this.KubernetesHelper.InstallResourceIfNotExists(
                    "starterkit",
                    "configmap",
                    "starterkit",
                    "--from-file=./../components/jenkins/config");
                this.KubernetesHelper.InstallApplicationeIfNotExists(
                    "jenkins",
                    "stable/jenkins",
                    "starterkit",
                    $"-f ./../components/jenkins/values.yaml --set master.ingress.hostName=jenkins.{this.Options.Dns}",
                    "deploy/jenkins");
            });
            
            Bullseye.Targets.Target("unInstallStarterKit", Bullseye.Targets.DependsOn("checkKubeclt"), () =>
            {
                this.WriteHeader("Uninstall Starterkit");
                this.KubernetesHelper.UnInstallApplicationeIfExists("jenkins", "starterkit");
                this.KubernetesHelper.UnInstallApplicationeIfExists("ldap-ui", "starterkit");
                this.KubernetesHelper.UnInstallApplicationeIfExists("ldap", "starterkit");
            });
        }
    }
}