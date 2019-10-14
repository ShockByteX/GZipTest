using System.IO;
using System.IO.Compression;
using System.Text;

namespace GZipTest.Compression.GZip
{
    public class GZipCompressor : CompressionProcessor
    {
        public GZipCompressor() : base() { }
        protected override void ReadThreadFunction()
        {
            using (Stream stream = GetSourceStream())
            {
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int blockId = 0;
                    int currentProgress = 0;
                    for (long leftData = stream.Length; !_isCanceled && leftData > 0; leftData = stream.Length - stream.Position)
                    {
                        int dataLength = leftData < _blockLength ? (int)leftData : _blockLength;
                        _readQueue.Enqueue(new CompressionBlock(blockId++, reader.ReadBytes(dataLength)));
                        int newProgress = (int)(stream.Position * 100 / stream.Length);
                        if (newProgress > currentProgress)
                        {
                            currentProgress = newProgress;
                            InvokeProgressChanged(currentProgress);
                        }
                    }
                }
            }
        }
        protected override void ProcessThreadFunction()
        {
            CompressionBlock block;
            while (!_isCanceled && _readQueue.TryDequeue(out block)) _writeQueue.Enqueue(Compress(block));
        }
        protected override void WriteThreadFunction()
        {
            using (Stream stream = GetDestinationStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8))
                {
                    CompressionBlock block;
                    while (!_isCanceled && _writeQueue.TryDequeue(out block)) writer.Write(block.Data);
                }
            }
        }
        private CompressionBlock Compress(CompressionBlock block)
        {
            using (MemoryStream ms = new MemoryStream(_blockLength))
            {
                using (GZipStream gz = new GZipStream(ms, CompressionMode.Compress)) gz.Write(block.Data, 0, block.Data.Length);
                block.Data = ms.ToArray();
            }
            return block;
        }
    }
}
