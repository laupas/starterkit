using System;
using Installer;
using Installer.Helper;
using Microsoft.Extensions.Logging;

namespace installer
{
    internal class Installer
    {
        private readonly Options options;
        private readonly ICommonTargets commonTargets;
        private readonly IRancherTargets rancherTargets;
        private readonly IStarterkitTargets starterkitTargets;
        private readonly IConsoleService consoleService;
        private readonly ILogger logger;

        public Installer(ILoggerFactory loggerFactory, Options options, ICommonTargets commonTargets,
            IRancherTargets rancherTargets, IStarterkitTargets starterkitTargets, IConsoleService consoleService)
        {
            this.options = options;
            this.commonTargets = commonTargets;
            this.rancherTargets = rancherTargets;
            this.starterkitTargets = starterkitTargets;
            this.consoleService = consoleService;
            this.logger = loggerFactory.CreateLogger(this.GetType().Name);
        }
        
        public void Install()
        {
            this.options.LogOptions(this.logger);
            var installerProcess = new InstallerProcess();

            try
            {
                this.commonTargets.CheckKubeCtl(installerProcess);
                if (!this.consoleService.AskFormConfirmation("Is the cluster above the correct one?", "y"))
                {
                    return;
                }
                this.commonTargets.InstallCertificates(installerProcess);
                this.commonTargets.CheckHelm(installerProcess);
                
                if (this.consoleService.AskFormConfirmation("Should Rancher be installed?", "y"))
                {
                    this.rancherTargets.InstallRancher(installerProcess);
                }

                if (this.consoleService.AskFormConfirmation("Should Jenkins be installed?", "y"))
                {
                    this.starterkitTargets.InstallLdap(installerProcess);
                    this.starterkitTargets.InstallJenkins(installerProcess);
                }

                this.commonTargets.ShowInfo(installerProcess);
            }
            catch (Exception e)
            {
                this.logger.LogError(e.Message);
            }
        }

    }
}