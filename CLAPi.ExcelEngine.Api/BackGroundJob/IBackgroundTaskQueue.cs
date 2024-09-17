﻿namespace CLAPi.ExcelEngine.Api.BackGroundJob
{
	public interface IBackgroundTaskQueue
	{
		void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);
		Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
	}
}