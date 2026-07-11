# Uno2D Win2D Parity - Session 11: Image Sources + SpriteBatch

## Session goal

Implement `CanvasImageSource`, `CanvasVirtualImageSource` (XAML image source integration) and `CanvasSpriteBatch` (efficient batch rendering). These bridge 2D rendering into XAML's imaging pipeline and enable game-style batching.

## Current state

Surface-backed image source and sprite batch coverage is implemented.

## Progress update

- Added `CanvasImageSource` with Skia surface backing, drawing-session creation, invalidation state, and bitmap snapshot output.
- Added `CanvasVirtualImageSource` with tile size and invalid region tracking for large surfaces.
- Added `CanvasSpriteBatch` with queued bitmap draws, flush-to-session support, tint support, and maximum batch enforcement.
- Added tests for image source snapshot pixels, virtual invalid regions, sprite batch flush, and batch overflow state.

Current limitation: native XAML `ImageSource` bridging is represented by an `ImageSource` placeholder in this headless library/test target. The Skia rendering path and bitmap snapshot are functional.

## Desired outcomes

1. `CanvasImageSource` as a XAML-compatible `ImageSource` backed by Skia
2. `CanvasVirtualImageSource` for large virtualized surfaces
3. `CanvasSpriteBatch` for efficient sprite drawing

## API to add

### CanvasImageSource

| Member | Notes |
|--------|-------|
| `.ctor(CanvasDevice, float w, float h, float dpi)` | Create Skia surface-backed image source |
| `CreateDrawingSession(Color clearColor)` | Begin rendering to the source |
| `Invalidate()` | Mark source as dirty for XAML update |
| `ImageSource` | Returns `Microsoft.UI.Xaml.Media.ImageSource` |

Implementation: Wrap an `SKSurface` + `SKImage`. When `Invalidate()` is called, snapshot to `WriteableBitmap` or `SoftwareBitmapSource`.

### CanvasVirtualImageSource

| Member | Notes |
|--------|-------|
| `.ctor(CanvasDevice, float w, float h, float dpi, int tileSize)` | Tiled virtual surface |
| `CreateDrawingSession(Rect region)` | Render only a region |
| `Invalidate(Rect region)` | Invalidate partial region |

Implementation: Tile the large surface into `tileSize` SKSurface tiles. Render only visible/requested tiles.

### CanvasSpriteBatch

| Member | Notes |
|--------|-------|
| `.ctor(CanvasDevice)` | Create batch |
| `Draw(CanvasBitmap, Rect dest, Rect source, Color tint, ...)` | Queue a sprite |
| `Flush()` | Submit all queued sprites in one draw call |
| `IsFailed` | Check if batch failed |
| `MaximumSpritesPerBatch` | Max sprites per flush |

Implementation: Collect sprite quads. On Flush, draw all using a single `SKCanvas.DrawBitmap` loop (or atlas-based `DrawImage`).

## Work items

1. **CanvasImageSource** — Create SKSurface, expose CreateDrawingSession, wire Invalidate to image source update for XAML consumption.

2. **VirtualImageSource** — Tile management (allocate, evict, render). Region-to-tile mapping for partial invalidation.

3. **SpriteBatch** — Sprite quad collection. `Flush()` draws all queued sprites. Handle tint color via SKPaint.ColorFilter.

4. **Integration tests** — ImageSource renders to CanvasControl and displays correctly. SpriteBatch draws 100+ sprites at varying positions.

## Files to modify

- New file: `Canvas/CanvasImageSource.cs`
- New file: `Canvas/CanvasVirtualImageSource.cs`
- New file: `Canvas/CanvasSpriteBatch.cs`
- `tests/` — image source and sprite batch tests

## Exit criteria

- CanvasImageSource produces valid XAML image source — Skia surface + bitmap snapshot complete; native XAML bridge pending
- CanvasVirtualImageSource handles >4096px surfaces — region/tile tracking complete
- CanvasSpriteBatch draws 500+ sprites correctly — queue/flush path complete; tests cover rendering and limits
- 3+ tests passing — complete
- Parity matrix updated — complete

## Estimated effort

- Core implementation: 3h
- Tests: 2h
- Total: ~5h
