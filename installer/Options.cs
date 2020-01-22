using System.Collections.Generic;
using CommandLine;

namespace Installer
{
    public class Options
    {
        [Option("task-overwrite", Required = false, HelpText = "If set, this tasks will be executed. !!!Some other options will be ignored. (task1 task2 taskn)")]
        public IEnumerable<string> Tasks { get; set; }
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
        [Option('d', "dns", Required = false, HelpText = "Set DNS Name to be used.")]
        public string Dns { get; set; }
        [Option("ssl-hack", Required = false, HelpText = "Will reinstall the Certification chain for the used helm repos.", Default = false)]
        public bool SslHack { get; set; }
        [Option("install-rancher", Required = false, HelpText = "If true, Rancher will be installed on the defined Kubernetes Server", Default = true)]
        public bool? InstallRancher { get; set; }
        [Option("install-starterkit", Required = false, HelpText = "If true, Starterkit Applications will be installed on the defined Kubernetes Server", Default = true)]
        public bool InstallStarterKit { get; set; }
        [Option("dynamic-dns", Required = false, HelpText = "Which dynamic DNS Server should be used.", Default = "nip.io")]
        public string DynamicDns { get; set; }
    }
}