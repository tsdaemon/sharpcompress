using System;
using System.Collections.Generic;
using System.IO;
using SharpCompress.IO;
using SharpCompress.Readers;

namespace SharpCompress.Common.Rar5.Headers
{
    internal class Rar5HeaderFactory
    {
        private const int MAX_SFX_SIZE = 0x80000 - 16; //archive.cpp line 136

        internal Rar5HeaderFactory(StreamingMode mode, ReaderOptions options)
        {
            StreamingMode = mode;
            Options = options;
        }

        private ReaderOptions Options { get; }
        internal StreamingMode StreamingMode { get; }
        internal bool IsEncrypted { get; private set; }

        internal IEnumerable<Rar5Header> ReadHeaders(Stream stream)
        {
            if (Options.LookForHeader)
            {
                stream = CheckSFX(stream);
            }

            Rar5Header header;
            while ((header = ReadNextHeader(stream)) != null)
            {
                yield return header;
                if (header.HeaderType == Rar5HeaderType.EndOfArchiveHeader)
                {
                    yield break; // the end?
                }
            }
        }

        private Stream CheckSFX(Stream stream)
        {
            RewindableStream rewindableStream = GetRewindableStream(stream);
            stream = rewindableStream;
            BinaryReader reader = new BinaryReader(rewindableStream);
            try
            {
                int count = 0;
                while (true)
                {
                    byte firstByte = reader.ReadByte();
                    if (firstByte == 0x52)
                    {
                        MemoryStream buffer = new MemoryStream();
                        byte[] nextThreeBytes = reader.ReadBytes(3);
                        if ((nextThreeBytes[0] == 0x45)
                            && (nextThreeBytes[1] == 0x7E)
                            && (nextThreeBytes[2] == 0x5E))
                        {
                            //old format and isvalid
                            buffer.WriteByte(0x52);
                            buffer.Write(nextThreeBytes, 0, 3);
                            rewindableStream.Rewind(buffer);
                            break;
                        }
                        byte[] secondThreeBytes = reader.ReadBytes(3);
                        if ((nextThreeBytes[0] == 0x61)
                            && (nextThreeBytes[1] == 0x72)
                            && (nextThreeBytes[2] == 0x21)
                            && (secondThreeBytes[0] == 0x1A)
                            && (secondThreeBytes[1] == 0x07)
                            && (secondThreeBytes[2] == 0x00))
                        {
                            //new format and isvalid
                            buffer.WriteByte(0x52);
                            buffer.Write(nextThreeBytes, 0, 3);
                            buffer.Write(secondThreeBytes, 0, 3);
                            rewindableStream.Rewind(buffer);
                            break;
                        }
                        buffer.Write(nextThreeBytes, 0, 3);
                        buffer.Write(secondThreeBytes, 0, 3);
                        rewindableStream.Rewind(buffer);
                    }
                    if (count > MAX_SFX_SIZE)
                    {
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                if (!Options.LeaveStreamOpen)
                {
#if NET35
                    reader.Close();
#else
                    reader.Dispose();
#endif
                }
                throw new InvalidFormatException("Error trying to read rar signature.", e);
            }
            return stream;
        }

        private RewindableStream GetRewindableStream(Stream stream)
        {
            RewindableStream rewindableStream = stream as RewindableStream;
            if (rewindableStream == null)
            {
                rewindableStream = new RewindableStream(stream);
            }
            return rewindableStream;
        }

        private Rar5Header ReadNextHeader(Stream stream)
        {
#if !NO_CRYPTO
            var reader = new Rar5CryptoBinaryReader(stream, Options.Password);

            if (IsEncrypted)
            {
                if (Options.Password == null)
                {
                    throw new CryptographicException("Encrypted Rar archive has no password specified.");
                }
                reader.SkipQueue();
                byte[] salt = reader.ReadBytes(16);
                reader.InitializeAes(salt);
            }
#else
            var reader = new RarCrcBinaryReader(stream);

#endif

            Rar5Header header = Rar5Header.Create(reader, Options.ArchiveEncoding);
            if (header == null)
            {
                return null;
            }
            switch (header.HeaderType)
            {
                case Rar5HeaderType.EncriptionHeader:
                {
                    var eh = header.PromoteHeader<EncriptionHeader>(reader);
                    IsEncrypted = true;
                    return eh;
                }
                case Rar5HeaderType.MainHeader:
                {
                    var ah = header.PromoteHeader<MainHeader>(reader);
                    return ah;
                }
                case Rar5HeaderType.FileHeader:
                {
                    FileHeader fh = header.PromoteHeader<FileHeader>(reader);
                    switch (StreamingMode)
                    {
                        case StreamingMode.Seekable:
                            {
                                fh.DataStartPosition = reader.BaseStream.Position;
                                reader.BaseStream.Position += fh.CompressedSize;
                            }
                            break;
                        case StreamingMode.Streaming:
                            {
                                var ms = new ReadOnlySubStream(reader.BaseStream, fh.CompressedSize);
                                if (fh.Salt == null)
                                {
                                    fh.PackedStream = ms;
                                }
                                else
                                {
#if !NO_CRYPTO
                                    fh.PackedStream = new RarCryptoWrapper(ms, Options.Password, fh.Salt);
#else
                            throw new NotSupportedException("RarCrypto not supported");
#endif
                                }
                            }
                            break;
                        default:
                            {
                                throw new InvalidFormatException("Invalid StreamingMode");
                            }
                    }
                    return fh;
                }
                case Rar5HeaderType.EndOfArchiveHeader:
                {
                    return header.PromoteHeader<EndArchiveHeader>(reader);
                }
                default:
                {
                    throw new InvalidFormatException("Invalid Rar Header: " + header.HeaderType);
                }
            }
        }
    }
}