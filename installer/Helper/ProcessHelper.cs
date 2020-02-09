using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Installer.Helper
{
    internal class ProcessHelper : IProcessHelper
    {
        private ILogger logger;
        private StringBuilder output = new StringBuilder();

        public ProcessHelper(ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger(this.GetType().Name);
        }

        public int Run(string command, string args = null, string workingDirectory = "", bool throwOnError = true, bool noEcho = false)
        {
            return this.RunInternal(command, args, workingDirectory, throwOnError, noEcho).ExitCode;
        }
        
        public string Read(string command, string args = null, string workingDirectory = "", bool throwOnError = true, bool noEcho = false)
        {
            this.RunInternal(command, args, workingDirectory, throwOnError, noEcho);
            return this.output.ToString();
        }

        private Process RunInternal(string command, string args = null, string workingDirectory = "", bool throwOnError = true, bool noEcho = false)
        {
            this.output.Clear();
            var pi = new ProcessStartInfo
            {
                FileName = command,
                Arguments = args,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            var process = new Process() { StartInfo = pi };
            process.OutputDataReceived += (sender, eventArgs) =>
            {
                if (!string.IsNullOrEmpty(eventArgs?.Data))
                {
                    if (!noEcho)
                    {
                        this.logger.LogInformation(eventArgs.Data);
                    }
                    this.output.Append(eventArgs.Data);
                }
            };
            
            process.ErrorDataReceived += (sender, eventArgs) =>
            {
                if (!string.IsNullOrEmpty(eventArgs?.Data))
                {
                    if (!noEcho)
                    {
                        this.logger.LogError(eventArgs.Data);
                    }
                }
            };
            
            this.logger.LogDebug($"Run: {command} {args}");
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            if (throwOnError && process.ExitCode != 0)
            {
                throw new Exception($"Process {command} failed.");
            }
            return process;
        }
    }
}