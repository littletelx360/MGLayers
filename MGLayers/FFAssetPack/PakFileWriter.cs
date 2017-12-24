using System.Collections.Generic;
using System.IO;
using System.Text;
using LZ4;

namespace FFAssetPack {
    public class PakFileWriter {
        private Stream _outputStream;
        private readonly bool compressed;

        public List<KeyValuePair<string, Stream>> filesToAdd = new List<KeyValuePair<string, Stream>>();

        public PakFileWriter(Stream outputStream, bool compress) {
            _outputStream = outputStream;
            this.compressed = compress;
        }

        public void addFile(string path, Stream file) {
            filesToAdd.Add(new KeyValuePair<string, Stream>(path, file));
        }

        public void write() {
            // write header
            // header format:
            // - Compression Indicator -
            // 0x00 - uncompressed, 0x01 - compressed
            // - File Index -
            // Repeated blocks of format:
            // MAGIC:\0x11:int FILE_LENGTH:long PATH_LENGTH:int PATH:string
            // - Offsets -
            // MAGIC:\0x12:int
            // Repeated, corresponds in same order to the entries in the index
            // FILE_DATA_OFFSET: long
            // - Data -
            // 0x12, Raw file data at the offsets


            if (compressed) {
                // Write compression indicator
                using (var bw = new BinaryWriter(_outputStream, Encoding.Default, true)) {
                    bw.Write(compressed ? PakFile.INDIC_COMPRESSED : PakFile.INDIC_UNCOMPRESSED);
                }

                // create new output stream
                _outputStream = new LZ4Stream(_outputStream, LZ4StreamMode.Compress);
            }

            using (var bw = new BinaryWriter(_outputStream, Encoding.Default)) {
                // Write file index
                foreach (var fileEntry in filesToAdd) {
                    bw.Write(PakFile.INDEX_HEADER_DELIM);
                    bw.Write(fileEntry.Value.Length); // stream length
                    bw.Write(fileEntry.Key.Length); // name length
                    bw.Write(fileEntry.Key); // name
                }

                bw.Write(PakFile.OFFSET_BEGIN);
                bw.Flush();
                var headerSize = _outputStream.Position + filesToAdd.Count * sizeof(ulong);
                // Write offsets
                var offset = headerSize;
                foreach (var fileEntry in filesToAdd) {
                    bw.Write(offset);
                    offset += fileEntry.Value.Length;
                }

                bw.Flush();
                // Write data
                foreach (var fileEntry in filesToAdd) {
                    fileEntry.Value.CopyTo(_outputStream);
                }
            }
        }

        public void close() {
            _outputStream.Close();
        }
    }
}