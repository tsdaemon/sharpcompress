using System;
using System.IO;
using SharpCompress.IO;
using System.Text;

namespace SharpCompress.Common.Rar5.Headers
{
    /// <summary>
    /// https://www.rarlab.com/technote.htm#arcstruct
    /// </summary>
    internal class Rar5Header
    {
        private void FillBase(Rar5Header baseHeader)
        {
            HeadCRC = baseHeader.HeadCRC;
            HeaderType = baseHeader.HeaderType;
            HeaderFlags = baseHeader.HeaderFlags;
            HeaderSize = baseHeader.HeaderSize;
            DataAreaSize = baseHeader.DataAreaSize;
            ExtraAreaSize = baseHeader.ExtraAreaSize;
            ReadBytes = baseHeader.ReadBytes;
            ArchiveEncoding = baseHeader.ArchiveEncoding;
        }

        internal static Rar5Header Create(Rar5CrcBinaryReader reader, ArchiveEncoding archiveEncoding)
        {
            try
            {
                var header = new Rar5Header {ArchiveEncoding = archiveEncoding};

                reader.Mark();
                header.ReadStartFromReader(reader);
                header.ReadBytes += (int)reader.CurrentReadByteCount;

                return header;
            }
            catch (EndOfStreamException)
            {
                return null;
            }
        }

        private void ReadStartFromReader(Rar5CrcBinaryReader reader)
        {
            HeadCRC = reader.ReadUInt32();
            reader.ResetCrc();
            HeaderSize = (int)reader.ReadVInt();
            HeaderType = (Rar5HeaderType)reader.ReadVInt();
            HeaderFlags = reader.ReadVInt();
            
            if (FlagUtility.HasFlag(HeaderFlags, Flags.ExtraArea))
            {
                ExtraAreaSize = reader.ReadVInt();
            }
            
            if (FlagUtility.HasFlag(HeaderFlags, Flags.DataArea))
            {
                DataAreaSize = reader.ReadVInt();
            }
        }

        protected virtual void ReadFromReader(MarkingBinaryReader reader)
        {
            throw new NotImplementedException();
        }

        internal T PromoteHeader<T>(Rar5CrcBinaryReader reader)
            where T : Rar5Header, new()
        {
            T header = new T();
            header.FillBase(this);

            reader.Mark();
            header.ReadFromReader(reader);
            header.ReadBytes += (int)reader.CurrentReadByteCount;

            var headerSizeDiff = header.HeaderSize - header.ReadBytes;

            if (headerSizeDiff > 0)
            {
                reader.ReadBytes(headerSizeDiff);
            }

            VerifyHeaderCrc(reader.GetCrc());

            return header;
        }

        private void VerifyHeaderCrc(UInt32 crc)
        {
            if (crc != HeadCRC)
            {
                throw new InvalidFormatException("rar header crc mismatch");
            }
        }

        protected virtual void PostReadingBytes(MarkingBinaryReader reader)
        {
        }

        /// <summary>
        /// This is the number of bytes read when reading the header
        /// </summary>
        protected int ReadBytes { get; private set; }

        protected UInt32 HeadCRC { get; private set; }

        internal Rar5HeaderType HeaderType { get; private set; }

        /// <summary>
        /// Untyped flags.  These should be typed when Promoting to another header
        /// </summary>
        protected Int64 HeaderFlags { get; private set; }
        
        /// <summary>
        /// Size of header data starting from Header type field and up to and including the optional extra area. This field must not be longer than 3 bytes in current implementation, resulting in 2 MB maximum header size.
        /// </summary>
        protected int HeaderSize { get; private set; }

        internal ArchiveEncoding ArchiveEncoding { get; private set; }

        protected Int64 ExtraAreaSize { get; private set; }
        
        protected Int64 DataAreaSize { get; private set; }
    }
}