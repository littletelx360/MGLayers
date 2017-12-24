using System.IO;

namespace MGLayers {
    public class DirectoryContentSource : ILayeredContentSource {
        private readonly string _directoryPath;

        public DirectoryContentSource(string path) {
            _directoryPath = Path.GetFullPath(path);
        }

        public Stream openFile(string path) {
            return File.OpenRead(Path.Combine(_directoryPath, path));
        }

        public bool hasFile(string path) {
            return File.Exists(Path.Combine(_directoryPath, path));
        }

        public void Dispose() { }
    }
}