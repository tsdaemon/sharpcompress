using SharpCompress.IO;

namespace SharpCompress.Common.Rar5.Headers
{
    internal class EncriptionHeader : Rar5Header
    {
        protected override void ReadFromReader(MarkingBinaryReader reader)
        {
            EncriptionVersion = reader.ReadVInt();
            EncriptionFlags = reader.ReadVInt();
            KdfCount = reader.ReadByte();
            Salt = reader.ReadBytes(16);
            if (PasswordCheckDataIsPresent)
            {
                CheckValue = reader.ReadBytes(12);
            }
        }
        
        /// <summary>
        /// Value used to verify the password validity. First 8 bytes are calculated using additional PBKDF2 rounds, 
        /// 4 last bytes is the additional checksum. Together with the standard header CRC32 we have 64 bit checksum to reliably 
        /// verify this field integrity and distinguish invalid password and damaged data. 
        /// Further details can be found in UnRAR source code.
        /// </summary>
        public byte[] CheckValue { get; set; }

        /// <summary>
        /// Salt value used globally for all encrypted archive headers.
        /// </summary>
        public byte[] Salt { get; private set; }

        /// <summary>
        /// Binary logarithm of iteration number for PBKDF2 function. RAR can refuse to process KDF count exceeding some threshold. 
        /// Concrete value of threshold is a version dependent.
        /// </summary>
        public byte KdfCount { get; private set; }

        /// <summary>
        /// 	0x0001   Password check data is present.
        /// </summary>
        public long EncriptionFlags { get;  private set; }

        public bool PasswordCheckDataIsPresent => EncriptionFlags.HasFlag(0x0001);

        /// <summary>
        /// Version of encryption algorithm. Now only 0 version (AES-256) is supported.
        /// </summary>
        public long EncriptionVersion { get; private set; }
    }
}