# Uno2D Win2D Parity - Session 5: CanvasTextFormat Expansion

## Session goal

Complete `CanvasTextFormat` from its current 6 properties to a near-full implementation covering font weight, stretch, style, line spacing, trimming, and other commonly-used text formatting options.

## Current state

`CanvasTextFormat` has: `FontFamily`, `FontSize`, `HorizontalAlignment`, `VerticalAlignment`, `WordWrapping`, `ResolveTypeface()`.

## Progress update

- Added font weight/stretch/style properties and Skia typeface resolution.
- Added locale, line spacing, optical alignment, trimming, direction, word/letter spacing, and paragraph alignment properties.
- Wired `CanvasTextLayout` measurement, caret positions, hit testing, bounds, and line metrics to word/letter spacing and uniform line spacing.
- Added reusable trimming helper and applied it to bounded `CanvasDrawingSession.DrawText`.
- Verified with focused tests for typeface cache, spacing, uniform line spacing, and character/word trimming.

## Desired outcomes

1. All major text format properties implemented
2. Font weight/stretch/style resolve correctly to Skia `SKTypeface`
3. Line spacing and trimming behavior matches Win2D semantics
4. Backward compatibility: existing code unaffected

## API to add

### Font properties

| Property | Type | Skia mapping |
|----------|------|--------------|
| `FontWeight` | `CanvasFontWeight` | `SKFontStyleWeight` |
| `FontStretch` | `CanvasFontStretch` | `SKFontStyleWidth` |
| `FontStyle` | `CanvasFontStyle` | `SKFontStyleSlant` |
| `Locale` | `string` | typeface family filtering |

### Line properties

| Property | Type | Notes |
|----------|------|-------|
| `LineSpacing` | `float` | Override line height; 0 = default |
| `LineSpacingMethod` | `CanvasLineSpacingMethod` | Default or Uniform |
| `OpticalAlignment` | `bool` | Approximate via side-bearing adjustments |

### Trimming

| Property | Type | Notes |
|----------|------|-------|
| `TrimmingGranularity` | `CanvasTrimmingGranularity` | None, Character, Word |
| `TrimmingDelimiter` | `string` | typically `...` |
| `TrimmingDelimiterCount` | `int` | Number of delimiters shown |

### Direction / Spacing

| Property | Type | Notes |
|----------|------|-------|
| `Direction` | `CanvasTextDirection` | LTR / RTL |
| `WordSpacing` | `float` | Additional space between words |
| `LetterSpacing` | `float` | SKPaint.TextTracking |

### Paragraph

| Property | Type | Notes |
|----------|------|-------|
| `ParagraphAlignment` | `CanvasParagraphAlignment` | Near, Far, Center |

## Work items

1. **FontWeight/Stretch/Style** — Map Win2D enums to SKFontStyle (100-950, 1-9, Upright/Italic/Oblique). Update ResolveTypeface() to use these.

2. **Locale** — Pass through to SKTypeface creation.

3. **LineSpacing** — Default mode uses Skia default; Uniform forces LineSpacing height per line.

4. **Trimming** — Measure text; if exceeds layout width, truncate at delimiter boundary. Character vs Word granularity.

5. **Direction** — LTR default; RTL maps to SKTextDirection.

6. **WordSpacing/LetterSpacing** — Applied as paint-level overrides during DrawText.

7. **ParagraphAlignment** — Affects horizontal alignment within text layout bounds.

## Files to modify

- `Canvas/Text/CanvasTextFormat.cs` — add all new properties
- New enum files: `CanvasFontWeight.cs`, `CanvasFontStretch.cs`, `CanvasFontStyle.cs`, `CanvasLineSpacingMethod.cs`, `CanvasTrimmingGranularity.cs`, `CanvasTextDirection.cs`, `CanvasParagraphAlignment.cs`
- `Canvas/Text/CanvasTextLayout.cs` — respect new format properties in layout
- `tests/` — format property tests

## Exit criteria

- 15+ new format properties added and functional — mostly complete
- All Skia typeface mappings verified — compile/test covered; visual/font-family-specific coverage still needed
- 3+ snapshot tests showing distinct weight/stretch/trimming output — pending visual snapshot tests
- Parity matrix updated

## Estimated effort

- Core implementation: 2.5h
- Tests + snapshots: 1.5h
- Total: ~4h
