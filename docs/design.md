# Win2D Compatibility Shim Design

## Overview

The goal of this design is to describe how the Win2D compatibility shim for Uno Platform will be built on top of SkiaSharp.
The shim provides a lightweight subset of the `Microsoft.Graphics.Canvas` API surface so Uno apps can compile and render Win2D-style graphics across non-Windows platforms.

This design is based on the existing `Win2D.UnoCompat` implementation and its current types, including:

- `Microsoft.Graphics.Canvas.CanvasDevice`
- `Microsoft.Graphics.Canvas.CanvasRenderTarget`
- `Microsoft.Graphics.Canvas.CanvasDrawingSession`
- `Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl`
- `Microsoft.Graphics.Canvas.Text.CanvasTextFormat`
- `Microsoft.Graphics.Canvas.Text.CanvasTextLayout`
- `Microsoft.Graphics.Canvas.Geometry.CanvasGeometry`
- `Microsoft.Graphics.Canvas.CanvasStrokeStyle`

## Design Principles

- Keep the API surface compatible with Win2D semantics where possible.
- Map high-level Win2D operations to SkiaSharp primitives.
- Support Uno Platform rendering by using `SkiaSharp.Views.Uno.WinUI`.
- Keep the implementation minimal and testable.
- Preserve DPI-aware rendering and coordinate consistency between XAML and Skia.

## Architecture

The shim is organized into three main layers:

1. Core canvas primitives (`Canvas/`)
2. Text and layout support (`Canvas/Text/`)
3. XAML integration with Uno (`Canvas/UI/Xaml/`)

### Core canvas primitives

`Microsoft.Graphics.Canvas.CanvasDevice` is the entry point for canvas rendering.
It provides a shared device and creates `CanvasRenderTarget` objects.

`CanvasRenderTarget` owns a Skia `SKSurface` and exposes:

- `CreateDrawingSession()` to produce a `CanvasDrawingSession`
- `GetPixelBytes()` to inspect the rendered buffer
- `SaveAsync()` to encode the render target to a PNG stream
- `Dispose()` to clean up the Skia surface

`CanvasDrawingSession` wraps a Skia `SKCanvas` and exposes basic Win2D-compatible drawing APIs:

- `Clear(Color)`
- `FillCircle` / `DrawCircle`
- `DrawLine`
- `DrawRectangle` / `FillRectangle`
- `DrawRoundedRectangle` / `FillRoundedRectangle`
- `DrawText` with text bounds or point coordinates

The drawing session maps Win2D styles to Skia types:

- `CanvasStrokeStyle` → `SKPaint.StrokeCap`, `SKPaint.StrokeJoin`, `SKPathEffect`
- `Color` → `SKColor`
- Stroke/fill painting uses `SKPaintStyle.Fill` or `SKPaintStyle.Stroke`

### Text support

`CanvasTextFormat` encapsulates Win2D text formatting options.
It resolves font family names to `SKTypeface` by:

- using `SKTypeface.Default` as fallback
- supporting `ms-appx:///` and `file://` asset URIs
- parsing `uri#FontFamily` WinUI style strings
- caching typefaces in a dictionary for reuse

`CanvasTextLayout` provides text layout placeholder support and exposes a Skia-compatible path via `CreatePath()`.
It is designed to work with text path rendering and geometry creation.

### Geometry support

`CanvasGeometry` wraps a Skia `SKPath` and exposes Win2D-like geometry operations.
Current functionality includes:

- `CreateText(CanvasTextLayout)` to generate geometry from text
- `ComputeBounds()` to read path bounds
- `SendPathTo(ICanvasPathReceiver)` to convert Skia path verbs back into Win2D-style path operations

This approach preserves geometry interoperability without replicating the full Win2D geometry API.

### XAML integration

`CanvasControl` is implemented as a subclass of `SkiaSharp.Views.Windows.SKXamlCanvas`.
It integrates with Uno/XAML by:

- subscribing to `PaintSurface`
- computing DIP scaling between device pixels and XAML layout size
- creating a `CanvasDrawingSession` for each paint pass
- exposing a `Draw` event with `CanvasDrawEventArgs`
- providing `Invalidate()` to trigger redraws

This makes the control behave like a Win2D `CanvasControl` while using SkiaSharp as the rendering backend.

## Coordinate and DPI handling

The shim must maintain coordinate consistency between XAML DIP and Skia device pixels.
`CanvasControl.TryComputeDipScale(...)` computes a scaling factor using:

- Skia surface pixel size (`e.Info.Width`, `e.Info.Height`)
- XAML `ActualWidth` and `ActualHeight` in DIP

The control applies `e.Surface.Canvas.Scale(sx, sy)` before drawing to keep layout coordinates stable.

## Compatibility mapping strategy

### Direct API mappings

For simple drawing operations, map directly to Skia:

- Win2D `FillRectangle` → `SKCanvas.DrawRect(..., SKPaintStyle.Fill)`
- Win2D `DrawLine` → `SKCanvas.DrawLine(...)`
- Win2D `DrawCircle` → `SKCanvas.DrawCircle(...)`

### Style translation

Translate Win2D stroke styles into Skia equivalents in a centralized helper:

- `CanvasStrokeStyle.StartCap` / `EndCap` → `SKStrokeCap`
- `CanvasStrokeStyle.LineJoin` → `SKStrokeJoin`
- `CanvasDashStyle.Dash` → `SKPathEffect.CreateDash(...)`

### Text alignment and layout

Text drawing uses manual alignment helpers to emulate Win2D text alignment semantics.
Text measurements are computed using `SKFont.MeasureText(...)`, and text origin is adjusted based on:

- `CanvasHorizontalAlignment` (`Left`, `Center`, `Right`)
- `CanvasVerticalAlignment` (`Top`, `Center`, `Bottom`)

### Geometry and path conversion

`CanvasGeometry.SendPathTo(...)` reads Skia path verbs and emits Win2D path calls via `ICanvasPathReceiver`.
This is a compact bridge for consumers that need geometry-based vector operations without implementing a full Win2D path builder.

## Extension and future improvements

The design intentionally keeps the implementation extensible for future compatibility:

- Add more Win2D drawing methods to `CanvasDrawingSession` as needed
- Add `CanvasPathBuilder`, `CanvasGeometry.CreateRectangle`, and path intersection support
- Add richer text layout support in `CanvasTextLayout` and line wrapping
- Add resource-backed brush types like `CanvasSolidColorBrush`
- Add `CanvasVirtualImageSource` or `CanvasRenderTarget` integration with XAML visuals

## Implementation approach

The current shim is implemented as a direct translation layer:

- `CanvasDevice` and `CanvasRenderTarget` manage Skia rendering surfaces
- `CanvasDrawingSession` acts as the Win2D API facade over `SKCanvas`
- `CanvasTextFormat` resolves fonts and caches `SKTypeface`
- `CanvasGeometry` converts Skia path data back into Win2D geometry events
- `CanvasControl` adapts Skia rendering into Uno's XAML painting lifecycle

This is the recommended approach for future expansion: keep Win2D semantic wrappers thin, and implement the heavy lifting in SkiaSharp primitives.

## File boundaries

The shim is organized by feature area:

- `src/Win2D.UnoCompat/Canvas/` — core compatibility types and primitives
- `src/Win2D.UnoCompat/Canvas/Text/` — text format and layout
- `src/Win2D.UnoCompat/Canvas/Geometry/` — geometry and path semantics
- `src/Win2D.UnoCompat/Canvas/UI/Xaml/` — Uno/XAML control integration

## Summary

The Win2D compatibility shim on SkiaSharp should remain:

- API-compatible enough for common Win2D drawing patterns
- small and maintainable
- focused on core canvas, text, and geometry scenarios
- integrated cleanly with Uno/XAML via `SKXamlCanvas`
- DPI-aware and coordinate-consistent across platforms

This design forms the basis for expanding the shim gradually while preserving Win2D-style developer expectations.

---

## Parity Matrix & Session Roadmap

### Overall coverage: ~89% (~529 / ~600 public members across 124 types)

Legend: ✅ Implemented | 🔶 Partial | ❌ Not started

| Type | Members Done | Real Win2D | Coverage | Status |
|------|:-----------:|:----------:|:--------:|:------:|
| CanvasDevice | 23 | ~50 | 46% | 🔶 Device creation, properties, event (remaining ~50 count is mostly Direct3D11-interop specific, N/A for a 2D-only shim) |
| CanvasRenderTarget | 17 | ~30 | 57% | 🔶 Lifecycle + pixel access (incl. regional GetPixelColors) + save |
| CanvasDrawingSession | 70 | ~200+ | 35% | 🔶 Shapes, geometry, text (incl. no-format DrawText overloads), layers (incl. geometry clip), blend, DrawImage (incl. true perspective quad), DrawInk |
| CanvasTextFormat | 31 | ~30 | 100%+ | ✅ Full parity |
| CanvasTextLayout | 80 | ~60 | 100%+ | ✅ Full parity + extras |
| CanvasGeometry | 27 | ~40 | 68% | 🔶 Factories + combine + stroke + hit-testing + path builder |
| CanvasStrokeStyle | 9 | ~9 | 100%+ | ✅ Full parity (all real properties implemented; ~15 count in earlier notes was stale) |
| CanvasControl | 16 | ~20 | 80% | 🔶 Draw + Invalidate + DPI handling |
| CanvasBitmap | 34 | ~30 | 100%+ | ✅ Full parity |
| CanvasCommandList | 10 | ~15 | 67% | 🔶 Surface-backed record/replay + GetBounds(resourceCreator[, transform]) + resourceCreator ctor |
| CanvasImageSource | 13 | ~10 | 100%+ | ✅ Surface-backed snapshot source + Device/Size + resourceCreator ctor |
| CanvasVirtualImageSource | 7 | ~10 | 70% | 🔶 Large surface + invalid region tracking (list + GetInvalidRegions) + resourceCreator ctor |
| CanvasAnimatedControl | 18 | ~20 | 90% | 🔶 Tickable game-loop + resource/update events |
| CanvasSpriteBatch | 14 | ~10 | 100%+ | ✅ Full parity |
| Brushes | 26 | ~20+ | 100%+ | ✅ Solid/image/linear/radial fill brushes |
| Effects | 96 | ~50+ | 100%+ | ✅ 16 effect types + DrawImage + PixelShaderEffect |
| CanvasPathBuilder | 14 | ~15+ | 93% | ✅ Full path building API |
| Printing | 6 | ~10 | 60% | 🔶 PDF rendering path |
| SVG / Ink / Typography | 21 | ~20+ | 100%+ | ✅ SVG, ink strokes, typography features |

### Session plan

| Session | Area | Target Coverage | Effort |
|:-------:|------|:--------------:|:------:|
| 1 | P0 baseline | ✅ 9% | 3h |
| 2 | CanvasDrawingSession: ellipses, geometry, textlayout, transform, blend | ✅ 11% | 4h |
| 3 | CanvasGeometry: shape factories + geometric operations | ✅ 13% (16 new members) | 5h |
| 4 | CanvasPathBuilder | ✅ 20% | 4h |
| 5 | CanvasTextFormat: weight, stretch, trimming, spacing | ✅ 24% | 4h |
| 6 | CanvasTextLayout: cluster metrics, range properties, DrawToBitmap | ✅ 27% | 5h |
| 7 | CanvasBitmap: load/save/copy/create | ✅ 33% | 5h |
| 8 | Brushes: SolidColor, Image, Gradient | ✅ 37% | 5h |
| 9 | CanvasCommandList + CanvasCachedGeometry | ✅ 40% | 4h |
| 10 | Effects pipeline: blur, color matrix, shadow, composite, etc. | ✅ 50% | 6.5h |
| 11 | ImageSource + VirtualImageSource + SpriteBatch | ✅ 55% | 5h |
| 12 | CanvasAnimatedControl + SwapChainPanel | ✅ 60% | 4.5h |
| 13 | Remaining: Printing, SVG, Ink, Typography | ✅ 70% | 6h |
| 14 | Win2D compatibility test suite: 46/46 passing | ✅ 82% | 8h |
| 15 | CanvasDrawingSession: DrawEllipse(Rect, brush), DrawCachedGeometry offsets, geometry-clip layers, sprite batch sort mode, DrawGlyphRun overloads, DrawInk, DrawImage(effect, point), DrawImage(destinationQuad) | ✅ 86% | 4h |
| 16 | CanvasImageSource: Device/Size/SizeInPixels; CanvasVirtualImageSource: CreateDrawingSession(color,rect), GetInvalidRegions(); re-audited CanvasDevice against real API | ✅ 87% | 2h |
| 17 | CanvasRenderTarget: regional GetPixelColors; CanvasCommandList: GetBounds(resourceCreator[, transform]); re-audited CanvasStrokeStyle (already complete) | ✅ 88% | 2h |
| 18 | CanvasDrawingSession: no-format DrawText overloads (Rect/point, color/brush) | ✅ 88% | 1h |
| 19 | CanvasDrawingSession: true perspective transform for DrawImage(destinationQuad) (Heckbert unit-square-to-quad homography, replacing the earlier 3-point affine approximation); CanvasTextFormat.FontSize now defaults to 20 (was 0, invisible) | ✅ 88% | 2h |
| 20 | Added the real Win2D `ICanvasResourceCreator`/`ICanvasResourceCreatorWithDpi`-based constructors to CanvasCommandList, CanvasImageSource, CanvasVirtualImageSource | ✅ 89% | 1.5h |

Each session's detailed plan is in `docs/session{N}.md`.
Target after all 20 sessions: ~89% API coverage covering all commonly-used Win2D scenarios.
