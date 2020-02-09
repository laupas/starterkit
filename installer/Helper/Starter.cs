using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Scrutor;

namespace Installer.Helper
{
    internal static class Starter
    {
        private static IServiceProvider serviceProvider;
        public static void Build(string[] args, Action<IServiceCollection> extend = null)
        {
            var options = Options.SetOptions(args);
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder
                .AddFilter(level =>
                {
                    if (options.Verbose)
                    {
                        return level >= LogLevel.Trace;
                    }

                    return level >= LogLevel.Information;
                })
            );
            
            var assemblies = new List<Assembly>()
            {
                Assembly.GetCallingAssembly(),
                Assembly.GetEntryAssembly(),
                Assembly.GetExecutingAssembly()
            }.Distinct();

            var namespaces = new []{"Installer"};
            serviceCollection.Scan(scan =>
                scan.FromAssemblies(assemblies)
                    .AddClasses(c => c.WithoutAttribute<Singleton>().InNamespaces(namespaces))
                    .UsingRegistrationStrategy(RegistrationStrategy.Append)
                    .AsSelfWithInterfaces()
                    .AsSelf()
                    .WithTransientLifetime());

            serviceCollection.Scan(scan =>
                scan.FromAssemblies(assemblies)
                    .AddClasses(c => c.WithAttribute<Singleton>().InNamespaces(namespaces))
                    .UsingRegistrationStrategy(RegistrationStrategy.Append)
                    .AsSelfWithInterfaces()
                    .AsSelf()
                    .WithSingletonLifetime());
            
            serviceCollection.AddSingleton(options);
            extend?.Invoke(serviceCollection);
            
            serviceProvider = serviceCollection.BuildServiceProvider();
            
        }

        public static T Get<T>()
        {
            return serviceProvider.GetService<T>();
        }
        
    }
}