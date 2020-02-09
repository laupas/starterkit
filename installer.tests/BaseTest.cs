using System.Collections.Generic;
using System.Linq;
using Installer.Helper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Installer.Tests
{
    [TestClass]
    public abstract class BaseTest
    {
//        protected Mock<IProcessHelper> ProcessMock { get; set; }
        private readonly MockRepository mockRepository = new MockRepository(MockBehavior.Default);
        private readonly List<Mock> mocks = new List<Mock>();

        protected void StartAllServices()
        {
            Starter.Build(this.Arguments.ToArray(), collection =>
            {
                foreach (var mock in this.mocks)
                {
                   collection
                       .Where(r => r.ServiceType == mock.GetType().GenericTypeArguments[0])
                       .ToList()
                       .ForEach(c =>
                       {
                           collection.Remove(c);
                       });
                   collection.AddSingleton(mock.GetType().GenericTypeArguments[0], mock.Object);
                }
            });
        }
        
        protected Mock<T> RegisterMock<T>() where T : class
        {
            var mock = this.mockRepository.Create<T>();
            this.mocks.Add(mock);
            return mock;
        }

        protected List<string> Arguments { get; set; } = new List<string>();

        protected T Get<T>()
        {
            return Starter.Get<T>();
        }
    }
}