using System.Collections.Generic;

namespace Installer.Helper
{
    public class InstallerProcess
    {
        private readonly List<string> executedActions = new List<string>();

        public void AddExecutedTask(string name)
        {
            this.executedActions.Add(name);
        }

        public IReadOnlyCollection<string> ExecutedActions => this.executedActions;
        public bool InsideDocker { get; set; }
    }
}