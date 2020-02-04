using System;
using System.Collections.Generic;
using Installer;
using installer.Helper;
using Installer.Helper;
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

        protected void StartAllServices()
        {
            if (this.serviceProvider == null)
            {
                var options = Options.SetOptions(this.Arguments.ToArray());

                var serviceCollection = new ServiceCollection();
                serviceCollection.AddLogging();

                this.ProcessMock = new MockRepository(MockBehavior.Loose).Create<IProcessHelper>();

                serviceCollection.AddTransient<IProcessHelper>(provider => this.ProcessMock.Object);

                serviceCollection.AddSingleton<Helper.Installer>();
                serviceCollection.AddSingleton<ITargetsBase, CommonTargets>();
                serviceCollection.AddSingleton<ITargetsBase, RancherTargets>();
                serviceCollection.AddSingleton<ITargetsBase, StarterkitTargets>();
                serviceCollection.AddSingleton<IKubernetesHelper, KubernetesHelper>();
                serviceCollection.AddSingleton(options);
                
                this.serviceProvider = serviceCollection.BuildServiceProvider();
            }
        }

        protected void StartSpecial(Action<ServiceCollection, MockRepository> starter)
        {
            if (this.serviceProvider == null)
            {
                var serviceCollection = new ServiceCollection();
                serviceCollection.AddLogging();
                var mockRepository = new MockRepository(MockBehavior.Loose);
                starter.Invoke(serviceCollection, mockRepository);
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