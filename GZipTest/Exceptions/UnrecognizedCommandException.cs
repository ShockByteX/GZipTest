using System;

namespace GZipTest.Exceptions
{
    public class UnrecognizedCommandException : Exception
    {
        public UnrecognizedCommandException(string command) : base($"Unrecognized command '{command}'") { }
    }
}
