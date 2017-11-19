using System;

namespace SharpCompress.Common.Rar5.Headers
{
    internal enum Rar5HeaderType
    {
        MainHeader = 1,
        FileHeader = 2,
        ServiceHeader = 3,
        ArchiveEncriptionHeader = 4,
        EndOfArchiveHeader = 5
    }
}