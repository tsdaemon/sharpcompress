using System.IO;
using SharpCompress.Common.Rar.Headers;
using SharpCompress.Common.Rar5;
using SharpCompress.IO;
using SharpCompress.Readers;
using Xunit;

namespace SharpCompress.Test.Rar5
{
    /// <summary>
    /// Summary description for RarFactoryReaderTest
    /// </summary>
    public class Rar5SinatureTest : TestBase
    {
        private Rar5SignatureFactory signatureFactory;

        public Rar5SinatureTest()
        {
            ResetScratch();
            signatureFactory = new Rar5SignatureFactory();
        }

        [Fact]
        public void ReadSignature_CorrectFile()
        {
            var signature = signatureFactory.ReadSignature(GetReaderStream("Rar5.rar"));
            Assert.True(signature.IsValid());
        }
        
        [Fact]
        public void ReadSignature_IncorrectFile()
        {
            var signature = signatureFactory.ReadSignature(GetReaderStream("Rar.rar"));
            Assert.False(signature.IsValid());
        }

        private FileStream GetReaderStream(string testArchive)
        {
            return new FileStream(Path.Combine(TEST_ARCHIVES_PATH, testArchive),
                                  FileMode.Open);
        }
    }
}
