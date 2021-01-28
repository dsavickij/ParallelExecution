using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parallelism
{
    public class ParallelExecutionBase<TItem, TResultTask> where TResultTask: Task
    {
        private readonly int _maxConcurrentThreads;
        private readonly Func<int, Task<IEnumerable<TItem>>> _provider;
        private readonly Func<TItem, TResultTask> _processor;

        protected ILogger Logger;

        internal ParallelExecutionBase(
            Func<int, Task<IEnumerable<TItem>>> provider,
            Func<TItem, TResultTask> processor,
            int maxConcurrentThreads,
            ILogger logger = null)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _processor = processor ?? throw new ArgumentNullException(nameof(processor));
            _maxConcurrentThreads = (maxConcurrentThreads <= 0)
                ? throw new ArgumentException($"{nameof(maxConcurrentThreads)} cannot be equal or less than 0")
                : maxConcurrentThreads;
            Logger = logger;
        }

        protected async Task<List<TResultTask>> RunAsync(CancellationToken cancellationToken = default)
        {
            Logger?.LogDebug($"Started parallel execution of item processing at {DateTime.UtcNow}");
            var result = await ExecuteParallelProcessingAsync(cancellationToken);
            Logger?.LogDebug($"Finished parallel execution of item processing at {DateTime.UtcNow}");

            return result;
        }

        protected async IAsyncEnumerable<TItem> GetItemsAsync()
        {
            var skip = 0;

            while (true)
            {
                var batch = await _provider(skip);

                if (batch is null)
                    throw new ArgumentNullException(null, $"Item provider returned uninitialized object");

                if (batch.Any())
                {
                    Logger?.LogDebug($"Received {batch.Count()} items. Processing");

                    foreach (var item in batch)
                        yield return item;

                    skip += batch.Count();
                    continue;
                }

                Logger?.LogDebug($"No more items are available for processing. Total items processed: {skip}");
                yield break;
            }
        }

        protected async Task<List<TResultTask>> ExecuteParallelProcessingAsync(
            CancellationToken cancellationToken = default) 
        {
            var (activeWorkers, allWorkers) = (new List<TResultTask>(), new List<TResultTask>());

            await foreach (var item in GetItemsAsync())
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await ManageDegreeOfParallelism((activeWorkers, allWorkers), _processor(item));
            }

            await Task.WhenAll(allWorkers);
            cancellationToken.ThrowIfCancellationRequested();

            return allWorkers;
        }

        protected async Task ManageDegreeOfParallelism(
            (List<TResultTask> activeWorkers, List<TResultTask> allWorkers) workers, TResultTask newWorker)
        {
            workers.activeWorkers.Add(newWorker);
            workers.allWorkers.Add(newWorker);

            if (workers.activeWorkers.Count == _maxConcurrentThreads)
            {
                await Task.WhenAny(workers.activeWorkers);
                workers.activeWorkers.RemoveAll(worker => worker.IsCompleted);
            }
        }
    }
}