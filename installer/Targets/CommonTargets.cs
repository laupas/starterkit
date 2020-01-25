using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Installer;
using Installer.Helper;
using Microsoft.Extensions.Logging;
using SimpleExec;

namespace installer.Targets
{
    public class CommonTargets : TargetsBase
    {
        public CommonTargets(ILoggerFactory loggerFactory, Options options, KubernetesHelper kubernetesHelper, RancherHelper rancherHelper) : base(loggerFactory, options, kubernetesHelper, rancherHelper)
        {
        }

        public override IDictionary<int, string> DefineTargetToExecute()
        {
            var targets = new Dictionary<int, string>();
            
            targets.Add(0, "checkKubeclt");

            if(this.Options.InstallRootCertificates)
            {
                targets.Add(1, "installCertificates");
            }

            targets.Add(99, "showInfo");

            return targets;        
        }
        
        public override void CreateTargets()
        {
            Bullseye.Targets.Target("installCertificates",  () =>
            {
                this.WriteHeader("Installing root CAs of helm repos");
                var urls = new List<string>();
                urls.Add("kubernetes-charts.storage.googleapis.com");
                urls.Add("charts.jetstack.io");
                urls.Add("releases.rancher.com");
                foreach (var url in urls)
                {
                    File.WriteAllText("install.sh",
                        $"openssl s_client -showcerts -verify 5 -connect " + url +
                        ":443 < /dev/null | awk '/BEGIN/,/END/{ if(/BEGIN/){a++}; out=\"cert\"a\".crt\"; print >out}' && for cert in *.crt; do newname=$(openssl x509 -noout -subject -in $cert | sed -n 's/^.*CN=\\(.*\\)$/\\1/; s/[ ,.*]/_/g; s/__/_/g; s/^_//g;p').crt; mv $cert /usr/local/share/ca-certificates/$newname; done && update-ca-certificates");
                    Command.Run("bash", "install.sh");
                }
            });
            
            Bullseye.Targets.Target("checkKubeclt", () =>
            {
                try
                {
                    Command.Run("kubectl", "get nodes");
                    try
                    {
                        var hostAddress = Dns.GetHostEntry("host.docker.internal").AddressList.First().ToString();
                        this.Logger.LogInformation("Running inside Docker");
                        this.Logger.LogInformation($"Add {hostAddress} {this.Options.Dns} to host");
                        File.AppendAllText("/etc/hosts", $"{hostAddress} {this.Options.Dns}{Environment.NewLine}");
                    }
                    catch
                    {
                        this.Logger.LogInformation("Running outside Docker");
                    }
                }
                catch
                {
                    this.Logger.LogInformation("Kube folder is not mapped. Please start this container with:");
                    this.Logger.LogInformation("-v $HOME/.kube:/root/.kube");
                    throw;
                }
            });
        }
    }
}