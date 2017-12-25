using System;
using System.IO;
using System.Linq;
using FFAssetPack;

namespace FFAssetPackTool {
    internal class Program {
        public static void Main(string[] args) {
            if (args.Length < 3) {
                Console.WriteLine(@"
    Usage:
        AssetPackTool <create|unpack> <directory> <target file> [options]

    Options:
        -c    Enable LZ4 compression of package
");
                return;
            }

            var command = args[0];
            var directory = Path.GetFullPath(args[1]);
            var target = Path.GetFullPath(args[2]);
            var options = args.Skip(3).ToArray();

            var useCompression = options.Contains("-c");

            if (command == "create") {
                using (var targetFile = File.Open(target, FileMode.Create, FileAccess.Write))
                using (var pakWriter = new PakFileWriter(targetFile, useCompression)) {
                    foreach (var sourceFile in Directory.GetFiles(directory, "*", SearchOption.AllDirectories)) {
                        var fileName = sourceFile.Substring(directory.Length)
                            .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                            .Replace("\\", "/"); // replace Windows directory-separator with default one
                        pakWriter.addFile(fileName, File.OpenRead(sourceFile));
                    }

                    pakWriter.write();
                }
            } else if (command == "unpack") {
                Directory.CreateDirectory(directory);
                using (var targetFile = File.OpenRead(target))
                using (var pakFile = new PakFile(targetFile)) {
                    pakFile.read();

                    foreach (var packedFile in pakFile.getFileNames()) {
                        var file = pakFile.getFile(packedFile);
                        var outputPath = Path.Combine(directory, packedFile);
                        Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
                        
                        using (var outputStream = File.Open(outputPath, FileMode.Create, FileAccess.Write)) {
                            file.CopyTo(outputStream);
                        }
                    }
                }
            }
        }
    }
}