using GZipTest.Compression;
using GZipTest.Compression.GZip;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace GZipTest.Compression.GZip.Tests
{
    [TestClass()]
    public class CompressionTests
    {
        public const int BLOCK_SIZE = 1 << 20; // 1 Mb
        private byte[] _smallData, _largeData;
        private Random _rand = new Random();
        private CompressionSettings _settings;
        [TestInitialize]
        public void Initialize()
        {
            _smallData = new byte[1 << 14]; // 16 Kb
            _rand.NextBytes(_smallData);
            _largeData = new byte[1 << 24]; // 16 Mb
            _rand.NextBytes(_largeData);
        }
        [TestMethod()]
        public void GZipCompressionTest()
        {
            MemoryStream dstStream = new MemoryStream();
            _settings = new CompressionSettings(new MemoryStream(_smallData), dstStream, 1 << 20);
            GZipCompressor compressor = new GZipCompressor(_settings);
            compressor.ProgressChanged += Compressor_ProgressChanged;
            compressor.Run();
        }

        private void Compressor_ProgressChanged(int obj)
        {
            Console.WriteLine(obj);
        }

        private void GZipDecompressionTest()
        {

        }

        private void FillArrayWithRandomBytes(byte[] buffer)
        {

        }
    }
}
