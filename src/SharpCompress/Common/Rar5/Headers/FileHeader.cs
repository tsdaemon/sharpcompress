using SharpCompress.IO;
using System;
using System.IO;
using System.Text;

namespace SharpCompress.Common.Rar5.Headers
{
    internal class FileHeader : Rar5Header
    {
        private const byte SALT_SIZE = 16;

        private const byte NEWLHD_SIZE = 32;

        protected override void ReadFromReader(MarkingBinaryReader reader)
        {
            FileHeaderFlags = (FileFlags)reader.ReadVInt();
            UnpackedSize = reader.ReadVInt();
            Attributes = reader.ReadVInt();
            if (FileHeaderFlags.HasFlag(FileFlags.TimeFieldIsPresent))
            {
                MTime = reader.ReadUInt32();
                FileLastModifiedTime = Utility.UnixTimeToDateTime(MTime);
            }
            if (FileHeaderFlags.HasFlag(FileFlags.CRC32IsPresent))
            {
                DataCRC32 = reader.ReadUInt32();
            }
            CompressionInformation = reader.ReadVInt();
            HostOS = (HostOS)reader.ReadVInt();

            var nameSize = (int)reader.ReadVInt();
            Name = Encoding.UTF8.GetString(reader.ReadBytes(nameSize));

            uint highCompressedSize = 0;
            uint highUncompressedkSize = 0;
            if (FileFlags.HasFlag(FileFlags.LARGE))
            {
                highCompressedSize = reader.ReadUInt32();
                highUncompressedkSize = reader.ReadUInt32();
            }
            else
            {
                if (lowUncompressedSize  == 0xffffffff)
                {
                    lowUncompressedSize = 0xffffffff;
                    highUncompressedkSize = int.MaxValue;
                }
            }
            CompressedSize = UInt32To64(highCompressedSize, AdditionalSize);
            UncompressedSize = UInt32To64(highUncompressedkSize, lowUncompressedSize);

            nameSize = nameSize > 4 * 1024 ? (short)(4 * 1024) : nameSize;

            byte[] fileNameBytes = reader.ReadBytes(nameSize);

            switch (HeaderType)
            {
                case HeaderType.FileHeader:
                    {
                        if (FileFlags.HasFlag(FileFlags.UNICODE))
                        {
                            int length = 0;
                            while (length < fileNameBytes.Length
                                   && fileNameBytes[length] != 0)
                            {
                                length++;
                            }
                            if (length != nameSize)
                            {
                                length++;
                                FileName = FileNameDecoder.Decode(fileNameBytes, length);
                            }
                            else
                            {
                                FileName = ArchiveEncoding.Decode(fileNameBytes);
                            }
                        }
                        else
                        {
                            FileName = ArchiveEncoding.Decode(fileNameBytes);
                        }
                        FileName = ConvertPath(FileName, HostOS);
                    }
                    break;
                case HeaderType.NewSubHeader:
                    {
                        int datasize = HeaderSize - NEWLHD_SIZE - nameSize;
                        if (FileFlags.HasFlag(FileFlags.SALT))
                        {
                            datasize -= SALT_SIZE;
                        }
                        if (datasize > 0)
                        {
                            SubData = reader.ReadBytes(datasize);
                        }

                        if (NewSubHeaderType.SUBHEAD_TYPE_RR.Equals(fileNameBytes))
                        {
                            RecoverySectors = SubData[8] + (SubData[9] << 8)
                                              + (SubData[10] << 16) + (SubData[11] << 24);
                        }
                    }
                    break;
            }

            if (FileFlags.HasFlag(FileFlags.SALT))
            {
                Salt = reader.ReadBytes(SALT_SIZE);
            }
            if (FileFlags.HasFlag(FileFlags.EXTTIME))
            {
                // verify that the end of the header hasn't been reached before reading the Extended Time.
                //  some tools incorrectly omit Extended Time despite specifying FileFlags.EXTTIME, which most parsers tolerate.
                if (ReadBytes + reader.CurrentReadByteCount <= HeaderSize - 2)
                {
                    ushort extendedFlags = reader.ReadUInt16();
                    FileLastModifiedTime = ProcessExtendedTime(extendedFlags, FileLastModifiedTime, reader, 0);
                    FileCreatedTime = ProcessExtendedTime(extendedFlags, null, reader, 1);
                    FileLastAccessedTime = ProcessExtendedTime(extendedFlags, null, reader, 2);
                    FileArchivedTime = ProcessExtendedTime(extendedFlags, null, reader, 3);
                }
            }
        }
        
        /// <summary>
        /// For file header this is a name of archived file. Forward slash character 
        /// is used as the path separator both for Unix and Windows names. Backslashes are treated 
        /// as a part of name for Unix names and as invalid character for Windows file names. 
        /// Type of name is defined by Host OS field.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Lower 6 bits (0x003f mask) contain the version of compression algorithm, resulting in possible 0 - 63 values. 
        /// Current version is 0.
        /// 
        /// 7th bit (0x0040) defines the solid flag. If it is set, RAR continues to use the compression dictionary 
        /// left after processing preceding files. It can be set only for file headers and is never set for service 
        /// headers.
        /// 
        /// Bits 8 - 10 (0x0380 mask) define the compression method. Currently only values 0 - 5 are used. 0 means 
        /// no compression.
        /// 
        /// Bits 11 - 14 (0x3c00) define the minimum size of dictionary size required to extract data. 
        /// Value 0 means 128 KB, 1 - 256 KB, ..., 14 - 2048 MB, 15 - 4096 MB.
        /// </summary>
        public long CompressionInformation { get; set; }

        /// <summary>
        /// CRC32 of unpacked file or service data. For files split between volumes it contains CRC32 
        /// of file packed data contained in current volume for all file parts except the last.
        /// </summary>
        public uint DataCRC32 { get; set; }

        /// <summary>
        /// File modification time in Unix time format.
        /// </summary>
        public uint MTime { get; set; }
        internal DateTime? FileLastModifiedTime { get; private set; }

        /// <summary>
        /// Operating system specific file attributes in case of file header. 
        /// Might be either used for data specific needs or just reserved and set to 0 for service header.
        /// </summary>
        public long Attributes { get; set; }

        /// <summary>
        /// If flag 0x0008 is set, unpacked size field is still present, but must be ignored and extraction must be 
        /// performed until reaching the end of compression stream. This flag can be set if actual file size is 
        /// larger than reported by OS or if file size is unknown such as for all volumes except last when archiving from 
        /// stdin to multivolume archive.
        /// </summary>
        public long UnpackedSize { get; set; }

        public FileFlags FileHeaderFlags { get; set; }
        
        internal HostOS HostOS { get; private set; }

        private static DateTime? ProcessExtendedTime(ushort extendedFlags, DateTime? time, MarkingBinaryReader reader,
                                                     int i)
        {
            uint rmode = (uint)extendedFlags >> (3 - i) * 4;
            if ((rmode & 8) == 0)
            {
                return null;
            }
            if (i != 0)
            {
                uint DosTime = reader.ReadUInt32();
                time = Utility.DosDateToDateTime(DosTime);
            }
            if ((rmode & 4) == 0)
            {
                time = time.Value.AddSeconds(1);
            }
            uint nanosecondHundreds = 0;
            int count = (int)rmode & 3;
            for (int j = 0; j < count; j++)
            {
                byte b = reader.ReadByte();
                nanosecondHundreds |= (((uint)b) << ((j + 3 - count) * 8));
            }

            //10^-7 to 10^-3
            return time.Value.AddMilliseconds(nanosecondHundreds * Math.Pow(10, -4));
        }

        private static string ConvertPath(string path, HostOS os)
        {
#if NO_FILE
            return path.Replace('\\', '/');
#else
            if (Path.DirectorySeparatorChar == '/')
            {
                return path.Replace('\\', '/');
            }
            else if (Path.DirectorySeparatorChar == '\\')
            {
                return path.Replace('/', '\\');
            }
            return path;
#endif
        }

        internal long DataStartPosition { get; set; }

        internal DateTime? FileCreatedTime { get; private set; }

        internal DateTime? FileLastAccessedTime { get; private set; }

        internal DateTime? FileArchivedTime { get; private set; }

        internal byte RarVersion { get; private set; }

        internal byte PackingMethod { get; private set; }

        internal long CompressedSize { get; private set; }

        internal string FileName { get; private set; }

        internal byte[] SubData { get; private set; }

        internal int RecoverySectors { get; private set; }

        internal byte[] Salt { get; private set; }

        public override string ToString()
        {
            return FileName;
        }

        public Stream PackedStream { get; set; }
    }
}