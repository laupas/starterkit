using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Installer;
using Microsoft.Extensions.Logging;

namespace installer.Helper
{
    internal class Installer
    {
        private readonly Options options;
        private readonly IEnumerable<ITargetsBase> targetsBases;
        private readonly ILogger logger;

        public Installer(ILoggerFactory loggerFactory, Options options, IEnumerable<ITargetsBase> targetsBases)
        {
            this.options = options;
            this.targetsBases = targetsBases;
            this.logger = loggerFactory.CreateLogger(this.GetType().Name);
        }
        
        public void Install()
        {
            this.options.LogOptions(this.logger);
            var targetsToExecute = this.CreateTargetsList();
            this.RunTargets(targetsToExecute.ToList());
        }

        private void RunTargets(IList<Action> targetsToExecute)
        {
            var result = new Dictionary<string, Tuple<Exception, TimeSpan>>();
            var currentAction = string.Empty;
            var sw = new Stopwatch();

            try
            {
                targetsToExecute.ToList().ForEach(target =>
                {
                    currentAction = target.Method.Name;
                    this.logger.LogInformation("======================================================================");
                    this.logger.LogInformation($"==== {currentAction}");
                    this.logger.LogInformation("======================================================================");

                    sw.Start();
                    if (!this.options.TryRun)
                    {
                        target.Invoke();
                    }
                    else
                    {
                        this.logger.LogInformation($"Try-Run is active. Target {currentAction} is not executed.");
                    }
                    sw.Stop();
                    result.Add($"{currentAction}", new Tuple<Exception, TimeSpan>(null, sw.Elapsed));
                });
            }
            catch (Exception e)
            {
                result.Add($"{currentAction}", new Tuple<Exception, TimeSpan>(e, sw.Elapsed));
                this.logger.LogError(e.Message);
            }
            finally
            {
                this.PrintResult(targetsToExecute, result);

                if (result.Any(a => a.Value.Item1 != null))
                {
                    throw new InstallerException($"Execution failed.");
                }
            }
        }

        private void PrintResult(IList<Action> targetsToExecute, Dictionary<string, Tuple<Exception, TimeSpan>> result)
        {
            this.logger.LogInformation("======================================================================");
            this.logger.LogInformation("==== Result");
            this.logger.LogInformation("======================================================================");
            foreach (var action in result)
            {
                var resultString = action.Value.Item1 == null ? "Succeeded" : "Failed";
                this.logger.LogInformation($"{action.Key}, Duration: {Math.Round((double) action.Value.Item2.Milliseconds / 1000, 2)} s, Result: {resultString}");
            }

            this.logger.LogInformation("======================================================================");
            var endResult = result.All(a => a.Value.Item1 == null);
            var endResultString = endResult ? "Succeeded" : "Failed";
            this.logger.LogInformation($"Total: {targetsToExecute.Count()} Targets, Duration: {Math.Round((double) result.Sum(r => r.Value.Item2.Milliseconds) / 1000, 2)}s , Result: {endResultString}");
            this.logger.LogInformation("======================================================================");
        }

        internal IEnumerable<Action> CreateTargetsList()
        {
            var targetsToExecute = new List<Action>();

            var targetDic = new Dictionary<int, Action>();
            this.targetsBases.ToList().SelectMany(tb => tb.DefineTargetToExecute()).ToList().ForEach(tb =>
            {
                targetDic.Add(tb.Key, tb.Value);
                this.logger.LogDebug($"Order: {tb.Key} Target: {tb.Value.Method.Name}");
            });
            targetsToExecute.AddRange(targetDic.OrderBy(kvp => kvp.Key).Select(kvp => kvp.Value));
            
            if ( ! string.IsNullOrEmpty(this.options.Filter))
            {
                var tempList = this.options.Filter.Split(' ').ToList();
                targetsToExecute = targetsToExecute
                    .Where(target => tempList.Any(t => t == target.Method.Name)).ToList();
            }

            this.logger.LogDebug($"The following Targets will be executed:");
            foreach (var target in targetsToExecute)
            {
                this.logger.LogTrace($"{target.Method.Name}");
            }
            
            return targetsToExecute;
        }
    }
}