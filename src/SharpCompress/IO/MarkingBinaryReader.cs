using System;
using System.IO;
using System.Linq;
using SharpCompress.Converters;

namespace SharpCompress.IO
{
    internal class MarkingBinaryReader : BinaryReader
    {
        public MarkingBinaryReader(Stream stream)
            : base(stream)
        {
        }

        public virtual long CurrentReadByteCount { get; protected set; }

        public virtual void Mark()
        {
            CurrentReadByteCount = 0;
        }

        public override int Read()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int index, int count)
        {
            throw new NotSupportedException();
        }

        public override int Read(char[] buffer, int index, int count)
        {
            throw new NotSupportedException();
        }

        public override bool ReadBoolean()
        {
            return ReadBytes(1).Single() != 0;
        }

        public override byte ReadByte()
        {
            return ReadBytes(1).Single();
        }

        public override byte[] ReadBytes(int count)
        {
            CurrentReadByteCount += count;
            var bytes = base.ReadBytes(count);
            if (bytes.Length != count)
            {
                throw new EndOfStreamException($"Could not read the requested amount of bytes. End of stream reached. Requested: {count} Read: {bytes.Length}");
            }
            return bytes;
        }

        public override char ReadChar()
        {
            throw new NotSupportedException();
        }

        public override char[] ReadChars(int count)
        {
            throw new NotSupportedException();
        }

#if !SILVERLIGHT
        public override decimal ReadDecimal()
        {
            throw new NotSupportedException();
        }
#endif

        public override double ReadDouble()
        {
            throw new NotSupportedException();
        }

        public override short ReadInt16()
        {
            return DataConverter.LittleEndian.GetInt16(ReadBytes(2), 0);
        }

        public override int ReadInt32()
        {
            return DataConverter.LittleEndian.GetInt32(ReadBytes(4), 0);
        }

        public override long ReadInt64()
        {
            return DataConverter.LittleEndian.GetInt64(ReadBytes(8), 0);
        }

        public override sbyte ReadSByte()
        {
            return (sbyte)ReadByte();
        }

        public override float ReadSingle()
        {
            throw new NotSupportedException();
        }

        public override string ReadString()
        {
            throw new NotSupportedException();
        }

        public override ushort ReadUInt16()
        {
            return DataConverter.LittleEndian.GetUInt16(ReadBytes(2), 0);
        }

        public override uint ReadUInt32()
        {
            return DataConverter.LittleEndian.GetUInt32(ReadBytes(4), 0);
        }

        public override ulong ReadUInt64()
        {
            return DataConverter.LittleEndian.GetUInt64(ReadBytes(8), 0);
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