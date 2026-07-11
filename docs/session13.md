# Uno2D Win2D Parity - Session 13: Remaining Features

## Session goal

Implement the remaining Win2D API surface: Printing, SVG rendering, Ink support, Typography APIs, and miscellaneous types. This is the final session to reach comprehensive Win2D coverage.

## Current state

Core coverage exists for printing, SVG, ink, and typography.

## Progress update

- Added `CanvasPrintDevice`, `CanvasPrintDocument`, and `CanvasPrintPageEventArgs`; documents can render pages to PDF streams through Skia.
- Added `CanvasSvgDocument` with stream loading, size extraction, and basic `rect`/`circle` rendering.
- Added `CanvasInkStroke`, `CanvasInkStrokeBuilder`, and `CanvasInk` with stroke rendering through `CanvasDrawingSession`.
- Added `CanvasTypography` and `CanvasTypographyFeatureName` feature storage APIs.
- Added tests for PDF generation, SVG rendering, ink stroke drawing, and typography feature storage.

Current limitation: platform print dialogs, full SVG grammar, variable-width ink simulation, and actual OpenType shaping feature application are not fully implemented. The added APIs provide compile-time and core rendering coverage for common/basic scenarios.

## Desired outcomes

1. `CanvasPrintDocument` + `CanvasPrintDevice` for printing support
2. `CanvasSvgDocument` for SVG rendering
3. `CanvasInk` for ink stroke rendering
4. `CanvasTypography` for advanced OpenType features

## API to add

### Printing

| Type | Notes |
|------|-------|
| `CanvasPrintDocument` | Print document with page drawing |
| `CanvasPrintDevice` | Print device abstraction |
| `PrintPage` event | Page rasterization callback |

Implementation: On Skia, render each page to an `SKSurface`, encode to PNG/SVG raster, send to platform print API. On macOS, use NSPrintOperation. On Linux, generate PDF via Skia's PDF backend (`SKDocument`). On WASM, use window.print() with rendered content.

### SVG

| Type | Notes |
|------|-------|
| `CanvasSvgDocument` | Load/parse SVG |
| `LoadAsync(Stream)` | Load from stream |
| `Draw(CanvasDrawingSession, Rect)` | Render to canvas |
| `GetSize()` | SVG document dimensions |

Implementation: Skia has `SKSVGDOM` (in `SkiaSharp.Svg` or via `SkiaSharp.Extended`). Wrap it for SVG loading and rendering.

### Ink

| Type | Notes |
|------|-------|
| `CanvasInkStroke` | Single ink stroke (points, properties) |
| `CanvasInkStrokeBuilder` | Build strokes from raw input |
| `CanvasInk` | Collection of strokes |
| `Draw(CanvasDrawingSession)` | Render all strokes |

Implementation: Convert ink stroke points to `SKPath` with variable-width stroke simulation (width interpolated between points). Apply `SKPaint.StrokeCap.Round` and `StrokeJoin.Round`.

### Typography

| Type | Notes |
|------|-------|
| `CanvasTypography` | OpenType feature overrides |
| `AddFeature(CanvasTypographyFeatureName, int value)` | Set feature |
| `CanvasTypographyFeatureName` | Enum: Kerning, Ligatures, etc. |

Implementation: Map to `SKFont` feature overrides via `SKFont.CreateTypeface` with feature settings.

## Work items

1. **Printing** — Skia PDF backend integration. Page draw callback → render to PDF page. Platform-specific print dialog integration.

2. **SVG** — Wrap SkiaSVGDOM. Load from stream/string. Draw at target rect with aspect ratio preservation.

3. **Ink strokes** — Build paths from control points with width interpolation. Round cap/join. Multi-stroke rendering.

4. **Typography** — `SKTypeface` feature setting via font table overrides. Simple C# API for Kerning, Ligatures, StylisticSets.

5. **Integration tests** — Print to PDF (verify file validity). SVG load + render. Ink stroke draw (pixel check). Typography feature affects glyph selection (test with font-dependent feature).

## Files to modify

- New directory: `Canvas/Printing/`
- New directory: `Canvas/Svg/`
- New directory: `Canvas/Ink/`
- New directory: `Canvas/Typography/`
- `tests/` — per-feature tests

## Exit criteria

- PrintDocument generates valid PDF — complete
- SVG renders correctly (basic shapes + text) — basic shapes complete; text pending
- Ink strokes render with correct variable-width appearance — constant-width stroke rendering complete; variable width pending
- Typography features are settable and affect glyph rendering — feature storage complete; shaping application pending
- 4+ tests passing — complete
- Full parity matrix updated — complete

## Estimated effort

- Core implementation: 4h
- Tests: 2h
- Total: ~6h
