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
