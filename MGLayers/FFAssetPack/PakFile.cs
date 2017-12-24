using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LZ4;

namespace FFAssetPack {
    public class PakFile : IDisposable {
        public const int INDEX_HEADER_DELIM = 0x11;
        public const int OFFSET_BEGIN = 0x12;

        public const int INDIC_UNCOMPRESSED = 0x00;
        public const int INDIC_COMPRESSED = 0x01;

        public Stream stream { get; private set; }

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
            
            var headerBuf = new byte[4];
            var headerRead = stream.Read(headerBuf, 0, headerBuf.Length);
            var compressionHeader = BitConverter.ToInt32(headerBuf, 0);
            if (compressionHeader == INDIC_COMPRESSED) {
                this.stream = new LZ4Stream(stream, LZ4StreamMode.Decompress);
            }
            
            using (var br = new BinaryReader(stream, Encoding.Default, true)) {
                // read pak file index
                var delim = -1;
                while (true) {
                    delim = br.ReadInt32();
                    if (delim != INDEX_HEADER_DELIM) break;
                    var length = br.ReadInt64();
                    var nameLength = br.ReadInt32();
                    var name = br.ReadString();
                    _index[name] = new DataFile {length = length};
                }
                if (delim != OFFSET_BEGIN) throw new DataMisalignedException();
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

        public bool hasFile(string path) {
            return _index.ContainsKey(path);
        }

        public IEnumerable<string> getFileNames() {
            return _index.Keys.AsEnumerable();
        }

        public void close() {
            stream.Close();
        }

        public void Dispose() {
            close();
        }
    }
}