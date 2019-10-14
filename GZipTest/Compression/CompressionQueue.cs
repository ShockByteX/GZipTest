using System;
using System.Collections.Generic;
using System.Threading;

namespace GZipTest.Compression
{
    //TODO Less stupid Producer-Consumer pattern implementation possible
    public abstract class CompressionQueue
    {
        protected object _lock = new object();
        protected Queue<CompressionBlock> _queue;
        public int Capacity { get; private set; }
        public bool IsClosed { get; private set; }
        public CompressionQueue(int capacity)
        {
            if (capacity <= 0) throw new ArgumentException("The capacity should be greater than zero.", nameof(capacity));
            Capacity = capacity;
            _queue = new Queue<CompressionBlock>(capacity);
        }
        public void Enqueue(CompressionBlock block)
        {
            lock (_lock) LockedEnqeue(block);
        }
        public bool TryDequeue(out CompressionBlock block)
        {
            bool result = false;
            lock (_lock)
            {
                result = LockedTryDequeue(out block);
                Monitor.Pulse(_lock);
            }
            return result;
        }
        protected abstract void LockedEnqeue(CompressionBlock block);
        protected abstract bool LockedTryDequeue(out CompressionBlock block);
        public void Close()
        {
            if (IsClosed) return;
            lock (_lock)
            {
                IsClosed = true;
                Monitor.PulseAll(_lock);
            }
        }
    }
}
