using SharpCompress.IO;

namespace SharpCompress.Common.Rar5.Headers
{
    internal class EndArchiveHeader : Rar5Header
    {
        protected override void ReadFromReader(MarkingBinaryReader reader)
        {
            EndArchiveFlags = (EndOfArchiveFlags)reader.ReadVInt();
            
        }

        internal EndOfArchiveFlags EndArchiveFlags { get; private set; }

        internal bool IsVolume => EndArchiveFlags.HasFlag(EndOfArchiveFlags.ArchiveIsVolume);
    }
}