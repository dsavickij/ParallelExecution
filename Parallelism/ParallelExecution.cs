using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parallelism
{
    public static class ParallelExecution
    {
        private const int DEFAULT_MAX_CONCURRENT_THREADS = 32;

        /// <summary>
        /// Builds instance for parallel execution
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="provider">Batch item provider with the skip capability</param>
        /// <param name="processor">Thread-safe single item processor</param>
        /// <param name="maxConcurrentThreads">Maximum number of concurrent threads</param>
        /// <returns></returns>
        public static ParallelExecutionImplementation<TItem, TResult> Build<TItem, TResult>(
            Func<int, Task<IEnumerable<TItem>>> provider,
            Func<TItem, Task<TResult>> processor,
            int maxConcurrentThreads = DEFAULT_MAX_CONCURRENT_THREADS,
            ILogger logger = null)
        {
            return new ParallelExecutionImplementation<TItem, TResult>(
                provider, processor, maxConcurrentThreads, logger);
        }

        /// <summary>
        /// Builds instance for parallel execution
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="items">Items to process</param>
        /// <param name="processor">Thread-safe single item processor</param>
        /// <param name="maxConcurrentThreads">Maximum number of concurrent threads</param>
        /// <returns></returns>
        public static ParallelExecutionImplementation<TItem, TResult> Build<TItem, TResult>(
            IEnumerable<TItem> items,
            Func<TItem, Task<TResult>> processor,
            int maxConcurrentThreads = DEFAULT_MAX_CONCURRENT_THREADS,
            ILogger logger = null)
        {
            _ = items ?? throw new ArgumentNullException(nameof(items));

            return new ParallelExecutionImplementation<TItem, TResult>(
                skip => Task.FromResult(items.ToList().Skip(skip)), processor, maxConcurrentThreads, logger);
        }

        /// <summary>
        /// Builds instance for parallel execution
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="provider">Batch item provider with the skip capability</param>
        /// <param name="processor">Thread-safe single item processor</param>
        /// <param name="maxConcurrentThreads">Maximum number of concurrent threads</param>
        /// <returns></returns>
        public static ParallelExecutionImplementation<TItem> Build<TItem>(
            Func<int, Task<IEnumerable<TItem>>> provider,
            Func<TItem, Task> processor,
            int maxConcurrentThreads = DEFAULT_MAX_CONCURRENT_THREADS,
            ILogger logger = null)
        {
            return new ParallelExecutionImplementation<TItem>(
                provider, processor, maxConcurrentThreads, logger);
        }

        /// <summary>
        /// Builds instance for parallel execution
        /// </summary>
        /// <typeparam name="TItem"></typeparam>
        /// <param name="items">Items to process</param>
        /// <param name="processor">Thread-safe single item processor</param>
        /// <param name="maxConcurrentThreads">Maximum number of concurrent threads</param>
        /// <returns></returns>
        public static ParallelExecutionImplementation<TItem> Build<TItem>(
            IEnumerable<TItem> items,
            Func<TItem, Task> processor,
            int maxConcurrentThreads = DEFAULT_MAX_CONCURRENT_THREADS,
            ILogger logger = null)
        {
            _ = items ?? throw new ArgumentNullException(nameof(items));

            return new ParallelExecutionImplementation<TItem>(
                skip => Task.FromResult(items.ToList().Skip(skip)), processor, maxConcurrentThreads, logger);
        }
    }
}
