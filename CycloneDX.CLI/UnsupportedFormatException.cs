using System;

namespace CycloneDX.CLI
{
    public class UnsupportedFormatException : Exception
    {
        public UnsupportedFormatException(string message) : base(message) {}
    }
}