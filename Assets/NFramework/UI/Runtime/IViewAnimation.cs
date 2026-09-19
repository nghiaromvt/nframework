using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace NFramework.UI
{
    public interface IOpenViewAnimation
    {
        bool BlockInteract { get; }
        UniTask PlayOpen(CancellationToken cancellationToken = default);
    }

    public interface ICloseViewAnimation
    {
        bool BlockInteract { get; }
        UniTask PlayClose(CancellationToken cancellationToken = default);
    }
}
