using System.Collections.Generic;
using System.IO;
using System.Linq;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using SharpCompress.Common.Rar;
using SharpCompress.Common.Rar.Headers;
using SharpCompress.Common.Rar5;
using SharpCompress.Compressors.Rar;
using SharpCompress.IO;
using SharpCompress.Readers;
using SharpCompress.Readers.Rar;

namespace SharpCompress.Archives.Rar5
{
    public class Rar5Archive : AbstractArchive<Rar5ArchiveEntry, RarVolume>
    {
        internal Unpack Unpack { get; } = new Unpack();

#if !NO_FILE

        /// <summary>
        /// Constructor with a FileInfo object to an existing file.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="options"></param>
        internal Rar5Archive(FileInfo fileInfo, ReaderOptions options)
            : base(ArchiveType.Rar, fileInfo, options)
        {
        }

        protected override IEnumerable<RarVolume> LoadVolumes(FileInfo file)
        {
            return RarArchiveVolumeFactory.GetParts(file, ReaderOptions);
        }
#endif

        /// <summary>
        /// Takes multiple seekable Streams for a multi-part archive
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="options"></param>
        internal Rar5Archive(IEnumerable<Stream> streams, ReaderOptions options)
            : base(ArchiveType.Rar, streams, options)
        {
        }

        protected override IEnumerable<Rar5ArchiveEntry> LoadEntries(IEnumerable<RarVolume> volumes)
        {
            return Rar5ArchiveEntryFactory.GetEntries(this, volumes);
        }

        protected override IEnumerable<RarVolume> LoadVolumes(IEnumerable<Stream> streams)
        {
            return RarArchiveVolumeFactory.GetParts(streams, ReaderOptions);
        }

        protected override IReader CreateReaderForSolidExtraction()
        {
            var stream = Volumes.First().Stream;
            stream.Position = 0;
            return RarReader.Open(stream, ReaderOptions);
        }

        public override bool IsSolid => Volumes.First().IsSolidArchive;

        #region Creation

#if !NO_FILE

        /// <summary>
        /// Constructor with a FileInfo object to an existing file.
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="options"></param>
        public static Rar5Archive Open(string filePath, ReaderOptions options = null)
        {
            filePath.CheckNotNullOrEmpty("filePath");
            return new Rar5Archive(new FileInfo(filePath), options ?? new ReaderOptions());
        }

        /// <summary>
        /// Constructor with a FileInfo object to an existing file.
        /// </summary>
        /// <param name="fileInfo"></param>
        /// <param name="options"></param>
        public static Rar5Archive Open(FileInfo fileInfo, ReaderOptions options = null)
        {
            fileInfo.CheckNotNull("fileInfo");
            return new Rar5Archive(fileInfo, options ?? new ReaderOptions());
        }
#endif

        /// <summary>
        /// Takes a seekable Stream as a source
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="options"></param>
        public static Rar5Archive Open(Stream stream, ReaderOptions options = null)
        {
            stream.CheckNotNull("stream");
            return Open(stream.AsEnumerable(), options ?? new ReaderOptions());
        }

        /// <summary>
        /// Takes multiple seekable Streams for a multi-part archive
        /// </summary>
        /// <param name="streams"></param>
        /// <param name="options"></param>
        public static Rar5Archive Open(IEnumerable<Stream> streams, ReaderOptions options = null)
        {
            streams.CheckNotNull("streams");
            return new Rar5Archive(streams, options ?? new ReaderOptions());
        }

#if !NO_FILE
        public static bool IsRar5File(string filePath)
        {
            return IsRar5File(new FileInfo(filePath));
        }

        public static bool IsRar5File(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
            {
                return false;
            }
            using (Stream stream = fileInfo.OpenRead())
            {
                return IsRar5File(stream);
            }
        }
#endif
        
        public static bool IsRar5File(Stream stream, ReaderOptions options = null)
        {
            try
            {
                var signatureFactory = new Rar5SignatureFactory();
                var signature = signatureFactory.ReadSignature(stream);
                return signature != null && signature.IsValid();
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}