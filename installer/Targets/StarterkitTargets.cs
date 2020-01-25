using System.Collections.Generic;
using Installer;
using Installer.Helper;
using Microsoft.Extensions.Logging;
using SimpleExec;

namespace installer.Targets
{
    public class StarterkitTargets : TargetsBase
    {
        public StarterkitTargets(ILoggerFactory loggerFactory, Options options, KubernetesHelper kubernetesHelper, RancherHelper rancherHelper) : base(loggerFactory, options, kubernetesHelper, rancherHelper)
        {
        }

        public override IDictionary<int, string> DefineTargetToExecute()
        {
            var targets = new Dictionary<int, string>();
            
            if(this.Options.InstallStarterKit || this.Options.FullInstallation)
            {
                targets.Add(21, "installStarterKit");
            }

            if(this.Options.UnInstallStarterKit)
            {
                targets.Add(71, "uninstallStarterKit");
            }

            
            return targets;        
        }
        
        public override void CreateTargets()
        {
            Bullseye.Targets.Target("installStarterKit", Bullseye.Targets.DependsOn("checkKubeclt"), () =>
            {
                this.WriteHeader("Install Starterkit");
                this.KubernetesHelper.InstallResourceIfNotExists("starterkit", "namespace");
                Command.Run("kubectl", "apply -f ./../components/common/cert.yaml");
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