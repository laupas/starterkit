using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LauPas.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Installer.Tests
{
    [TestClass]
    public abstract class BaseTest
    {
//        protected Mock<IProcessHelper> ProcessMock { get; set; }

        protected void StartAllServices()
        {
            Starter.Create().AddAssembly<installer.Installer>().AddModule<TestModule>().Build(this.Arguments.ToArray());
        }
        
        protected Mock<T> RegisterMock<T>() where T : class
        {
            return TestModule.RegisterMock<T>();
        }

        protected string CatchConsole(Action action, string stringToSend = null)
        {
            var tempOut = Console.Out;
            var consoleOutput = new StringWriter();
            var consoleIn = new StringReader(stringToSend);
            Console.SetIn(consoleIn);
            Console.SetOut(consoleOutput);
            try
            {
                action();
            }
            finally
            {
                Console.SetOut(tempOut);
            }

            return consoleOutput.ToString();
        }

        protected List<string> Arguments { get; set; } = new List<string>();

        protected T Get<T>()
        {
            return Starter.Get.Resolve<T>();
        }
    }

    public class TestModule : IModule
    {
        private readonly static MockRepository MockRepository = new MockRepository(MockBehavior.Default);
        private readonly static List<Mock> Mocks = new List<Mock>();

        internal static Mock<T> RegisterMock<T>() where T : class
        {
            var mock = MockRepository.Create<T>();
            Mocks.Add(mock);
            return mock;
        }

        public void Extend(IServiceCollection serviceCollection)
        {
            foreach (var mock in Mocks)
            {
                serviceCollection
                    .Where(r => r.ServiceType == mock.GetType().GenericTypeArguments[0])
                    .ToList()
                    .ForEach(c => { serviceCollection.Remove(c); });
                serviceCollection.AddSingleton(mock.GetType().GenericTypeArguments[0], mock.Object);
            }
        }
    }
}