# Uno2D Win2D Parity - Session 10: Effects Pipeline

## Session goal

Implement the Win2D effects system — the `CanvasEffect` graph API that applies GPU-accelerated image filters (blur, color matrix, composite, shadow, etc.) during drawing.

## Current state

Core effect coverage is implemented with Skia-backed eager raster evaluation.

## Progress update

- Added `ICanvasImage` and `CanvasEffect` base infrastructure.
- Made `CanvasBitmap` usable as an effect/image source.
- Added `GaussianBlurEffect`, `ColorMatrixEffect`, `SaturationEffect`, `OpacityEffect`, `Transform2DEffect`, `ShadowEffect`, `BlendEffect`, and `CompositeEffect`.
- Added `CanvasDrawingSession.DrawImage(CanvasEffect, Rect)` and source-rect overload.
- Added tests for color matrix channel swap, opacity, saturation, blend, composite, blur spread, and shadow offset.

Current limitation: effects render eagerly to intermediate Skia images. This is API-compatible for common rendering paths, but it is not yet a retained GPU filter graph with cache invalidation.

## Desired outcomes

1. Effect graph architecture (base `CanvasEffect` class with input chaining)
2. 6-8 core effects implemented (GaussianBlur, ColorMatrix, Composite, Shadow, Blend, Saturation, Transform2D)
3. `CanvasDrawingSession.DrawImage(effect, ...)` overloads
4. Snapshot tests for each effect

## Architecture

Win2D effects are a directed acyclic graph:
```
var blur = new GaussianBlurEffect();
blur.Source = bitmap;
blur.BlurAmount = 3.0f;
drawingSession.DrawImage(blur, destRect);
```

### Base class

| Member | Notes |
|--------|-------|
| `CanvasEffect` | Abstract base |
| `Source` | Input source (ICanvasImage) |
| `Sources[]` | Multiple sources for composite/blend |
| `GetCachedOutput()` | Lazily rendered output image |

### Effect types (P0)

| Effect | Win2D properties | Skia mapping |
|--------|------------------|-------------|
| `GaussianBlurEffect` | `BlurAmount`, `Optimization`, `BorderMode` | `SKImageFilter.CreateBlur` |
| `ColorMatrixEffect` | `ColorMatrix` (5x4), `SourceGraphic` | `SKColorFilter.CreateColorMatrix` |
| `CompositeEffect` | `Mode`, `Destinations[]` | `SKBlendMode` |
| `ShadowEffect` | `BlurAmount`, `Color`, `Offset` | `SKDropShadowImageFilter` |
| `BlendEffect` | `Mode`, `Background`, `Foreground` | `SKBlendMode` + `SKShader` |
| `SaturationEffect` | `Saturation` (0-1) | `SKColorFilter` with saturation matrix |
| `Transform2DEffect` | `TransformMatrix`, `InterpolationMode` | `SKCanvas.SetMatrix` |
| `OpacityEffect` | `Opacity` (nested in source) | Alpha channel |

### Drawing session integration

| Overload | Notes |
|----------|-------|
| `DrawImage(CanvasEffect, Rect dest)` | Evaluate effect graph, draw result |
| `DrawImage(CanvasEffect, Rect dest, Rect source, ...)` | With source rect |

## Work items

1. **Effect base** — `CanvasEffect` abstract class with `Source` property, lazy evaluation via `SKImageFilter` graph.

2. **GaussianBlur** — Map to `SKImageFilter.CreateBlur`. Handle `BorderMode` (Soft/Hard).

3. **ColorMatrix** — Represent 5x4 matrix. Map to `SKColorFilter.CreateColorMatrix`. Apply as filter on draw.

4. **Composite + Blend** — Combine multiple source images using `SKCanvas.DrawImage` with `SKPaint.BlendMode`.

5. **Shadow** — `SKImageFilter.CreateDropShadow` or `SKDrawShadowRec` for directional shadow.

6. **Saturation + Opacity** — Simple `SKColorFilter` matrix applications.

7. **Transform2D** — Apply matrix via canvas transform before drawing source.

8. **DrawImage(effect)** — Evaluate effect DAG to produce final renderable image, then draw.

9. **Snapshot tests** — One scene per effect: blur levels, color matrix color shift, shadow offset + color, blend modes grid, composite overlapping shapes.

## Files to modify

- New directory: `Canvas/Effects/`
- New file: `Canvas/Effects/CanvasEffect.cs` — base class
- New files per effect type (8 files)
- `Canvas/CanvasDrawingSession.cs` — add DrawImage(CanvasEffect) overloads
- `tests/` — effect tests

## Exit criteria

- 8 effects functional — complete
- Effect graph chaining works (blur → color matrix → draw) — supported through nested `CanvasEffect.Source`
- DrawImage(effect) API working — complete
- 5+ snapshot tests passing — complete with pixel assertions
- Parity matrix updated — complete

## Estimated effort

- Core implementation: 4h
- Tests + snapshots: 2.5h
- Total: ~6.5h
