# Uno2D Win2D Parity - Session 7: CanvasBitmap

## Session goal

Implement `CanvasBitmap` — the bitmap I/O type for loading, saving, copying, and querying pixel data. This is the most impactful new type: it unlocks `DrawImage` in `CanvasDrawingSession` (Session 2) and is the foundation for brush image sources (Session 8).

## Current state

`CanvasBitmap` now exists with create/load/save/pixel-copy/draw integration.

## Progress update

- Added `CanvasBitmap` with load from stream, byte array, file path, `file://`, and `ms-appx:///`.
- Added `CreateFromColors`, `CreateFromRenderTarget`, `Size`, `SizeInPixels`, `Bounds`, `GetBounds`, `GetPixelColors`, and `CopyPixels`.
- Added sub-rectangle `CopyPixels(byte[], x, y, width, height)`.
- Added PNG and JPEG save coverage; unsupported/weak Skia formats currently fall back to PNG where needed.
- Wired `CanvasDrawingSession.DrawImage` overloads to render `CanvasBitmap`.
- Added tests for create/read pixels, PNG round-trip, JPEG loadability, sub-rect copy, render target capture, and DrawImage.

## Desired outcomes

1. `CanvasBitmap.LoadAsync` from file, stream, URI, and byte array
2. `CanvasBitmap.SaveAsync` to stream (multiple formats)
3. `CopyPixels` for pixel data extraction
4. `GetBounds` / `GetBoundsAt` for size queries
5. `CreateFromColors` for programmatic bitmap creation
6. Integration with `CanvasRenderTarget` (convert render target to bitmap)

## API to add

### Factory methods

| Method | Skia mapping |
|--------|-------------|
| `CanvasBitmap.LoadAsync(CanvasDevice, Stream)` | `SKBitmap.Decode(Stream)` or `SKImage.FromEncodedData` |
| `CanvasBitmap.LoadAsync(CanvasDevice, string uri)` | URI → Stream → decode |
| `CanvasBitmap.LoadAsync(CanvasDevice, byte[] bytes)` | `SKBitmap.Decode(bytes)` |
| `CanvasBitmap.CreateFromColors(CanvasDevice, Color[], int w, int h)` | `SKBitmap` with pixel array |

### Instance methods

| Member | Skia mapping |
|--------|-------------|
| `Size` / `SizeInPixels` | Return SKBitmap dimensions |
| `Bounds` / `GetBounds(CanvasDrawingSession)` | Rect from size |
| `CopyPixels(byte[] buffer, ...)` | `SKBitmap.GetPixels` + marshal |
| `SaveAsync(Stream, CanvasBitmapFileFormat)` | SKBitmap.Encode(format) |
| `Dispose()` | Clean up SKBitmap/Image |

### Related enum expansion

Extend `CanvasBitmapFileFormat` from `Png` only to: `Png`, `Jpeg`, `Bmp`, `Tiff`, `Gif`, `Dds`, `Hdr`.

## Work items

1. **LoadAsync** — Implement each overload as async wrapper around `SKBitmap.Decode`. Support `ms-appx://` and `file://` URI schemes. Handle stream ownership.

2. **CreateFromColors** — Construct `SKBitmap` of given size, copy color array into pixel buffer (BGRA32 format). Flip R and B for Win2D color order vs Skia.

3. **SaveAsync** — Map each `CanvasBitmapFileFormat` to Skia's `SKEncodedImageFormat`. Write to memory stream.

4. **CopyPixels** — Expose raw pixel access. Support sub-rect extraction via `SKBitmap.ExtractSubset` or manual offset.

5. **Bounds** — Return `Rect` from bitmap dimensions. The `GetBounds(CanvasDrawingSession)` variant applies DIP scaling.

6. **Integration tests** — Load bitmap from known test image, verify pixel dimensions, save to PNG, reload, compare bytes.

## Files to modify

- New file: `Canvas/CanvasBitmap.cs` — main type
- New file: `Canvas/CanvasBitmapFileFormat.cs` — expanded enum (replace inline in CanvasPrimitives.cs)
- `Canvas/CanvasDrawingSession.cs` — un-stub `DrawImage` now that CanvasBitmap exists
- `tests/` — load/save/copy/create tests

## Exit criteria

- All LoadAsync overloads working — complete for stream, bytes, path/URI
- SaveAsync produces valid encoded output (at least PNG and JPEG) — complete
- CopyPixels returns correct pixel data — complete, including sub-rect copy
- CreateFromColors round-trips correctly — complete
- DrawImage functional — complete
- 4+ snapshot tests (load + draw, create + draw, save round-trip) — covered with pixel assertions
- Parity matrix updated — complete

## Estimated effort

- Core implementation: 3h
- Tests + snapshots: 2h
- Total: ~5h
