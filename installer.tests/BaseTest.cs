using System;
using System.Collections.Generic;
using Installer;
using Installer.Helper;
using installer.Targets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace installer.tests
{
    [TestClass]
    public abstract class BaseTest
    {
        private IServiceProvider serviceProvider;
        protected Mock<IProcessHelper> ProcessMock { get; set; }

        protected void Start()
        {
            if (this.serviceProvider == null)
            {
                var options = Options.SetOptions(this.Arguments.ToArray());

                var serviceCollection = new ServiceCollection();
                serviceCollection.AddLogging();

                this.ProcessMock = new Moq.MockRepository(MockBehavior.Loose).Create<IProcessHelper>();

                serviceCollection.AddTransient<IProcessHelper>(provider => this.ProcessMock.Object);

                serviceCollection.AddSingleton<Installer.Installer>();
                serviceCollection.AddSingleton<CommonTargets>();
                serviceCollection.AddSingleton<RancherTargets>();
                serviceCollection.AddSingleton<StarterkitTargets>();
                serviceCollection.AddSingleton<RancherHelper>();
                serviceCollection.AddSingleton<KubernetesHelper>();
                serviceCollection.AddSingleton(options);
                
                this.serviceProvider = serviceCollection.BuildServiceProvider();
            }
        }

        protected List<string> Arguments { get; set; } = new List<string>();

        protected T Get<T>()
        {
            return this.serviceProvider.GetService<T>();
        }
    }
}