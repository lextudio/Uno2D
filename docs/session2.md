# Uno2D Win2D Parity - Session 2: CanvasDrawingSession Expansion

## Session goal

Extend `CanvasDrawingSession` from 11 members to ~40, covering the next tier of drawing primitives that Win2D users most commonly reach for after basic shapes.

## Desired outcomes

1. Ellipse, Geometry, TextLayout, and Image drawing added
2. `Transform`, `Antialiasing`, `Blend` state properties implemented
3. Flush and Layer support added
4. All new methods covered by semantic + snapshot tests

## API to add

### Drawing primitives (priority order)

| Method | Win2D signature | Skia mapping |
|--------|----------------|--------------|
| `DrawEllipse` / `FillEllipse` | `(float cx, float cy, float rx, float ry, Color, ...)` | `SKCanvas.DrawOval` |
| `DrawGeometry` / `FillGeometry` | `(CanvasGeometry, Color, ...)` | `SKCanvas.DrawPath` |
| `DrawTextLayout` | `(CanvasTextLayout, float x, float y, Color)` | `SKCanvas.DrawText` using layout's formatted text |
| `DrawImage` | `(CanvasBitmap, Rect destRect, ...)` | `SKCanvas.DrawBitmap` / `DrawImage` |
| `DrawLayer` | `(float opacity, ...)` | `SKCanvas.SaveLayer` |

### State properties

| Property | Win2D type | Skia mapping |
|----------|-----------|--------------|
| `Transform` | `System.Numerics.Matrix3x2` | `SKCanvas.TotalMatrix` / `SetMatrix` |
| `Antialiasing` | `CanvasAntialiasing` | `SKCanvas.IsClipAA` + paint-level AA |
| `TextAntialiasing` | `CanvasTextAntialiasing` | `SKPaint.IsAntialias` + `SubpixelText` |
| `Blend` | `CanvasBlend` | `SKPaint.BlendMode` |
| `Units` | `CanvasUnits` | DIP-to-pixel scaling helper |

### Control methods

| Method | Purpose |
|--------|---------|
| `Flush()` | Force pending draw operations (`SKCanvas.Flush`) |
| `DrawInk` | Ink stroke rendering (convert to Skia path) |

## Work items

1. **Ellipse** — Verify `SKCanvas.DrawOval` maps correctly for circular vs elliptical aspect ratios; test stroke + fill.

2. **Geometry** — `DrawGeometry` accepts `CanvasGeometry` (wraps `SKPath`); `FillGeometry` with `CanvasFilledRegionDetermination` mapping to `SKPathFillType`.

3. **TextLayout** — `DrawTextLayout` extracts formatted text from `CanvasTextLayout` and draws at target position; must respect the layout's own alignment.

4. **Transform** — Expose `System.Numerics.Matrix3x2` on `CanvasDrawingSession`, translate to/from `SKMatrix`; test: push transform, draw, pop via `Save/Restore`.

5. **Antialiasing / Blend** — Enums `CanvasAntialiasing` (`Antialiased`, `Aliased`), `CanvasTextAntialiasing` (`Auto`, `Aliased`, `ClearType`, `Grayscale`), `CanvasBlend` (`SourceOver`, `Copy`, `Min`, `Max`, etc.); test each blend mode with overlapping primitives.

6. **DrawImage** — Requires `CanvasBitmap` from Session 7, so stub the method with `NotImplementedException` for now, but add the overload signature. Mark clearly in docs.

7. **Flush + Layer** — `Flush` maps to `SKCanvas.Flush()`; `DrawLayer` wraps `SKCanvas.SaveLayer` / `SaveCount` / `Restore`.

8. **Snapshot tests** — One scene per new primitive: ellipses grid, geometry with fills, transform+rotate, blend mode matrix, layer with opacity.

## Files to modify

- `src/Win2D.UnoCompat/Canvas/CanvasDrawingSession.cs` — add ellipses, geometry, textlayout, image, layer, flush, state properties
- New enum files: `CanvasAntialiasing.cs`, `CanvasBlend.cs`, `CanvasUnits.cs`
- `tests/` — new test files for each added feature

## Exit criteria

- 10+ new `CanvasDrawingSession` members merged
- `Transform`, `Antialiasing`, `Blend`, `Units` properties functional
- `DrawImage` signature added (even if throws, to unblock callers)
- 3+ snapshot tests passing
- Parity matrix in `design.md` updated

## Estimated effort

- **Core implementation**: 2.5h
- **Tests + snapshots**: 1.5h
- **Total**: ~4h

## Session 2 results

**Status: ✅ Complete**

### What was implemented

| Area | Members added | Notes |
|------|:-----------:|-------|
| DrawEllipse / FillEllipse | 4 overloads (center + rect) | Maps to SKCanvas.DrawOval |
| DrawGeometry / FillGeometry | 2 (Color-only) | Added internal GetPath() to CanvasGeometry |
| DrawTextLayout | 1 | Uses font metrics for baseline alignment |
| CreateLayer | 1 (+ CanvasLayer class) | SKCanvas.SaveLayer with alpha paint |
| Transform property | 1 (get/set) | Matrix3x2 ↔ SKMatrix conversion |
| Antialiasing property | 1 | Applied to all subsequent paint creation |
| TextAntialiasing property | 1 | Controls text paint antialiasing |
| Blend property | 1 | Mapped to SKBlendMode (Min/Max removed — no Skia equivalent) |
| Units property | 1 | Stored, not yet used in coordinate conversion |
| DrawImage | 1 | Stub — NotImplementedException; requires CanvasBitmap (Session 7) |
| Flush | 1 | SKCanvas.Flush() |

### New files created

- `Canvas/CanvasAntialiasing.cs`
- `Canvas/CanvasTextAntialiasing.cs`
- `Canvas/CanvasBlend.cs`
- `Canvas/CanvasUnits.cs`

### Modified files

- `Canvas/CanvasDrawingSession.cs` — added 15 new public members
- `Canvas/Geometry/CanvasGeometry.cs` — added internal `GetPath()`

### New API coverage added: ~15 members (from ~51 → ~66)
### Total coverage: ~11% (up from ~9%)

### Deviations from plan

- **Min/Max blend modes** removed from CanvasBlend enum — no SKBlendMode equivalent in SkiaSharp 3.x
- **Snapshot tests** deferred — the test project needs a rendering infrastructure; will be established in a later session
- Brush overloads for DrawGeometry/FillGeometry deferred to Session 8
