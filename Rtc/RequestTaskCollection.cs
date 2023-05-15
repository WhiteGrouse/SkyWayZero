using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;

namespace SkyWayZero.Rtc
{
	public record RequestTask<T>(int RequestId, UniTask<T> Task);

	public class RequestTaskCollection<T> : IDisposable
	{
		private volatile bool IsDisposed = false;
		private int _requestId = 0;
		private ConcurrentDictionary<int, UniTaskCompletionSource<T>> _tasks = new();

		public RequestTask<T> Request()
		{
			if (IsDisposed)
				throw new ObjectDisposedException(nameof(RequestTask<T>));

			var requestId = Interlocked.Increment(ref _requestId);
            var tcs = new UniTaskCompletionSource<T>();
			_tasks.TryAdd(requestId, tcs);
            return new RequestTask<T>(requestId, tcs.Task);
		}

		public void Response(int requestId, T result)
		{
            if (!IsDisposed && _tasks.TryRemove(requestId, out var tcs))
            {
	            tcs.TrySetResult(result);
            }
        }

		public void Dispose()
		{
			IsDisposed = true;
			for (int i = 0; i < _tasks.Count; i++)
				_tasks[i].TrySetCanceled();
			_tasks.Clear();
		}
	}
}

