using System;
using System.IO;
using System.Threading.Tasks;

namespace TarSharp
{
    class TarEntryStream : Stream
    {
        private readonly Stream _archiveStream;
        private readonly long _length;
        private readonly long _roundedLength;
        private long _position;
        private bool _isDisposed;

        public TarEntryStream(Stream archiveStream, long length)
        {
            _archiveStream = archiveStream;
            _length = length;
            _roundedLength = RoundToUpper512(length);
        }

        static long RoundToUpper512(long n)
        {
            long r = n % 512;
            if (r == 0)
                return n;
            return n + (512 - r);
        }

        public override void Flush()
        {
            CheckDisposed();
            _archiveStream.Flush();
        }

        public override Task FlushAsync(System.Threading.CancellationToken cancellationToken)
        {
            CheckDisposed();
            return _archiveStream.FlushAsync(cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            long remainingBytes = _length - _position;
            if (count > remainingBytes)
            {
                if (remainingBytes > 0)
                    count = (int) remainingBytes;
                else
                    count = 0;
            }
            int read = _archiveStream.Read(buffer, offset, count);
            _position += read;
            return read;
        }

        public async override Task<int> ReadAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
        {
            CheckDisposed();
            long remainingBytes = _length - _position;
            if (count > remainingBytes)
            {
                if (remainingBytes > 0)
                    count = (int)remainingBytes;
                else
                    count = 0;
            }
            int read = await _archiveStream.ReadAsync(buffer, offset, count, cancellationToken);
            _position += read;
            return read;
        }

        public override int ReadByte()
        {
            if (_position >= _length)
                return -1;
            int result = _archiveStream.ReadByte();
            _position++;
            return result;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        public override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead
        {
            get { return true; }
        }

        public override bool CanSeek
        {
            get { return false; }
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override long Length
        {
            get { return _length; }
        }

        public override long Position
        {
            get { return _position; }
            set { throw new NotSupportedException(); }
        }

        public override bool CanTimeout
        {
            get { return _archiveStream.CanTimeout; }
        }

        public override int ReadTimeout
        {
            get { return _archiveStream.ReadTimeout; }
            set { _archiveStream.ReadTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return _archiveStream.WriteTimeout; }
            set { _archiveStream.WriteTimeout = value; }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && !_isDisposed)
            {
                int remaining = (int) (_roundedLength - _position);
                if (remaining > 0)
                {
                    byte[] buffer = new byte[remaining];
                    _archiveStream.Read(buffer, 0, remaining);
                }
                _isDisposed = true;
            }
        }

        private void CheckDisposed()
        {
            if (_isDisposed)
                throw new InvalidOperationException("This stream is no longer valid");
        }

        public async Task DisposeAsync()
        {
            Dispose();
            if (!_isDisposed)
            {
                int remaining = (int)(_roundedLength - _position);
                if (remaining > 0)
                {
                    byte[] buffer = new byte[remaining];
                    await _archiveStream.ReadAsync(buffer, 0, remaining);
                }
                _isDisposed = true;
            }
        }
    }
}