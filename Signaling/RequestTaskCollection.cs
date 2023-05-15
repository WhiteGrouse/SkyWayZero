using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace SkyWayZero.Signaling
{
	public record RequestTask<T>(string RequestId, UniTask<T> Task);

	public class RequestTaskCollection<T> : IDisposable
	{
		private volatile bool IsDisposed = false;
		private ConcurrentDictionary<string, UniTaskCompletionSource<T>> _tasks = new();

		public RequestTask<T> Request(string requestId)
		{
			if (IsDisposed)
				throw new ObjectDisposedException(nameof(RequestTask<T>));

            var tcs = new UniTaskCompletionSource<T>();
			_tasks.TryAdd(requestId, tcs);
            return new RequestTask<T>(requestId, tcs.Task);
		}

		public void Response(string requestId, T result)
		{
            if (!IsDisposed && _tasks.TryRemove(requestId, out var tcs))
            {
                tcs.TrySetResult(result);
            }
        }

		public void Dispose()
		{
			IsDisposed = true;
			foreach(var requestId in _tasks.Keys)
				_tasks[requestId].TrySetCanceled();
			_tasks.Clear();
		}
	}
}

