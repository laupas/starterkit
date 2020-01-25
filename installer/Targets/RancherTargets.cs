using System.Collections.Generic;
using Installer;
using Installer.Helper;
using Microsoft.Extensions.Logging;
using RestSharp;

namespace installer.Targets
{
    public class RancherTargets : TargetsBase
    {
        public RancherTargets(ILoggerFactory loggerFactory, Options options, KubernetesHelper kubernetesHelper, RancherHelper rancherHelper) : base(loggerFactory, options, kubernetesHelper, rancherHelper)
        {
        }

        public override IDictionary<int, string> DefineTargetToExecute()
        {
            var targets = new Dictionary<int, string>();
            
            if(this.Options.InstallRancher || this.Options.FullInstallation)
            {
                targets.Add(11, "installIngress");
                targets.Add(12, "installRancher");
                targets.Add(13, "configureRancher-step-1");
            }
            

            if(this.Options.InstallRancher || this.Options.FullInstallation)
            {
                targets.Add(81, "configureRancher-step-2");
            }
            
            return targets;        
        }
        
        public override void CreateTargets()
        {
            var ssl = "self";
            string token = null;
            
            Bullseye.Targets.Target("installIngress", Bullseye.Targets.DependsOn("checkKubeclt"), () =>
            {
                this.WriteHeader("Install Ingress");
                this.KubernetesHelper.InstallResourceIfNotExists("ingress-nginx", "namespace");
                this.KubernetesHelper.InstallResourceIfNotExists(
                    "ingress-default-cert",
                    "secret tls",
                    "ingress-nginx",
                    $"--cert=./../ssl/{ssl}/server.crt --key=./../ssl/{ssl}/server.key ");
                    
                this.KubernetesHelper.InstallApplicationeIfNotExists(
                    "ingress-nginx",
                    "stable/nginx-ingress",
                    "ingress-nginx",
                    "--set controller.extraArgs.default-ssl-certificate=ingress-nginx/ingress-default-cert",
                    "deployment.apps/ingress-nginx-nginx-ingress-controller",
                    "deployment.apps/ingress-nginx-nginx-ingress-default-backend");
            });
            
            Bullseye.Targets.Target("installRancher", Bullseye.Targets.DependsOn("checkKubeclt"), () =>
            {
                this.WriteHeader("Install Rancher");
                this.KubernetesHelper.InstallResourceIfNotExists("cattle-system", "namespace");
                this.KubernetesHelper.InstallResourceIfNotExists(
                    "tls-rancher-ingress",
                    "secret tls",
                    "cattle-system",
                    $"--cert=./../ssl/{ssl}/server.crt --key=./../ssl/{ssl}/server.key ");

                this.KubernetesHelper.InstallResourceIfNotExists(
                    "tls-ca",
                    "secret generic",
                    "cattle-system",
                    $"--from-file=./../ssl/{ssl}/cacerts.pem");

                this.KubernetesHelper.InstallApplicationeIfNotExists(
                    "rancher",
                    "rancher-latest/rancher",
                    "cattle-system",
                    $"--set hostname={this.Options.Dns} " +
                    "--set privateCA=true " +
                    "--set 'extraEnv[0].name=CATTLE_CA_CHECKSUM' " +
                    "--set 'extraEnv[0].value=' " +
                    "--set ingress.tls.source=secret",
                    "deploy/rancher");

//                Command.Run("kubectl", "apply -f ./../components/rancher/rancher.ingress.yml");
            });
            
            Bullseye.Targets.Target("configureRancher-step-1", Bullseye.Targets.DependsOn("checkKubeclt"), () =>
            {
                this.WriteHeader("Configure Rancher");
                var client = new RestClient($"https://{this.Options.Dns}");
                this.RancherHelper.WaitUntilIsUpAndReady(client);
                token = this.RancherHelper.Login(client);
                client.AddDefaultHeader("Authorization", $"Bearer {token}");
                this.RancherHelper.ChangePassword(client, token);
                this.RancherHelper.SetServerUrl(client, this.Options.Dns);
            });

            Bullseye.Targets.Target("configureRancher-step-2", Bullseye.Targets.DependsOn("checkKubeclt"), () =>
            {
                this.WriteHeader("Configure Rancher step 2");
                var client = new RestClient($"https://{this.Options.Dns}");
                client.AddDefaultHeader("Authorization", $"Bearer {token}");
                this.RancherHelper.WaitUntilIsActive(client);
                this.RancherHelper.ExecuteUpdatesForDockerDesktop();
            });

            Bullseye.Targets.Target("showInfo", () => {
                this.WriteHeader("StarterKit installation is done");

                if(this.Options.InstallRancher || this.Options.FullInstallation)
                {
                    this.Logger.LogInformation($"RancherUi: https://{this.Options.Dns}");
                    this.Logger.LogInformation($"Credentials: admin:admin");
                }

                if(this.Options.InstallStarterKit || this.Options.FullInstallation)
                {
                    this.Logger.LogInformation($"UserManagement: https://ldap.{this.Options.Dns}");
                    this.Logger.LogInformation($"Credentials: cn=admin,dc=starterkit,dc=home:admin");
                    this.Logger.LogInformation($"Jenkins: https://jenkins.{this.Options.Dns}");
                    this.Logger.LogInformation($"Credentials: devops:devops");
                }
            });
        }
    }
}