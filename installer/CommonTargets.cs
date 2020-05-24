using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using Installer.Helper;
using Microsoft.Extensions.Logging;

namespace Installer
{
    internal class CommonTargets : ICommonTargets
    {
        private readonly Options options;
        private readonly IProcessHelper processHelper;
        private readonly IConsoleService consoleService;
        private readonly ILogger logger;

        public CommonTargets(ILoggerFactory loggerFactory, Options options, IProcessHelper processHelper, IConsoleService consoleService)        
        {
            this.logger = loggerFactory.CreateLogger(nameof(CommonTargets));
            this.options = options;
            this.processHelper = processHelper;
            this.consoleService = consoleService;
        }
        
        public void CheckKubeCtl(InstallerProcess installerProcess)
        {
            try
            {
                try
                {
                    var hostAddress = Dns.GetHostEntry("host.docker.internal").AddressList.First().ToString();
                    this.logger.LogInformation("Running inside Docker");
                    this.logger.LogDebug($"Add {hostAddress} {this.options.Dns} to host");
                    File.AppendAllText("/etc/hosts", $"{hostAddress} {this.options.Dns}{Environment.NewLine}");
                }
                catch
                {
                    installerProcess.InsideDocker = false;
                    this.logger.LogInformation("Running outside Docker");
                }
                var currentCluster = this.processHelper.Read("kubectl", "get nodes").Split(Environment.NewLine)[1];
                this.logger.LogInformation($"The following cluster will be used to install starterkit: {currentCluster}");
            }
            catch
            {
                installerProcess.InsideDocker = true;
                this.logger.LogError("Kube folder is not mapped. Please start this container with:");
                this.logger.LogError("-v $HOME/.kube:/root/.kube");
                throw;
            }
        }

        public void InstallCertificates(InstallerProcess installerProcess)
        {
            this.logger.LogInformation("Validate if you trust all needed helm repos");
            var urls = new List<string>();
            urls.Add("kubernetes-charts.storage.googleapis.com");
            urls.Add("charts.jetstack.io");
            urls.Add("releases.rancher.com");
            foreach (var url in urls)
            {
                this.logger.LogDebug($"Validate {url}");
                
                // check 
                if (!this.SSLTrust(url))
                {
                    this.logger.LogInformation($"It looks you do not trust {url}.");
                    if (this.consoleService.AskFormConfirmation("Should we try to fix this?", "y"))
                    {
                        File.WriteAllText("install.sh",
                            $"openssl s_client -showcerts -verify 5 -connect " + url +
                            ":443 < /dev/null | awk '/BEGIN/,/END/{ if(/BEGIN/){a++}; out=\"cert\"a\".crt\"; print >out}' && for cert in *.crt; do newname=$(openssl x509 -noout -subject -in $cert | sed -n 's/^.*CN=\\(.*\\)$/\\1/; s/[ ,.*]/_/g; s/__/_/g; s/^_//g;p').crt; mv $cert /usr/local/share/ca-certificates/$newname; done && update-ca-certificates");
                        this.processHelper.Run("bash", "install.sh");
                    }
                }
            }
        }

        private bool SSLTrust(string url)
        {
            var trust = false;
            var request = WebRequest.CreateHttp($"https://{url}");
            request.ServerCertificateValidationCallback += (sender, certificate, chain, errors) =>
            {
                trust = errors == SslPolicyErrors.None;
                return true;
            };
            using ((HttpWebResponse) request.GetResponse()) { }

            return trust;
        }

        public void ShowInfo(InstallerProcess installerProcess)
        {
            this.logger.LogInformation("StarterKit installation is done");

            if(installerProcess.ExecutedActions.Contains("InstallRancher"))
            {
                this.logger.LogInformation($"RancherUi: https://{this.options.Dns}");
                this.logger.LogInformation($"Credentials: admin:admin");
            }

            if (installerProcess.ExecutedActions.Contains("InstallLdap"))
            {
                this.logger.LogInformation($"UserManagement: https://ldap.{this.options.Dns}");
                this.logger.LogInformation($"Credentials: cn=admin,dc=starterkit,dc=home:admin");
            }

            if(installerProcess.ExecutedActions.Contains("InstallJenkins"))
            {
                this.logger.LogInformation($"Jenkins: https://jenkins.{this.options.Dns}");
                this.logger.LogInformation($"Credentials: devops:devops");
            }
        }

        public void CheckHelm(InstallerProcess installerProcess)
        {
            if (!this.processHelper.Read("helm", "repo list", throwOnError:false).Contains("stable"))
            {
                this.processHelper.Run("helm", "repo add stable https://kubernetes-charts.storage.googleapis.com/");
            }

            if (!this.processHelper.Read("helm", "repo list", throwOnError:false).Contains("rancher-latest"))
            {
                this.processHelper.Run("helm", "repo add rancher-latest https://releases.rancher.com/server-charts/latest");
            }

            if (!this.processHelper.Read("helm", "repo list", throwOnError:false).Contains("ingress-nginx"))
            {
                this.processHelper.Run("helm", "repo add ingress-nginx https://kubernetes.github.io/ingress-nginx");
            }

            if (!this.processHelper.Read("helm", "repo list", throwOnError:false).Contains("jetstack"))
            {
                this.processHelper.Run("helm", "repo add jetstack https://charts.jetstack.io");
            }

            this.processHelper.Run("helm", "repo update");
            
        }
    }
}