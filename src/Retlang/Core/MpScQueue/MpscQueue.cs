using System;

namespace Retlang.Core.MpScQueue 
{
    /// <summary>
    /// 
    /// </summary>
    public class MpscQueue : IQueue
    {
        private readonly BlockingMpscQueue<Action> _queue;
        private readonly IExecutor _executor;

        /// <summary>
        /// 
        /// </summary>
        public MpscQueue(IExecutor executor)
        {
            _executor = executor;
            _queue = new BlockingMpscQueue<Action>();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MpscQueue(): this(new DefaultExecutor())
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="action"></param>
        public void Enqueue(Action action) => _queue.Enqueue(action);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Run()
        {
            _queue.OnElements(act => _executor.Execute(act));
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Stop() => _queue.CompleteAdding();
    }
}
