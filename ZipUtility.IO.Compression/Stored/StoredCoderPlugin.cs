using System;

namespace ZipUtility.IO.Compression.Stored
{
    public abstract class StoredCoderPlugin
        : ICompressionCoder
    {
        private class DummyOption
            : ICoderOption
        {
        }

        public CompressionMethodId CompressionMethodId => CompressionMethodId.Stored;

        public ICoderOption DefaultOption => new DummyOption();

        public ICoderOption GetOptionFromGeneralPurposeFlag(Boolean bit1, Boolean bit2) => new DummyOption();
    }
}
