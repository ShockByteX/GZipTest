using System.IO;
using System.IO.Compression;
using System.Text;
using GZipTest.Helpers;

namespace GZipTest.Compression.GZip
{
    public class GZipDecompressor : CompressionProcessor
    {
        private readonly byte[] HEADER_BYTES = new byte[] { 0x1f, 0x8b, 0x08, 0x00, 0x00, 0x00, 0x00, 0x00 }; // ID1, ID2, CM, FLG, MTIME
        private byte[] _largeBuffer;
        public GZipDecompressor() : base() { }
        protected override void ReadThreadFunction()
        {
            using (Stream stream = GetSourceStream())
            {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8))
                {
                    int blockId = 0;
                    int currentProgress = 0;
                    while (!_isCanceled && stream.Position < stream.Length)
                    {
                        _readQueue.Enqueue(new CompressionBlock(blockId++, reader.ReadBytes(GetNextBlockLength(reader))));
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
            while (!_isCanceled && _readQueue.TryDequeue(out CompressionBlock block)) _writeQueue.Enqueue(Decompress(block));
        }
        protected override void WriteThreadFunction()
        {
            using (Stream stream = GetDestinationStream())
            {
                while (!_isCanceled && _writeQueue.TryDequeue(out CompressionBlock block)) stream.Write(block.Data, 0, block.Data.Length);
            }
        }
        private int GetNextBlockLength(BinaryReader reader)
        {
            Stream stream = reader.BaseStream;
            long lastPosition = stream.Position;
            if (!reader.ReadBytes(HEADER_BYTES.Length).SequenceEqual(HEADER_BYTES)) throw new InvalidDataException("Source file data is not correct.");
            if (_largeBuffer == null) _largeBuffer = new byte[_blockLength << 1]; // In rare cases GZip can compress data to a larger length. So we take buffer twice as much.
            int length = stream.Read(_largeBuffer, 0, _largeBuffer.Length);
            int offset = _largeBuffer.Find(HEADER_BYTES, 0, length);
            stream.Position = lastPosition;
            if (offset == -1) return (int)(stream.Length - stream.Position);
            return offset + HEADER_BYTES.Length;
        }
        private CompressionBlock Decompress(CompressionBlock block)
        {
            byte[] buffer = new byte[_blockLength];
            using (MemoryStream ms = new MemoryStream(block.Data))
            {
                using (GZipStream gz = new GZipStream(ms, CompressionMode.Decompress)) block.Data = buffer.Take(0, gz.Read(buffer, 0, buffer.Length));
            }
            return block;
        }
    }
}
