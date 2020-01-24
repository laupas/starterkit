using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bullseye;
using installer.Helper;
using installer.Targets;
using Microsoft.Extensions.Logging;

namespace Installer
{
    public class Installer
    {
        private readonly Options options;
        private readonly CommonTargets commonTargets;
        private readonly RancherTargets rancherTargets;
        private readonly StarterkitTargets starterkitTargets;
        private readonly ILogger logger;

        public Installer(ILoggerFactory loggerFactory, Options options, CommonTargets commonTargets, RancherTargets rancherTargets, StarterkitTargets starterkitTargets)
        {
            this.options = options;
            this.commonTargets = commonTargets;
            this.rancherTargets = rancherTargets;
            this.starterkitTargets = starterkitTargets;
            this.logger = loggerFactory.CreateLogger(this.GetType().Name);
        }
        
        public void Install()
        {
            this.options.LogOptions(this.logger);

            this.commonTargets.CreateTargets();
            this.rancherTargets.CreateTargets();
            this.starterkitTargets.CreateTargets();
            
            var targets = new List<string>();
            if(this.options.Tasks == null || this.options.Tasks.Any())
            {
                targets.AddRange(this.options.Tasks);
            }
            
            this.CreateTargets(targets);
            
            if (!this.options.TryRun)
            {
                Targets.RunTargetsAndExit(targets);
            }
        }

        internal void CreateTargets(List<string> targets)
        {
            var targetDic = new Dictionary<int, string>();
            this.commonTargets.DefineTargetToExecute().ToList().ForEach(kvp => targetDic.Add(kvp.Key, kvp.Value));
            this.rancherTargets.DefineTargetToExecute().ToList().ForEach(kvp => targetDic.Add(kvp.Key, kvp.Value));
            this.starterkitTargets.DefineTargetToExecute().ToList().ForEach(kvp => targetDic.Add(kvp.Key, kvp.Value));
            targets.AddRange(targetDic.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value));
            
            this.logger.LogTrace($"The following tasks will be executed:");
            foreach (var target in targets)
            {
                this.logger.LogTrace($"{target}");
            }

        }
    }
}