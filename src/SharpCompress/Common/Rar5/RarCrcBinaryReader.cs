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

        public Int64 ReadVInt()
        {
            // https://www.rarlab.com/technote.htm#dtypes
            // vint - variable length integer. Can include one or more bytes, where lower 7 bits of every byte contain 
            // integer data and highest bit in every byte is the continuation flag. 
            // If highest bit is 0, this is the last byte in sequence. So first byte contains 
            // 7 least significant bits of integer and continuation flag. 
            // Second byte, if present, contains next 7 bits and so on.
            // Currently RAR format uses vint to store up to 64 bit integers, 
            // resulting in 10 bytes maximum. This value may be increased 
            // in the future if necessary for some reason.
            Int64 result = 0;
            for (var i = 0; i < 10; i++)
            {
                var current = ReadByte();
                var value = current & 0x7F; // extract first seven bits 
                var shiftedValue = ((Int64)value) << (7 * i); // shift bits on their position
                result += shiftedValue;
                var finish = (current & 0x80) == 0;
                if (finish)
                    break;
            }
            return result;
        }
    }
}