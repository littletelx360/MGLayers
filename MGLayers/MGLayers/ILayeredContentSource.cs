using System;
using System.IO;

namespace MGLayers {
    public interface ILayeredContentSource : IDisposable {
        Stream openFile(string path);
        bool hasFile(string path);
    }
}