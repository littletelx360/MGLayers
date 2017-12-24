using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Content;

namespace MGLayers {
    public class LayeredContentManager : ContentManager {
        public LayeredContentManager(IServiceProvider serviceProvider) : base(serviceProvider) { }

        public LayeredContentManager(IServiceProvider serviceProvider, string rootDirectory) : base(serviceProvider,
            rootDirectory) { }

        private List<KeyValuePair<ILayeredContentSource, int>> _contentSources =
            new List<KeyValuePair<ILayeredContentSource, int>>();

        public void addContentSource(ILayeredContentSource source, int priority) {
            _contentSources.Add(new KeyValuePair<ILayeredContentSource, int>(source, priority));

            // re-sort content sources
            _contentSources = _contentSources.OrderBy(x => x.Value).ToList();
        }

        public bool removeContentSource(ILayeredContentSource source) {
            return _contentSources.RemoveAll(x => x.Key == source) > 0;
        }

        protected override Stream OpenStream(string assetName) {
            assetName += ".xnb"; // append built content format
            var source = _contentSources.FirstOrDefault(x => x.Key.hasFile(assetName));
            if (source.Key != null) {
                return source.Key.openFile(assetName);
            } else {
                throw new FileNotFoundException($"Asset with path {assetName} was not found in any content source.");
            }
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);

            // dispose content sources
            foreach (var contentSource in _contentSources) {
                contentSource.Key.Dispose();
            }
        }
    }
}