using System;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using NUnit.Framework;

namespace NFramework.Core.Tests
{
    [TestFixture]
    public class SceneLoaderTests
    {
        [Test]
        public void Load_NullOrEmptySceneName_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await SceneLoader.Load(null));
            Assert.ThrowsAsync<ArgumentException>(async () => await SceneLoader.Load(string.Empty));
        }

        [Test]
        public void Unload_NullOrEmptySceneName_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await SceneLoader.Unload(null));
            Assert.ThrowsAsync<ArgumentException>(async () => await SceneLoader.Unload(string.Empty));
        }

        [Test]
        public void Unload_NonExistentOrUnloadedScene_ThrowsInvalidOperationException()
        {
            Assert.ThrowsAsync<InvalidOperationException>(async () => await SceneLoader.Unload("NonExistentScene_12345"));
        }

        [Test]
        public void SetActive_NullOrEmptySceneName_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => SceneLoader.SetActive(null));
            Assert.Throws<ArgumentException>(() => SceneLoader.SetActive(string.Empty));
        }

        [Test]
        public void SetActive_NonExistentOrUnloadedScene_ThrowsInvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() => SceneLoader.SetActive("NonExistentScene_12345"));
        }

#if ADDRESSABLES
        [Test]
        public void LoadAddressables_NullOrEmptyAddress_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await SceneLoader.LoadAddressables(null));
            Assert.ThrowsAsync<ArgumentException>(async () => await SceneLoader.LoadAddressables(string.Empty));
        }

        [Test]
        public void UnloadAddressables_NullOrEmptyAddress_ThrowsArgumentException()
        {
            Assert.ThrowsAsync<ArgumentException>(async () => await SceneLoader.UnloadAddressables(null));
            Assert.ThrowsAsync<ArgumentException>(async () => await SceneLoader.UnloadAddressables(string.Empty));
        }

        [Test]
        public void UnloadAddressables_NotLoadedAddress_ThrowsInvalidOperationException()
        {
            Assert.ThrowsAsync<InvalidOperationException>(async () => await SceneLoader.UnloadAddressables("UnloadedAddress_12345"));
        }
#endif
    }
}
