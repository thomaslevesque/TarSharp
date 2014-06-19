using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TarSharp
{
    public class TarEntryHeader
    {
        public string Name { get; private set; }
        public int Mode { get; private set; }
        public int OwnerId { get; private set; }
        public string OwnerName { get; private set; }
        public int GroupId { get; private set; }
        public string GroupName { get; private set; }
        public long Size { get; private set; }
        public DateTime ModificationDateUtc { get; private set; }
        public int Checksum { get; private set; }
        public TarEntryType Type { get; private set; }
        public string LinkedFileName { get; private set; }

        internal HeaderReadResult Read(Stream stream)
        {
            var buffer = stream.ReadBytes(512);
            return Read(buffer, stream);
        }

        internal async Task<HeaderReadResult> ReadAsync(Stream stream)
        {
            var buffer = await stream.ReadBytesAsync(512);
            return Read(buffer, stream);
        }

        private HeaderReadResult Read(byte[] buffer, Stream stream)
        {
            if (buffer.Length < 512)
                return HeaderReadResult.EndOfStream;

            Name = ReadText(buffer, 0, 100);
            if (string.IsNullOrEmpty(Name) && buffer.All(b => b == 0))
            {
                return HeaderReadResult.EmptyRecord;
            }

            Mode = ParseOctalInt32(ReadText(buffer, 100, 8));
            OwnerId = ParseOctalInt32(ReadText(buffer, 108, 8));
            GroupId = ParseOctalInt32(ReadText(buffer, 116, 8));
            Size = ParseOctalInt64(ReadText(buffer, 124, 12));
            long timestamp = ParseOctalInt64(ReadText(buffer, 136, 12));
            ModificationDateUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
            Checksum = ParseOctalInt32(ReadText(buffer, 148, 8));
            Type = GetEntryType(buffer[156]);
            LinkedFileName = ReadText(buffer, 157, 100);

            string magic = ReadText(buffer, 257, 6);
            if (magic == "ustar")
            {
                OwnerName = ReadText(buffer, 265, 32);
                GroupName = ReadText(buffer, 297, 32);
                string prefix = ReadText(buffer, 345, 155);
                if (!string.IsNullOrEmpty(prefix))
                {
                    if (!prefix.EndsWith("/"))
                        prefix += "/";
                    Name = (prefix + Name.TrimStart('/'));
                }
            }
            _content = new TarEntryStream(stream, Size);
            return HeaderReadResult.Success;
        }

        static TarEntryType GetEntryType(byte b)
        {
            char c = (char)b;
            switch (c)
            {
                case '1':
                    return TarEntryType.HardLink;
                case '2':
                    return TarEntryType.SymbolicLink;
                case '3':
                    return TarEntryType.CharacterDeviceNode;
                case '4':
                    return TarEntryType.BlockDeviceNode;
                case '5':
                    return TarEntryType.Directory;
                case '6':
                    return TarEntryType.FifoNode;
                case '7':
                    return TarEntryType.ContiguousFile;
            }
            return TarEntryType.Normal;
        }

        static int ParseOctalInt32(string octal)
        {
            return Convert.ToInt32(octal.TrimEnd(' ', '\0'), 8);
        }

        static long ParseOctalInt64(string octal)
        {
            return Convert.ToInt64(octal.TrimEnd(' ', '\0'), 8);
        }

        static string ReadText(byte[] buffer, int offset, int length)
        {
            return Encoding.UTF8.GetString(buffer, offset, length).TrimEnd('\0');
        }

        private TarEntryStream _content;
        public Stream GetContent()
        {
            return _content;
        }
    }

    internal enum HeaderReadResult
    {
        Success,
        EndOfStream,
        EmptyRecord
    }
}
