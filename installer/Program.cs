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
                    WriteLine("checkVolumes");
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
                WriteLine("install Ingress");
                if(!Kubernetes.CheckIfResourceExists("namespaces", "ingress-nginx"))
                {
                    WriteLine("Create Namespace ingress-nginx");
                    Run("kubectl", "create namespace ingress-nginx");
                }

                if(!Kubernetes.CheckIfResourceExists("secret", "ingress-default-cert", "ingress-nginx"))
                {
                    WriteLine("Create ssl secret");
                    Run("kubectl", "-n ingress-nginx create secret tls ingress-default-cert " +
                                        $"--cert=./../ssl/{ssl}/server.crt " +
                                        $"--key=./../ssl/{ssl}/server.key ");
                }
                
                if (!Kubernetes.CheckIfApplicationExists("ingress-nginx", "ingress-nginx"))
                {
                    WriteLine("Install Ingress");
                    Run("helm", "install ingress-nginx stable/nginx-ingress" +
                                   " --namespace ingress-nginx " +
                                    "--set controller.extraArgs.default-ssl-certificate=ingress-nginx/ingress-default-cert");
                    
                    Run("kubectl", "-n ingress-nginx rollout status deployment.apps/ingress-nginx-nginx-ingress-controller");
                    Run("kubectl", "-n ingress-nginx rollout status deployment.apps/ingress-nginx-nginx-ingress-default-backend");
                }
            });
            
            Target("installRancher", DependsOn("checkKubeclt"), () =>
            {
                WriteLine("install Rancher");
                if(!Kubernetes.CheckIfResourceExists("namespaces", "cattle-system"))
                {
                    WriteLine("Create Namespace cattle-system");
                    Run("kubectl", "create namespace cattle-system");
                }
                
                if(!Kubernetes.CheckIfResourceExists("secret", "tls-rancher-ingress", "cattle-system"))
                {
                    WriteLine("Create ssl secret");
                    Run("kubectl", "-n cattle-system create secret tls tls-rancher-ingress " +
                                   $"--cert=./../ssl/{ssl}/server.crt " +
                                   $"--key=./../ssl/{ssl}/server.key ");
                }

                if(!Kubernetes.CheckIfResourceExists("secret", "tls-ca", "cattle-system"))
                {
                    WriteLine("Create generic secret");
                    Run("kubectl", "-n cattle-system create secret generic tls-ca " +
                                   $"--from-file=./../ssl/{ssl}/cacerts.pem ");
                }

                if (!Kubernetes.CheckIfApplicationExists("rancher", "cattle-system"))
                {
                    WriteLine("Install Rancher");
                    Run("helm", "install rancher rancher-latest/rancher " +
                                "--namespace cattle-system " +
                                $"--set hostname={ rancherUrl } " +
                                "--set privateCA=true " +
                                "--set ingress.tls.source=secret");
                    
                    Run("kubectl", "-n cattle-system rollout status deploy/rancher");
                }
            });
            
            Target("configureRancher", DependsOn("checkKubeclt"), () =>
            {
                WriteLine("Configure Rancher");
                var client = new RestClient($"https://{rancherUrl}");

                Rancher.WaitUntilIsUpAndReady(client);
                var token = Rancher.Login(client);
                Rancher.ChangePassword(client, token);
                Rancher.SetServerUrl(client, rancherUrl);
            });

            Target("installStarterKit", DependsOn("checkKubeclt"), () =>
            {
                WriteLine("Install Starterkit");
                if(!Kubernetes.CheckIfResourceExists("namespaces", "starterkit"))
                {
                    WriteLine("Create Namespace starterkit");
                    Run("kubectl", "create namespace starterkit");
                }
                
                Run("kubectl", "apply -f ./../components/common/cert.yaml");

                if (!Kubernetes.CheckIfApplicationExists("ldap", "starterkit"))
                {
                    WriteLine("Install Ingress");
                    Run("helm", "install ldap stable/openldap" +
                                " --namespace starterkit " +
                                "-f ./../components/openldap/values.yaml");
                    
                    Run("kubectl", "-n starterkit rollout status deploy/ldap-openldap");
                }

                if (!Kubernetes.CheckIfApplicationExists("ldap-ui", "starterkit"))
                {
                    WriteLine("Install ldap-ui");
                    Run("helm", "install ldap-ui" +
                                " --namespace starterkit " +
                                "./../components/openldapui");
                    
                    Run("kubectl", "-n starterkit rollout status deploy/ldap-ui-openldapui");
                }
                
                if(!Kubernetes.CheckIfResourceExists("configmap", "starterkit", "starterkit"))
                {
                    WriteLine("Create configmap starterkit");
                    Run("kubectl", "create configmap starterkit --namespace starterkit --from-file=./../components/jenkins/config");
                }

                if (!Kubernetes.CheckIfApplicationExists("jenkins", "starterkit"))
                {
                    WriteLine("Install ldap-ui");
                    Run("helm", "install jenkins --namespace starterkit stable/jenkins " +
                                $"-f ./../components/jenkins/values.yaml --set master.ingress.hostName=jenkins.{rancherUrl}");
                    
                    Run("kubectl", "-n starterkit rollout status deploy/jenkins");
                }

                
            });

            Target("default", DependsOn("checkKubeclt", "installIngress", "installRancher", "configureRancher", "installStarterKit"), () =>
            {
                WriteLine("default");
            });
            
            RunTargetsAndExit(args);
        }
    }
}