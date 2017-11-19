using System.IO;

namespace SharpCompress.Common.Rar5
{
    public class Rar5SignatureFactory
    {
        public Rar5Signature ReadSignature(Stream stream)
        {
            var reader = new BinaryReader(stream);
            return new Rar5Signature(reader.ReadBytes(8));            
        }
    }
}