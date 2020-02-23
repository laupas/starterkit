using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Installer.Helper;
using Microsoft.Extensions.Logging;

namespace Installer
{
    internal class CommonTargets
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
                this.logger.LogInformation("The kubernetes cluster is not reachable. Please start this container with:");
                this.logger.LogInformation("-v $HOME/.kube:/root/.kube");
                throw;
            }
        }

        internal void HelmRepoUpdate()
        {
            var result = this.processHelper.Read("helm", "repo update");
            if (result.Contains("certificate signed by unknown authority"))
            {
                this.logger.LogInformation("Installing root CAs of helm repos and try again");
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

                this.processHelper.Read("helm", "repo update");
            }
        }

    }
}