using System;
using System.IO;
using SharpCompress.Compressors.Rar;
using SharpCompress.IO;

namespace SharpCompress.Common.Rar5 {
    internal class Rar5CrcBinaryReader : MarkingBinaryReader {
        
        private UInt32 currentCrc;

        public Rar5CrcBinaryReader(Stream stream) : base(stream)
        {
        }

        public UInt32 GetCrc() 
        {
            return ~currentCrc;
        }

        public void ResetCrc()
        {
            currentCrc = 0xffffffff;
        }

        protected void UpdateCrc(byte b) 
        {
            currentCrc = RarCRC.CheckCrc(currentCrc, b);
        }

        protected byte[] ReadBytesNoCrc(int count)
        {
            return base.ReadBytes(count);
        }

        public override byte[] ReadBytes(int count)
        {
            var result = base.ReadBytes(count);
            currentCrc = RarCRC.CheckCrc(currentCrc, result, 0, result.Length);
            return result;
        }
    }
}