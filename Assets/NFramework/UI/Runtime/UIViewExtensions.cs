using System.Threading;
using Cysharp.Threading.Tasks;

namespace NFramework.UI
{
    public static class UIViewExtensions
    {
        public static UniTask WaitForOpenComplete(this UIView view, CancellationToken cancellationToken = default)
        {
            var ct = cancellationToken == default
                ? view.destroyCancellationToken
                : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, view.destroyCancellationToken).Token;

            return UniTask.WaitUntil(() => view.ViewStatus == EViewStatus.Open || view.ViewStatus == EViewStatus.Closed, cancellationToken: ct);
        }

        public static UniTask WaitForCloseComplete(this UIView view, CancellationToken cancellationToken = default)
        {
            var ct = cancellationToken == default
                ? view.destroyCancellationToken
                : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, view.destroyCancellationToken).Token;

            return UniTask.WaitUntil(() => view.ViewStatus == EViewStatus.Closed, cancellationToken: ct);
        }
    }
}
