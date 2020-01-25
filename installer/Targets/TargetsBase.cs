using System.Collections.Generic;
using Installer;
using Installer.Helper;
using Microsoft.Extensions.Logging;

namespace installer.Targets
{
    public abstract class TargetsBase
    {
        protected Options Options { get; }
        protected IProcessHelper ProcessHelper { get; }
        protected KubernetesHelper KubernetesHelper { get; }
        protected RancherHelper RancherHelper { get; }
        protected ILogger Logger { get; }

        protected TargetsBase(ILoggerFactory loggerFactory, Options options, IProcessHelper processHelper, KubernetesHelper kubernetesHelper, RancherHelper rancherHelper)
        {
            this.Options = options;
            this.ProcessHelper = processHelper;
            this.KubernetesHelper = kubernetesHelper;
            this.RancherHelper = rancherHelper;
            this.Logger = loggerFactory.CreateLogger(this.GetType().Name);
        }

        public abstract IDictionary<int, string> DefineTargetToExecute();
        public abstract void CreateTargets();
        
        protected void WriteHeader(string name)
        {
            this.Logger.LogInformation("####################################");
            this.Logger.LogInformation(name);
            this.Logger.LogInformation("####################################");
        }

    }
}