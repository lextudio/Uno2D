# Uno2D Win2D Parity - Session 6: CanvasTextLayout Expansion

## Session goal

Complete `CanvasTextLayout` from the current ~14 members to near-full coverage, adding cluster metrics, per-property getters/setters, and DrawToBitmap support.

## Current state

`CanvasTextLayout` has: constructor, `Text`, `Format`, `MaxWidth`, `MaxHeight`, `LayoutBounds`, `DrawBounds`, `LayoutBoundsIncludingTrailingWhitespace`, `LineMetrics`, `GetCaretPosition`, `HitTest`, `GetCharacterRegions`, `CreatePath`, `Dispose`.

## Progress update

- Added `RequestedSize`, `MinimumSize`, and `ClusterMetrics`.
- Added range override APIs for font size, locale, underline, strikethrough, and drawing effect.
- Added `DrawToBitmap(byte[] pixels, int x, int y, int w, int h)` for off-screen text rasterization into BGRA pixels.
- Added tests for cluster metrics, requested/minimum size, range property overrides, and text raster output.

Current limitation: range font-size overrides are stored and queryable, but the simple single-run Skia text layout still measures/renders using the base format. Full mixed-style shaping remains future work.

## Desired outcomes

1. Cluster metrics for fine-grained glyph-level layout data
2. Per-property getter/setter pair for font sizing, locale, strikethrough, underline, drawing effects
3. DrawToBitmap for off-screen text rendering
4. All new members covered by tests

## API to add

### Metrics

| Member | Type | Notes |
|--------|------|-------|
| `ClusterMetrics` | `CanvasClusterMetrics[]` | Array of cluster info (glyph count, width, properties) |
| `RequestedSize` | `Size` | The size requested at construction |
| `MinimumSize` | `Size` | Minimum size that fits the text |

### Per-property getters/setters

| Method | Returns | Notes |
|--------|---------|-------|
| `GetFontSize(int charIndex)` | `float` | Font size at given character |
| `SetFontSize(int charIndex, int charCount, float fontSize)` | void | Range override |
| `GetLocale(int charIndex)` | `string` | Locale at given character |
| `SetLocale(int charIndex, int charCount, string locale)` | void | Range override |
| `GetStrikethrough(int charIndex)` | `bool` | Strike state |
| `SetStrikethrough(int charIndex, int charCount, bool strikethrough)` | void | Range override |
| `GetUnderline(int charIndex)` | `bool` | Underline state |
| `SetUnderline(int charIndex, int charCount, bool underline)` | void | Range override |
| `DrawingEffect` | `ICanvasBrush` | Default drawing effect for layout |
| `SetDrawingEffect(int charIndex, int charCount, ICanvasBrush)` | void | Range override |

### Rendering

| Method | Notes |
|--------|-------|
| `DrawToBitmap(byte[] pixels, int x, int y, int w, int h)` | Render layout into pixel buffer |

## Work items

1. **ClusterMetrics** — Compute per-cluster data from SKia font metrics: cluster position, width, glyph count. Cache after computation.

2. **Range property API** — Implement character-range property overrides as sparse overlay arrays. `GetFontSize(i)` returns override if set, else format default. Store as sorted interval trees.

3. **DrawingEffect** — Brush interface `ICanvasBrush` (compatible with both future CanvasSolidColorBrush and Skia paints). Apply as paint override during text drawing.

4. **DrawToBitmap** — Create a temporary SKSurface, render text layout, copy pixels back to caller buffer.

5. **RequestedSize / MinimumSize** — Store construction args for RequestedSize. Compute MinimumSize by measuring text with no width constraint.

## Files to modify

- `Canvas/Text/CanvasTextLayout.cs` — add all new members
- New file: `Canvas/Text/CanvasClusterMetrics.cs` — struct
- `Canvas/Text/CanvasTextLayoutMetrics.cs` — may need expansion
- `tests/` — range getter/setter tests, cluster metrics test, DrawToBitmap test

## Exit criteria

- ClusterMetrics functional — complete for single-line cluster reporting
- 5+ range property APIs working (Get/Set font size, locale, strikethrough, underline) — complete as queryable range overlays
- DrawToBitmap produces valid pixel output — complete
- 3+ tests passing — complete
- Parity matrix updated

## Estimated effort

- Core implementation: 3h
- Tests: 2h
- Total: ~5h
