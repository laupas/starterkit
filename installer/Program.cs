using System;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static Bullseye.Targets;
using static SimpleExec.Command;
using static System.Console;
using RestSharp;
using System.Linq;
using System.IO;
using installer;

namespace Installer
{
    static class Program
    {
        static string rancherUrl = "starterkit.devops.family";
        static void Main(string[] args)
        {
            var ssl = "self";
            
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;

            Target("checkKubeclt", () =>
            {
                try
                {
                    Run("kubectl", "get nodes");
                    try 
                    {
                        var hostAddress = Dns.GetHostEntry("host.docker.internal").AddressList.First().ToString();
                        WriteLine("Running inside Docker");
                        WriteLine($"Add {hostAddress} {rancherUrl} to host");

                        File.AppendAllText("/etc/hosts", $"{hostAddress} {rancherUrl}{Environment.NewLine}");
                    }
                    catch
                    {
                            WriteLine("Running outside Docker");
                    }
                }
                catch
                {
                    WriteLine("Kube folder is not mapped. Please start this container with:");
                    WriteLine("-v $HOME/.kube:/root/.kube");
                    throw;
                }
            });

            Target("installIngress", DependsOn("checkKubeclt"), () =>
            {
                Kubernetes.WriteHeader("Install Ingress");

                Kubernetes.InstallResourceIfNotExists("ingress-nginx", "namespace");
                
                Kubernetes.InstallResourceIfNotExists(
                    "ingress-default-cert", 
                    "secret tls", 
                    "ingress-nginx", 
                    $"--cert=./../ssl/{ssl}/server.crt --key=./../ssl/{ssl}/server.key ");

                Kubernetes.InstallApplicationeIfNotExists(
                    "ingress-nginx", 
                    "stable/nginx-ingress", 
                    "ingress-nginx", 
                    "--set controller.extraArgs.default-ssl-certificate=ingress-nginx/ingress-default-cert",
                    "deployment.apps/ingress-nginx-nginx-ingress-controller",
                    "deployment.apps/ingress-nginx-nginx-ingress-default-backend");
            });
            
            Target("installRancher", DependsOn("checkKubeclt"), () =>
            {
                Kubernetes.WriteHeader("Install Rancher");
                
                Kubernetes.InstallResourceIfNotExists("cattle-system", "namespace");
                
                Kubernetes.InstallResourceIfNotExists(
                    "tls-rancher-ingress", 
                    "secret tls", 
                    "cattle-system",
                    $"--cert=./../ssl/{ssl}/server.crt --key=./../ssl/{ssl}/server.key ");

                Kubernetes.InstallResourceIfNotExists(
                    "tls-ca", 
                    "secret generic", 
                    "cattle-system",
                    $"--from-file=./../ssl/{ssl}/cacerts.pem");
                
                Kubernetes.InstallApplicationeIfNotExists(
                    "rancher", 
                    "rancher-latest/rancher", 
                    "cattle-system", 
                    $"--set hostname={ rancherUrl } " +
                    "--set privateCA=true " +
                    "--set ingress.tls.source=secret",
                    "deploy/rancher");

            });
            
            Target("configureRancher", DependsOn("checkKubeclt"), () =>
            {
                Kubernetes.WriteHeader("Configure Rancher");
                var client = new RestClient($"https://{rancherUrl}");

                Rancher.WaitUntilIsUpAndReady(client);
                var token = Rancher.Login(client);
                Rancher.ChangePassword(client, token);
                Rancher.SetServerUrl(client, rancherUrl);
            });

            Target("installStarterKit", DependsOn("checkKubeclt"), () =>
            {
                Kubernetes.WriteHeader("Install Starterkit");

                Kubernetes.InstallResourceIfNotExists("starterkit", "namespace");
                
                Run("kubectl", "apply -f ./../components/common/cert.yaml");

                Kubernetes.InstallApplicationeIfNotExists(
                    "ldap", 
                    "stable/openldap", 
                    "starterkit", 
                    $"-f ./../components/openldap/values.yaml",
                    "deploy/ldap-openldap");

                Kubernetes.InstallApplicationeIfNotExists(
                    "ldap-ui", 
                    "", 
                    "starterkit", 
                    $"./../components/openldapui",
                    "deploy/ldap-ui-openldapui");

                Kubernetes.InstallResourceIfNotExists(
                    "starterkit", 
                    "configmap", 
                    "starterkit", 
                    "--from-file=./../components/jenkins/config");

                Kubernetes.InstallApplicationeIfNotExists(
                    "jenkins", 
                    "stable/jenkins", 
                    "starterkit", 
                    $"-f ./../components/jenkins/values.yaml --set master.ingress.hostName=jenkins.{rancherUrl}",
                    "deploy/jenkins");
                
            });

            Target("default", DependsOn("checkKubeclt", "installIngress", "installRancher", "configureRancher", "installStarterKit"), () =>
            {
                WriteLine("default");
            });
            
            RunTargetsAndExit(args);
        }
    }
}