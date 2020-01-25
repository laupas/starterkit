using System;
using Installer;
using Installer.Helper;
using installer.Targets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace installer.Helper
{
    internal static class Starter
    {
        private static IServiceProvider serviceProvider;
        public static void Build(string[] args)
        {
            if (serviceProvider == null)
            {
                var options = Options.SetOptions(args);
                var serviceCollection = new ServiceCollection();
                serviceCollection.AddLogging(builder => builder.AddProvider(new CustomLoggerProvider())
                    .AddFilter(level =>
                    {
                        if (options.Verbose)
                        {
                            return level >= LogLevel.Trace;
                        }
                        else
                        {
                            return level >= LogLevel.Information;
                        }
                    })
                );
                
                serviceCollection.AddTransient<IProcessHelper, ProcessHelper>();
                serviceCollection.AddSingleton<Installer.Installer>();
                serviceCollection.AddSingleton<CommonTargets>();
                serviceCollection.AddSingleton<RancherTargets>();
                serviceCollection.AddSingleton<StarterkitTargets>();
                serviceCollection.AddSingleton<RancherHelper>();
                serviceCollection.AddSingleton<KubernetesHelper>();
                serviceCollection.AddSingleton(options);
                serviceProvider = serviceCollection.BuildServiceProvider();
            }
        }

        public static T Get<T>()
        {
            return serviceProvider.GetService<T>();
        }
        
    }
}