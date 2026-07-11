# Uno2D Win2D Parity - Session 9: CanvasCommandList + CanvasCachedGeometry

## Session goal

Implement `CanvasCommandList` (recording/replay of drawing commands) and `CanvasCachedGeometry` (pre-composited geometry caching). These are intermediate building blocks that enable performance optimizations and advanced rendering patterns.

## Current state

`CanvasCommandList` and `CanvasCachedGeometry` are now implemented.

## Progress update

- Added `CanvasCommandList` with offscreen Skia surface recording and replay into a target `CanvasDrawingSession`.
- Added `CanvasCachedGeometry` with cached `SKPath` copies for filled geometry and pre-stroked geometry.
- Added internal `CanvasDrawingSession` helpers for replaying Skia images and drawing cached paths.
- Added tests for command list replay, idempotent replay, cached geometry fill, and stroked cached geometry.

Current limitation: command lists replay recorded pixels from an offscreen surface. They are not yet resolution-independent operation lists, so callers should pick an explicit recording size when using this shim.

## Desired outcomes

1. `CanvasCommandList` can record drawing session commands and replay them
2. `CanvasCachedGeometry` caches a geometry + stroke combination
3. Both are GPU-light using Skia surfaces for command recording

## API to add

### CanvasCommandList

| Member | Notes |
|--------|-------|
| `.ctor(CanvasDevice)` | Creates a recording surface |
| `CreateDrawingSession()` | Returns a recording CanvasDrawingSession |
| `Draw(CanvasDrawingSession)` | Replay recorded commands to target session |

Implementation strategy: Record commands by creating an offscreen `SKSurface` and routing drawing to it. On `Draw()`, blit the recorded surface to the target. More sophisticated: intercept and store `IDrawingOperation` list for resolution-independent replay (deferred to P2).

### CanvasCachedGeometry

| Method | Notes |
|--------|-------|
| `.ctor(CanvasGeometry)` | Cache a geometry |
| `.ctor(CanvasGeometry, float strokeWidth, CanvasStrokeStyle)` | Cache a stroked geometry |
| `Draw(CanvasDrawingSession, Color, ...)` | Draw cached geometry |
| `Fill(CanvasDrawingSession, Color, ...)` | Fill cached geometry |

Implementation: Convert geometry to `SKPath`, apply stroke if specified, optionally rasterize to `SKSurface` for quick blit.

## Work items

1. **CommandList recording** — On `CreateDrawingSession()`, create an offscreen `SKSurface`. Return a `CanvasDrawingSession` wrapping it.

2. **CommandList replay** — `Draw(target)` does `SKSurface.Draw(targetCanvas, ...)` to copy recorded pixels.

3. **CachedGeometry** — Wraps `CanvasGeometry` (or its `SKPath`) with optional pre-applied stroke. Cache rasterization as `SKPicture` or `SKSurface`.

4. **Stability tests** — Record a sequence of commands, replay, compare pixel output to inline drawing. Verify idempotent replay.

## Files to modify

- New file: `Canvas/CanvasCommandList.cs`
- New file: `Canvas/Geometry/CanvasCachedGeometry.cs`
- `tests/` — command list recording + replay tests, cached geometry test

## Exit criteria

- CanvasCommandList records and replays basic primitives — complete
- CanvasCachedGeometry caches and draws/fills correctly — complete
- 2+ tests passing — complete
- Parity matrix updated — complete

## Estimated effort

- Core implementation: 2.5h
- Tests: 1.5h
- Total: ~4h
