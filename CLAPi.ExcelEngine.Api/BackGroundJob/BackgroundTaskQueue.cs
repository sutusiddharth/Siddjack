using System.Threading.Channels;

namespace CLAPi.ExcelEngine.Api.BackGroundJob
{
	public class BackgroundTaskQueue(int capacity) : IBackgroundTaskQueue
	{
		private readonly Channel<Func<CancellationToken, Task>> _queue = Channel.CreateBounded<Func<CancellationToken, Task>>(capacity);

		public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
		{
			ArgumentNullException.ThrowIfNull(workItem);
			_queue.Writer.TryWrite(workItem);
		}

		public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
		{
            return await _queue.Reader.ReadAsync(cancellationToken);
        }
	}
}