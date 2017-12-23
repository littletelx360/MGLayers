using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FFAssetPack {
    public class PakFileWriter {
        private readonly Stream _outputStream;

        public List<KeyValuePair<string, Stream>> filesToAdd = new List<KeyValuePair<string, Stream>>();

        public PakFileWriter(Stream outputStream) {
            _outputStream = outputStream;
        }

        public void addFile(string path, Stream file) {
            filesToAdd.Add(new KeyValuePair<string, Stream>(path, file));
        }

        public void write() {
            // write header
            // header format:
            // - File Index -
            // Repeated blocks of format:
            // MAGIC:\0x11:int FILE_LENGTH:long PATH_LENGTH:int PATH:string
            // - Offsets -
            // MAGIC:\0x12:int
            // Repeated, corresponds in same order to the entries in the index
            // FILE_DATA_OFFSET: long
            // - Data -
            // 0x12, Raw file data at the offsets

            using (var sw = new BinaryWriter(_outputStream, Encoding.Default)) {
                // Write file index
                foreach (var fileEntry in filesToAdd) {
                    sw.Write(PakFile.INDEX_HEADER_DELIM);
                    sw.Write(fileEntry.Value.Length); // stream length
                    sw.Write(fileEntry.Key.Length); // name length
                    sw.Write(fileEntry.Key); // name
                }
                sw.Write(PakFile.OFFSET_BEGIN);
                sw.Flush();
                var headerSize = _outputStream.Position + filesToAdd.Count * sizeof(ulong);
                // Write offsets
                var offset = headerSize;
                foreach (var fileEntry in filesToAdd) {
                    sw.Write(offset);
                    offset += fileEntry.Value.Length;
                }
                sw.Flush();
                // Write data
                foreach (var fileEntry in filesToAdd) {
                    fileEntry.Value.CopyTo(_outputStream);
                }
            }
        }
    }
}