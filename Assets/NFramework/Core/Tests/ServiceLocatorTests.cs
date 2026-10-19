using System;
using NUnit.Framework;

namespace NFramework.Core.Tests
{
    [TestFixture]
    public class ServiceLocatorTests
    {
        private interface ITestService
        {
            string GetValue();
        }

        private class TestService : ITestService
        {
            public string GetValue() => "test_value";
        }

        [SetUp]
        public void SetUp()
        {
            ServiceLocator.ClearAll();
        }

        [TearDown]
        public void TearDown()
        {
            ServiceLocator.ClearAll();
        }

        [Test]
        public void Register_NullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => ServiceLocator.Register<ITestService>(null));
        }

        [Test]
        public void RegisterAndGet_ReturnsRegisteredInstance()
        {
            var service = new TestService();
            ServiceLocator.Register<ITestService>(service);

            ITestService resolved = ServiceLocator.Get<ITestService>();

            Assert.AreSame(service, resolved);
            Assert.AreEqual("test_value", resolved.GetValue());
        }

        [Test]
        public void Register_DuplicateType_ThrowsInvalidOperationException()
        {
            var service1 = new TestService();
            var service2 = new TestService();

            ServiceLocator.Register<ITestService>(service1);

            Assert.Throws<InvalidOperationException>(() => ServiceLocator.Register<ITestService>(service2));
        }

        [Test]
        public void Unregister_RegisteredType_RemovesService()
        {
            var service = new TestService();
            ServiceLocator.Register<ITestService>(service);

            ServiceLocator.Unregister<ITestService>();

            Assert.IsFalse(ServiceLocator.TryGet<ITestService>(out _));
            Assert.Throws<InvalidOperationException>(() => ServiceLocator.Get<ITestService>());
        }

        [Test]
        public void Unregister_NotRegistered_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => ServiceLocator.Unregister<ITestService>());
        }

        [Test]
        public void Get_NotRegistered_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => ServiceLocator.Get<ITestService>());
        }

        [Test]
        public void TryGet_NotRegistered_ReturnsFalseAndNull()
        {
            bool found = ServiceLocator.TryGet(out ITestService service);

            Assert.IsFalse(found);
            Assert.IsNull(service);
        }

        [Test]
        public void ClearAll_RemovesAllServices()
        {
            ServiceLocator.Register<ITestService>(new TestService());

            ServiceLocator.ClearAll();

            Assert.IsFalse(ServiceLocator.TryGet<ITestService>(out _));
        }
    }
}
