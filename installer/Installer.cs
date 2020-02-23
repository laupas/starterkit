using System;
using Microsoft.Extensions.Logging;

namespace Installer
{
    internal class Installer
    {
        private readonly Options options;
        private readonly CommonTargets commonTargets;
        private readonly RancherTargets rancherTargets;
        private readonly StarterkitTargets starterKitTargets;
        private readonly ILogger logger;

        public Installer(ILoggerFactory loggerFactory, Options options, CommonTargets commonTargets, RancherTargets rancherTargets, StarterkitTargets starterKitTargets)
        {
            this.options = options;
            this.commonTargets = commonTargets;
            this.rancherTargets = rancherTargets;
            this.starterKitTargets = starterKitTargets;
            this.logger = loggerFactory.CreateLogger(this.GetType().Name);
        }
        
        public void Install()
        {
            this.options.LogOptions(this.logger);

            this.commonTargets.CheckKubeCtl();
            this.options.Ask("Is the kubernetes cluster above the right one?", noAction:() => throw new Exception("Wrong Cluster"));
            this.options.LogOptions(this.logger);
            this.options.Ask("Are the options above correct?", noAction: () => throw new Exception("Wrong Options"));
            
            this.options.Ask("Update the helm repos before installing?", "n", () =>
            {
                this.commonTargets.HelmRepoUpdate();
            });
            
            this.options.Ask("Install Rancher?", "y", () =>
            {
                this.options.Ask("Install Ingress service?", "y", () =>
                {
                    this.rancherTargets.InstallIngress();
                });

                this.rancherTargets.InstallRancher();
                this.rancherTargets.WaitUntilIsUpAndReady();
                this.rancherTargets.Login();
                this.rancherTargets.ChangePassword();
                this.rancherTargets.SetServerUrl();
            });

            this.options.Ask("Install Starterkit?", "y", () =>
            {
                this.starterKitTargets.InstallStarterkitCommon();

                this.options.Ask("Install LDAP?", "y", () =>
                {
                    this.starterKitTargets.InstallLdap();
                });

                this.options.Ask("Install Jenkins?", "y", () =>
                {
                    this.starterKitTargets.InstallJenkins();
                });
            });


            this.logger.LogInformation("StarterKit installation is done");

            this.logger.LogInformation($"RancherUi: https://{this.options.Dns}");
            this.logger.LogInformation($"Credentials: admin:admin");

            this.logger.LogInformation($"UserManagement: https://ldap.{this.options.Dns}");
            this.logger.LogInformation($"Credentials: cn=admin,dc=starterkit,dc=home:admin");
            this.logger.LogInformation($"Jenkins: https://jenkins.{this.options.Dns}");
            this.logger.LogInformation($"Credentials: devops:devops");
            
        }


    }
}