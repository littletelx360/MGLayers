using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FFAssetPack {
    public class PakFile {
        public const int INDEX_HEADER_DELIM = 0x11;
        public const int OFFSET_BEGIN = 0x12;

        public const int PREFERRED_BUF_SIZE = 64 * 1024;

        public readonly Stream stream;

        class DataFile {
            public long length;
            public long offset;
        }

        public class PakFileStream : Stream {
            private Stream baseStream;
            private long baseLength;

            private long position;

            public PakFileStream(Stream baseStream, long offset, long length) {
                this.baseStream = baseStream;
                this.baseLength = length;

                baseStream.Position = offset;
            }

            public override void Flush() {
                baseStream.Flush();
            }

            public override long Seek(long offset, SeekOrigin origin) {
                throw new NotSupportedException();
            }

            public override void SetLength(long value) {
                throw new NotSupportedException();
            }

            public override int Read(byte[] buffer, int offset, int count) {
                var remaining = baseLength - position;
                if (remaining <= 0) return 0;
                if (remaining < count) count = (int) remaining;
                var read = baseStream.Read(buffer, offset, count);
                position += read;
                return read;
            }

            public override void Write(byte[] buffer, int offset, int count) {
                throw new NotSupportedException();
            }

            public override bool CanRead { get; } = true;
            public override bool CanSeek { get; } = true;
            public override bool CanWrite { get; } = false;
            public override long Length => baseLength;

            public override long Position {
                get => position;
                set { throw new NotSupportedException(); }
            }
        }

        private Dictionary<string, DataFile> _index = new Dictionary<string, DataFile>();

        public PakFile(Stream stream) {
            this.stream = stream;
        }

        public void read() {
            // read pak file index
            using (var br = new BinaryReader(stream, Encoding.Default, true)) {
                while (true) {
                    var delim = br.PeekChar();
                    if (delim != INDEX_HEADER_DELIM) break;
                    br.ReadInt32(); // eat the header
                    var length = br.ReadInt64();
                    var nameLength = br.ReadInt32();
                    var name = br.ReadString();
                    _index[name] = new DataFile {length = length};
                }
                var offsetDelim = br.ReadInt32();
                if (offsetDelim != OFFSET_BEGIN) throw new DataMisalignedException();
                // read offsets
                foreach (var entry in _index) {
                    entry.Value.offset = br.ReadInt64();
                }
            }
        }

        public PakFileStream getFile(string path) {
            var entry = _index[path];
            return new PakFileStream(stream, entry.offset, entry.length);
        }
    }
}