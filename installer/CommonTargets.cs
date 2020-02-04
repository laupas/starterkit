using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Installer;
using installer.Helper;
using Installer.Helper;
using Microsoft.Extensions.Logging;

namespace installer
{
    internal class CommonTargets : ITargetsBase
    {
        private readonly Options options;
        private readonly IProcessHelper processHelper;
        private readonly ILogger logger;

        public CommonTargets(ILoggerFactory loggerFactory, Options options, IProcessHelper processHelper)        
        {
            this.logger = loggerFactory.CreateLogger(typeof(CommonTargets).Name);
            this.options = options;
            this.processHelper = processHelper;
        }

        public IDictionary<int, Action> DefineTargetToExecute()
        {
            var targets = new Dictionary<int, Action>
            {
                {0, this.CheckKubeCtl}
            };

            if(this.options.InstallRootCertificates)
            {
                targets.Add(1, this.InstallCertificates);
            }

            targets.Add(99, this.ShowInfo);

            return targets;        
        }

        internal void CheckKubeCtl()
        {
            try
            {
                this.processHelper.Run("kubectl", "get nodes");
                try
                {
                    var hostAddress = Dns.GetHostEntry("host.docker.internal").AddressList.First().ToString();
                    this.logger.LogInformation("Running inside Docker");
                    this.logger.LogInformation($"Add {hostAddress} {this.options.Dns} to host");
                    File.AppendAllText("/etc/hosts", $"{hostAddress} {this.options.Dns}{Environment.NewLine}");
                }
                catch
                {
                    this.logger.LogInformation("Running outside Docker");
                }
            }
            catch
            {
                this.logger.LogInformation("Kube folder is not mapped. Please start this container with:");
                this.logger.LogInformation("-v $HOME/.kube:/root/.kube");
                throw;
            }
        }

        internal void InstallCertificates()
        {
            this.logger.LogInformation("Installing root CAs of helm repos");
            var urls = new List<string>();
            urls.Add("kubernetes-charts.storage.googleapis.com");
            urls.Add("charts.jetstack.io");
            urls.Add("releases.rancher.com");
            foreach (var url in urls)
            {
                File.WriteAllText("install.sh",
                    $"openssl s_client -showcerts -verify 5 -connect " + url +
                    ":443 < /dev/null | awk '/BEGIN/,/END/{ if(/BEGIN/){a++}; out=\"cert\"a\".crt\"; print >out}' && for cert in *.crt; do newname=$(openssl x509 -noout -subject -in $cert | sed -n 's/^.*CN=\\(.*\\)$/\\1/; s/[ ,.*]/_/g; s/__/_/g; s/^_//g;p').crt; mv $cert /usr/local/share/ca-certificates/$newname; done && update-ca-certificates");
                this.processHelper.Run("bash", "install.sh");
            }
        }

        internal void ShowInfo()
        {
            this.logger.LogInformation("StarterKit installation is done");

            if(this.options.InstallRancher || this.options.FullInstallation)
            {
                this.logger.LogInformation($"RancherUi: https://{this.options.Dns}");
                this.logger.LogInformation($"Credentials: admin:admin");
            }

            if(this.options.InstallStarterKit || this.options.FullInstallation)
            {
                this.logger.LogInformation($"UserManagement: https://ldap.{this.options.Dns}");
                this.logger.LogInformation($"Credentials: cn=admin,dc=starterkit,dc=home:admin");
                this.logger.LogInformation($"Jenkins: https://jenkins.{this.options.Dns}");
                this.logger.LogInformation($"Credentials: devops:devops");
            }
        }
    }
}