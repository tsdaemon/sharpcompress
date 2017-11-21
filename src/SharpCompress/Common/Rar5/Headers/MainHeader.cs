using SharpCompress.IO;

namespace SharpCompress.Common.Rar5.Headers
{
    internal class MainHeader : Rar5Header
    {
        protected override void ReadFromReader(MarkingBinaryReader reader)
        {
            ArchiveHeaderFlag = (ArchiveFlags)reader.ReadVInt();
            if (ArchiveHeaderFlag.HasFlag(ArchiveFlags.VolumeNumberFieldIsPresented))
            {
                VolumeNumber = reader.ReadVInt();
            }
        }

        public long VolumeNumber { get; private set; }

        public ArchiveFlags ArchiveHeaderFlag { get; private set; }

        public bool IsVolume => ArchiveHeaderFlag.HasFlag(ArchiveFlags.ArchiveIsVolume);
        
        public bool IsLocked => ArchiveHeaderFlag.HasFlag(ArchiveFlags.LockedArchive);
        
        public bool RecoveryIsPresented => ArchiveHeaderFlag.HasFlag(ArchiveFlags.RecoveryRecordIsPresented);
    }
}