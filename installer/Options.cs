using System;
using System.IO;
using System.Linq;
using CommandLine;
using LauPas.Common;
using Microsoft.Extensions.Logging;

namespace Installer
{
    [Singleton]
    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }

        [Option('s', "silent", Required = false, HelpText = "If set silent installation of all tools.")]
        public bool Silent { get; set; }

        [Option('d', "dns", Required = false, HelpText = "Set DNS Name to be used.", Default = "starterkit.devops.family")]
        public string Dns { get; set; }

        // [Option('u', "uninstall", Required = false, HelpText = "Uninstall starterkit.")]
        // public bool Uninstall { get; set; }

        internal void LogOptions(ILogger logger)
        {
            logger.LogInformation("======================================================================");
            logger.LogInformation("Command Line Options");
            logger.LogInformation("======================================================================");
            logger.LogInformation($"Verbose:           {this.Verbose}");
            logger.LogInformation($"Dns:               {this.Dns}");
            logger.LogInformation($"CurrentDirectory:  {Directory.GetCurrentDirectory()}");
        }
        
        internal static Options SetOptions(string[] args)
        {
            var options = new Options();
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
                        Console.WriteLine($"Error: {error.Tag}");
                        Console.ForegroundColor = ConsoleColor.White;
                    });

                    if (errorList.Any())
                    {
                        throw new Exception("Wrong arguments defined.");
                    }
                });
            return options;
        }
    }
}