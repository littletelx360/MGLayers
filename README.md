# MGLayers

layered and packed asset loading extensions for MonoGame

## Using layered assets

First, create an instance of `LayeredContentManager`.
Then, you can add content sources (`addContentSource`) with a corresponding priority (0 is highest priority, greater values are lower priority).

```csharp
// Add a .pak file as a content source
contentManager.addContentSource(new PakFileContentSource(new PakFile(File.OpenRead("PackedContent.pak"))), 0);

// Add the 'Content' directory for the default loading behavior
contentSource.addContentSource(new DirectoryContentSource("Content"), 1);
```

Everything else is handled automatically by overriding `OpenStream` in the `ContentManager`. You can use
the content manager as usual, but assets will be loaded based on this layered priority and from multiple sources.

## Creating asset packs

A tool, [`FFAssetPackTool`](MGLayers/FFAssetPackTool) is included in this repository.
Build it from source and use it `dotnet FFAssetPackTool.dll` to see usage information.
Compression does not currently work.

## Packed asset format

FFAssetPack provides a custom packed asset format (.pak) for storing assets in a single file.
Earlier, LZ4 compression was supported, but since the new format requires stream seeking, it has been disabled.

The format is:

```
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
```

See the `PakFileWriter` source file for a reference implementation.
`PakFile` has a reference implementation of a reader.

## Troubleshooting

Sometimes, installing the `MGLayers` package can cause MonoGame to prioritize `MonoGame.Framework.Portable`, and
this will result in `Window` being `null`, likely breaking your game. To solve this, uninstall your platform-specific
package (`MonoGame.Framework.DesktopGL` for example), install `MGLayers`, then reinstall the platform-specific package.

Copyright &copy; 2017-2018 0xFireball. All Rights Reserved.

Licensed under the Apache License 2.0.
