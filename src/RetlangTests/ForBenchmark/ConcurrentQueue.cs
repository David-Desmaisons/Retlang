using Retlang.Core;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace RetlangTests.ForBenchmark
{
    /// <summary>
    /// Default implementation.
    /// </summary>
    public class ConcurrentQueue : IQueue
    {
        private readonly IExecutor _executor;

        private CancellationTokenSource _cancellationTokenSource;

        private BlockingCollection<Action> _actions = new BlockingCollection<Action>(new ConcurrentQueue<Action>());

        ///<summary>
        /// Default queue with custom executor
        ///</summary>
        ///<param name="executor"></param>
        public ConcurrentQueue(IExecutor executor)
        {
            _executor = executor;
        }

        ///<summary>
        /// Default queue with default executor
        ///</summary>
        public ConcurrentQueue() 
            : this(new DefaultExecutor())
        {
        }

        /// <summary>
        /// Enqueue action.
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action)
        {
            _actions.Add(action);
        }

        /// <summary>
        /// Execute actions until stopped.
        /// </summary>
        public void Run()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            try 
            {
                foreach (var action in _actions.GetConsumingEnumerable(_cancellationTokenSource.Token)) 
                {
                    action();
                }
            }
            catch(OperationCanceledException) 
            {
            }

        }

        /// <summary>
        /// Stop consuming actions.
        /// </summary>
        public void Stop()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}