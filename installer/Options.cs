using System;
using System.IO;
using System.Linq;
using CommandLine;
using Microsoft.Extensions.Logging;

namespace Installer
{
    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option("try-run", Required = false, HelpText = "Shows only the targets which will be executed.")]
        public bool TryRun { get; set; }

        [Option('d', "dns", Required = false, HelpText = "Set DNS Name to be used.", Default = "starterkit.devops.family")]
        public string Dns { get; set; }

        [Option("filter", Required = false, HelpText = "If set, the defined tasks will be filtered and not all tasks will be executed. (task1 task2 taskn)")]
        public string Filter { get; set; }

        [Option("install-root-certificates", Required = false, HelpText = "Will reinstall the Certification chain for the used helm repos.")]
        public bool InstallRootCertificates { get; set; }

        [Option('f', "full-installation", Required = false, HelpText = "Will install Rancher and all Applications.")]
        public bool FullInstallation { get; set; }
        
        [Option("install-rancher", Required = false, HelpText = "If true, Rancher will be installed on the defined Kubernetes Server")]
        public bool InstallRancher { get; set; }
        
        [Option("install-starterkit", Required = false, HelpText = "If true, Starterkit Applications will be installed on the defined Kubernetes Server")]
        public bool InstallStarterKit { get; set; }

        [Option("uninstall-starterkit", Required = false, HelpText = "If true, Starterkit Applications will be uninstalled on the defined Kubernetes Server")]
        public bool UnInstallStarterKit { get; set; }

        internal void LogOptions(ILogger logger)
        {
            logger.LogInformation("#######################################");
            logger.LogInformation("Command Line Options");
            logger.LogInformation("#######################################");
            logger.LogInformation($"Verbose:           {this.Verbose}");
            logger.LogInformation($"Dns:               {this.Dns}");
            logger.LogInformation($"FullInstallation:  {this.FullInstallation}");
            logger.LogInformation($"InstallRancher:    {this.InstallRancher}");
            logger.LogInformation($"InstallStarterKit: {this.InstallStarterKit}");
            logger.LogInformation($"SslHack:           {this.InstallRootCertificates}");
            logger.LogInformation($"CurrentDirectory:  {Directory.GetCurrentDirectory()}");
        }
        
        internal static Options SetOptions(string[] args)
        {
            Options options = new Options();
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o => 
                {
                    options = o;
                })
                .WithNotParsed(errorList => 
                {
                    errorList.ToList().ForEach(error => 
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Error {error.Tag}");
                        Console.ForegroundColor = ConsoleColor.White;
                    });
                });
            return options;
        }
    }
}