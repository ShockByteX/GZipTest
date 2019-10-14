namespace GZipTest.Compression
{
    public class CompressionBlock
    {
        public int Id;
        public byte[] Data;
        public CompressionBlock(int id, byte[] data)
        {
            Id = id;
            Data = data;
        }
    }
}
