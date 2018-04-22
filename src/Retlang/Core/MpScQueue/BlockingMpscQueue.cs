using System;
using System.Threading;

//Inspired by and adapted from http://www.1024cores.net/
//Ref: http://www.1024cores.net/home/lock-free-algorithms/queues/non-intrusive-mpsc-node-based-queue

namespace Retlang.Core.MpScQueue
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BlockingMpscQueue<T> where T: class
    {
        private readonly SimplifiedEventCount _simplifiedEventCount = new SimplifiedEventCount();
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;

        private Node<T> _head;
        private Node<T> _tail;
        
        /// <summary>
        /// 
        /// </summary>
        public BlockingMpscQueue()
        {
            _cancellationToken = _cancellationTokenSource.Token;
            var empty = new Node<T>();
            _head = empty;
            _tail = empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Enqueue(T item)
        {
            _cancellationToken.ThrowIfCancellationRequested();

            var newItem = new Node<T>(item);
            var prev = Interlocked.Exchange(ref _head, newItem);
            prev.Next = newItem;

            _simplifiedEventCount.NotifyOne();
        }

        /// <summary>
        /// 
        /// </summary>
        public void CompleteAdding()
        {
            _cancellationTokenSource.Cancel();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onItem"></param>
        public void OnElements(Action<T> onItem) 
        {
            while (true)
            {
                var value = Next();
                onItem(value);
            }
        }

        private T Next()
        {
            var spin = new SpinWait();
            while (_tail.Next == null && !spin.NextSpinWillYield)
            {
                spin.SpinOnce();
            }

            if (_tail.Next != null)
            {
                _tail = _tail.Next;
                return _tail.Value;
            }

            while (true)
            {
                _simplifiedEventCount.PrepareWait();

                if (_tail.Next != null) 
                {
                    _simplifiedEventCount.RetireWait();
                    _tail = _tail.Next;
                    return _tail.Value;
                }

                _simplifiedEventCount.Wait(_cancellationToken);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
            _simplifiedEventCount.Dispose();
        }
    }
}
