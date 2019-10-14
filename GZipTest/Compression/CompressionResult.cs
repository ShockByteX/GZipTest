using System;
namespace GZipTest.Compression
{
    public enum CompressionResultType : byte { Success, Fail, Cancelled }
    public class CompressionResult
    {
        public readonly CompressionResultType Type;
        public readonly Exception Exception;
        public CompressionResult(CompressionResultType type, Exception exception)
        {
            Type = type;
            Exception = exception;
        }
    }
}
