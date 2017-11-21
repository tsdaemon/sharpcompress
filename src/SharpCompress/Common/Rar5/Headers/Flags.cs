using System;

namespace SharpCompress.Common.Rar5.Headers
{
    internal enum Flags
    {
        ExtraArea = 0x0001,
        DataArea = 0x0002,
        BlocksWithUnknownType = 0x0004,
        DataAreaIsContinuingFromPreviousVolume = 0x0008,
        DataAreaIsContinuingInNextVolume = 0x0010,
        BlockDependsOnPrecedingBlock = 0x0020,
        PreserveChildBlock = 0x0040
    }
    
    internal enum Rar5HeaderType
    {
        MainHeader = 1,
        FileHeader = 2,
        ServiceHeader = 3,
        EncriptionHeader = 4,
        EndOfArchiveHeader = 5
    }

    internal enum EndOfArchiveFlags
    {
        ArchiveIsVolume = 0x0001
    }
    
    [Flags]
    internal enum ArchiveFlags
    {
        ArchiveIsVolume = 0x0001,
        VolumeNumberFieldIsPresented = 0x0002,
        SolidArchive = 0x0004,
        RecoveryRecordIsPresented = 0x0008,
        LockedArchive = 0x0010
    }
    
    [Flags]
    internal enum FileFlags
    {
        DirectoryFileSystemObject = 0x0001,
        TimeFieldIsPresent = 0x0002,
        CRC32IsPresent = 0x0004,
        UnpackedSizeIsUnknown = 0x0008
    }
    
    internal enum HostOS
    {
        Win32 = 0,
        Unix = 1
    }
}