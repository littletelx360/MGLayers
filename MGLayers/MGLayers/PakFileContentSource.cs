using System.IO;
using FFAssetPack;

namespace MGLayers {
    public class PakFileContentSource : ILayeredContentSource {
        private readonly PakFile _pakFile;

        public PakFileContentSource(PakFile pakFile) {
            _pakFile = pakFile;
            _pakFile.read();
        }

        public Stream openFile(string path) {
            return _pakFile.getFile(path);
        }

        public bool hasFile(string path) {
            return _pakFile.hasFile(path);
        }

        public void Dispose() {
            _pakFile.Dispose();
        }
    }
}