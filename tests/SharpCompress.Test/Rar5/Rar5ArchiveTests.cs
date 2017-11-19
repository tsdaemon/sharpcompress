using System.IO;
using System.Linq;
using SharpCompress.Archives;
using SharpCompress.Archives.Rar;
using SharpCompress.Common;
using SharpCompress.Readers;
using Xunit;

namespace SharpCompress.Test.Rar
{
    public class Rar5ArchiveTests : ArchiveTests
    {
        [Fact]
        public void Rar5_ArchiveFileRead()
        {
            ArchiveFileRead("Rar5.rar");
        }
    }
}
