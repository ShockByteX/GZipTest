using System;
using System.IO;
using System.Threading;
using GZipTest.Helpers;

namespace GZipTest.Compression
{
    public abstract class CompressionProcessor
    {
        private Thread _readThread, _writeThread;
        private Thread[] _processThreads;
        protected CompressionQueue _readQueue, _writeQueue;
        protected int _blockLength;
        protected bool _isCanceled;
        public Func<Stream> GetSourceStream { get; private set; }
        public Func<Stream> GetDestinationStream { get; private set; }
        public bool IsRunning { get; private set; }
        public CompressionProcessor()
        {
            _readQueue = new CompressionInputQueue(Environment.ProcessorCount);
            _writeQueue = new CompressionOutputQueue(Environment.ProcessorCount);
        }
        public void Run(Func<Stream> getSrcStream, Func<Stream> getDstStream, int blockLength)
        {
            if (IsRunning) return;
            IsRunning = true;
            new Thread(() =>
            {
                GetSourceStream = getSrcStream;
                GetDestinationStream = getDstStream;
                _blockLength = blockLength;
                Exception exception = null;
                _readThread = ThreadHelper.RunGuarded(ReadThreadFunction, (ex) =>
                {
                    _isCanceled = true;
                    exception = ex;
                });
                _writeThread = ThreadHelper.RunGuarded(WriteThreadFunction, (ex) =>
                {
                    _isCanceled = true;
                    exception = ex;
                });
                _processThreads = new Thread[Environment.ProcessorCount];
                for (int i = 0; i < _processThreads.Length; i++)
                {
                    _processThreads[i] = ThreadHelper.RunGuarded(ProcessThreadFunction, (ex) =>
                    {
                        _isCanceled = true;
                        exception = ex;
                    });
                }
                ThreadHelper.WaitThreads(_readThread);
                _readQueue.Close();
                ThreadHelper.WaitThreads(_processThreads);
                _writeQueue.Close();
                ThreadHelper.WaitThreads(_writeThread);
                CompressionResultType crType = CompressionResultType.Success;
                if (exception == null) crType = _isCanceled ? CompressionResultType.Cancelled : CompressionResultType.Success;
                else crType = CompressionResultType.Fail;
                IsRunning = false;
                ProcessingFinished?.Invoke(this, new CompressionResult(crType, exception));
            }).Start();
        }
        public void Cancel()
        {
            if (!IsRunning) return;
            _isCanceled = true;
            _writeQueue.Close();
            _readQueue.Close();
        }
        protected abstract void ProcessThreadFunction();
        protected abstract void ReadThreadFunction();
        protected abstract void WriteThreadFunction();
        protected void InvokeProgressChanged(int value) => ProgressChanged?.Invoke(this, value);
        public EventHandler<int> ProgressChanged;
        public EventHandler<CompressionResult> ProcessingFinished;
    }
}
