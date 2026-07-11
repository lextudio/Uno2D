# Uno2D Win2D Parity - Session 8: Brushes

## Session goal

Implement the Win2D brush hierarchy — `CanvasSolidColorBrush`, `CanvasImageBrush`, `CanvasLinearGradientBrush`, `CanvasRadialGradientBrush` — enabling textured, gradient, and colored fill operations in drawing sessions.

## Current state

Brush coverage is now implemented for solid color, image, linear gradient, and radial gradient fills.

## Progress update

- Added `ICanvasBrush`, `ICanvasSolidColorBrush`, and internal Skia brush adapter.
- Added `CanvasSolidColorBrush` with color and opacity.
- Added `CanvasImageBrush` with image, extend behavior, optional source rect, opacity, and transform.
- Added `CanvasLinearGradientBrush` and `CanvasRadialGradientBrush` with gradient stops, opacity, edge behavior, and transform.
- Added `CanvasEdgeBehavior` and `CanvasGradientStop`.
- Added brush overloads for `FillRectangle`, `FillCircle`, `FillEllipse`, `FillGeometry`, and bounded `DrawText`.
- Added tests for solid opacity fill, repeated image brush, linear gradient, radial gradient, geometry fill, and text brush rendering.

Current limitation: brush support is focused on fill/text paths. Stroke brush overloads, richer image stretch modes, and advanced brush interpolation options remain future work.

## Desired outcomes

1. `CanvasSolidColorBrush` for simple colored fills with opacity
2. `CanvasImageBrush` for tiled/stretched bitmap fills
3. `CanvasLinearGradientBrush` for linear gradient fills
4. `CanvasRadialGradientBrush` for radial gradient fills
5. Updated `CanvasDrawingSession` to accept brush variants of existing methods

## API to add

### Base / Interface

| Type | Notes |
|------|-------|
| `ICanvasBrush` | Marker interface for all brushes |
| `ICanvasSolidColorBrush` | Color + Opacity |

### Concrete types

| Type | Key properties | Skia mapping |
|------|----------------|-------------|
| `CanvasSolidColorBrush` | `Color`, `Opacity` | `SKColor` + alpha |
| `CanvasImageBrush` | `Image` (CanvasBitmap), `ExtendX/Y`, `SourceRect`, `Transform` | `SKShader.CreateBitmap` |
| `CanvasLinearGradientBrush` | `StartPoint`, `EndPoint`, `Stops[]`, `ExtendX/Y`, `Transform` | `SKShader.CreateLinearGradient` |
| `CanvasRadialGradientBrush` | `Center`, `Radius`, `Stops[]`, `ExtendX/Y` | `SKShader.CreateRadialGradient` |

### Enums

| Enum | Values |
|------|--------|
| `CanvasEdgeBehavior` | `Clamp`, `Wrap`, `Mirror` |
| `CanvasGradientStop` | struct: `Position`, `Color` |

### DrawingSession overloads

Add brush-accepting overloads for:
- `FillRectangle(Rect, ICanvasBrush)`
- `FillCircle(float, float, float, ICanvasBrush)`
- `FillEllipse(Rect, ICanvasBrush)`
- `FillGeometry(CanvasGeometry, ICanvasBrush)`
- `DrawText(string, Rect, ICanvasBrush, CanvasTextFormat)`

## Work items

1. **CanvasSolidColorBrush** — Implement as thin wrapper around `Color` + `Opacity`. Apply via `SKPaint.Color` with alpha blending.

2. **CanvasImageBrush** — Map to `SKShader.CreateBitmap`. Handle `ExtendX`/`ExtendY` → `SKShaderTileMode`. Apply `Transform` via shader local matrix.

3. **Gradient brushes** — `LinearGradient` maps to `SKShader.CreateLinearGradient`; `RadialGradient` to `CreateRadialGradient`. Color stops with positions.

4. **Brush overloads** — Add overloaded `Fill*` methods in `CanvasDrawingSession` that take `ICanvasBrush`. Create `SKPaint` from brush (shader + color + opacity) per draw call.

5. **Snapshot tests** — Solid fill grid, image brush tile, linear gradient (horizontal/vertical/diagonal), radial gradient concentric, gradient with different EdgeBehaviors.

## Files to modify

- New file: `Canvas/Brushes/ICanvasBrush.cs` — interfaces
- New file: `Canvas/Brushes/CanvasSolidColorBrush.cs`
- New file: `Canvas/Brushes/CanvasImageBrush.cs`
- New file: `Canvas/Brushes/CanvasLinearGradientBrush.cs`
- New file: `Canvas/Brushes/CanvasRadialGradientBrush.cs`
- New file: `Canvas/Brushes/CanvasEdgeBehavior.cs` — enum
- New file: `Canvas/Brushes/CanvasGradientStop.cs` — struct
- `Canvas/CanvasDrawingSession.cs` — brush overload methods
- `tests/` — brush tests and snapshot renders

## Exit criteria

- All 4 brush types functional — complete
- Brush overloads for FillRectangle, FillCircle, FillGeometry, DrawText — complete; `FillEllipse` also covered
- 5+ snapshot tests passing (each brush type + combined) — complete with pixel assertions
- Parity matrix updated — complete

## Estimated effort

- Core implementation: 3h
- Tests + snapshots: 2h
- Total: ~5h
