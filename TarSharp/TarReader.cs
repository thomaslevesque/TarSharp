using System;
using System.IO;
using System.Threading.Tasks;

namespace TarSharp
{
    public class TarReader : IDisposable
    {
        private readonly Stream _stream;

        public TarReader(Stream stream)
        {
            _stream = stream;
        }

        public void Dispose()
        {
            if (_currentEntry != null)
            {
                _currentEntry.GetContent().Dispose();
                _currentEntry = null;
            }
            _stream.Dispose();
        }

        private bool _endOfArchive;

        private TarEntryHeader _currentEntry;

        public TarEntryHeader GetNextEntry()
        {
            if (_currentEntry != null)
            {
                _currentEntry.GetContent().Dispose();
                _currentEntry = null;
            }

            int consecutiveEmptyRecords = 0;

            var entry = new TarEntryHeader();

            while (!_endOfArchive)
            {
                var result = entry.Read(_stream);
                if (result == HeaderReadResult.Success)
                    return _currentEntry = entry;

                if (result == HeaderReadResult.EmptyRecord)
                {
                    consecutiveEmptyRecords++;
                    if (consecutiveEmptyRecords > 1)
                    {
                        _endOfArchive = true;
                    }
                }
                else if (result == HeaderReadResult.EndOfStream)
                {
                    _endOfArchive = true;
                }
            }
            return null;
        }

        public async Task<TarEntryHeader> GetNextEntryAsync()
        {
            if (_currentEntry != null)
            {
                var stream = (TarEntryStream) _currentEntry.GetContent();
                await stream.DisposeAsync();
                _currentEntry = null;
            }

            int consecutiveEmptyRecords = 0;

            var entry = new TarEntryHeader();

            while (!_endOfArchive)
            {
                var result = await entry.ReadAsync(_stream);
                if (result == HeaderReadResult.Success)
                    return _currentEntry = entry;

                if (result == HeaderReadResult.EmptyRecord)
                {
                    consecutiveEmptyRecords++;
                    if (consecutiveEmptyRecords > 1)
                    {
                        _endOfArchive = true;
                    }
                }
                else if (result == HeaderReadResult.EndOfStream)
                {
                    _endOfArchive = true;
                }
            }
            return null;
        }
    }
}
