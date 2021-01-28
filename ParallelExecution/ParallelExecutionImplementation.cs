using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parallelism
{
    public class ParallelExecutionImplementation<TItem, TResult> : ParallelExecutionBase<TItem, Task<TResult>>
    {
        internal ParallelExecutionImplementation(
            Func<int, Task<IEnumerable<TItem>>> provider,
            Func<TItem, Task<TResult>> processor,
            int maxConcurrentThreads,
            ILogger logger = null) : base(provider, processor, maxConcurrentThreads, logger)
        { }

        public new async Task<IEnumerable<TResult>> RunAsync(CancellationToken cancellationToken = default)
        {
            return (await base.RunAsync(cancellationToken))
                .Select(itemProcessing => itemProcessing.Result);
        }
    }

    public class ParallelExecutionImplementation<TItem> : ParallelExecutionBase<TItem, Task>
    {
        internal ParallelExecutionImplementation(
            Func<int, Task<IEnumerable<TItem>>> provider,
            Func<TItem, Task> processor,
            int maxConcurrentThreads,
            ILogger logger = null) : base(provider, processor, maxConcurrentThreads, logger)
        { }

        public new async Task RunAsync(CancellationToken cancellationToken = default)
        {
            await base.RunAsync(cancellationToken);
        }
    }
}
