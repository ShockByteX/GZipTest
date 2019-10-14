using System.Threading;

namespace GZipTest.Compression
{
    public class CompressionOutputQueue : CompressionQueue
    {
        private int _processedBlocks;
        public CompressionOutputQueue(int capacity) : base(capacity) { }
        protected override void LockedEnqeue(CompressionBlock block)
        {
            while (_queue.Count >= Capacity || _processedBlocks != block.Id)
            {
                if (IsClosed) return;
                Monitor.Wait(_lock);
            }
            _queue.Enqueue(block);
            _processedBlocks++;
            Monitor.PulseAll(_lock);
        }
        protected override bool LockedTryDequeue(out CompressionBlock block)
        {
            block = null;
            while (_queue.Count == 0)
            {
                if (IsClosed) return false;
                Monitor.Wait(_lock);
            }
            block = _queue.Dequeue();
            return true;
        }
    }
}
