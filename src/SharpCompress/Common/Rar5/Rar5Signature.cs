using System.Linq;

namespace SharpCompress.Common.Rar5
{
    public class Rar5Signature
    {
        private byte[] _bytes;
        private static readonly byte[] VALID_SIGNATURE = {0x52, 0x61, 0x72, 0x21, 0x1A, 0x07, 0x01, 0x00};

        internal Rar5Signature(byte[] bytes)
        {
            _bytes = bytes;
        }

        public bool IsValid() => _bytes.SequenceEqual(VALID_SIGNATURE);
    }
}