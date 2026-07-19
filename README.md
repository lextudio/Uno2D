# Win2D.UnoCompat

Win2D.UnoCompat is a desktop-first compatibility shim that provides a subset of the [Win2D](https://github.com/microsoft/Win2D) API surface for [Uno Platform](https://platform.uno) apps.

It translates `Microsoft.Graphics.Canvas` drawing and text APIs into [SkiaSharp](https://github.com/mono/SkiaSharp) operations so Uno projects can compile and run without the native Win2D package on non-Windows targets.

Current scope:

- **124 public types** covering ~89% of the Win2D public API surface (~529 / ~600 members)
- Full drawing session with shapes, geometry, text, layers, transforms, and blend modes
- Bitmap load/save/create/copy with pixel-level access
- Effects pipeline: blur, shadow, color matrix, composite, blend, crop, transform, color management, and custom PixelShaderEffect with HLSL compilation
- Brushes: solid color, image, linear/radial gradient
- Geometry: shape factories, combine, stroke, hit-testing, path builder
- Text: format, layout, cluster metrics, font face
- Controls: CanvasControl, CanvasAnimatedControl
- Image sources: CanvasImageSource, CanvasVirtualImageSource
- Sprite batching, printing, SVG, ink, typography
- Keep public namespaces close to Win2D so existing source code needs fewer changes
- Track parity explicitly and avoid pretending unsupported Win2D APIs are complete

## Compatibility

Win2D.UnoCompat includes a dedicated compatibility test suite that links Win2D's official C# test files and runs them against the Uno2D implementation.

**Current status: 46 / 46 tests passing** across all major API areas:

| Area | Tests | Status |
|------|:-----:|:------:|
| CanvasDevice | 3 | ✅ |
| CanvasRenderTarget | 3 | ✅ |
| CanvasBitmap (load/save/pixels) | 10 | ✅ |
| CanvasDrawingSession (DrawImage) | 3 | ✅ |
| Effects (blur, shadow, color matrix, composite, etc.) | 5 | ✅ |
| PixelShaderEffect (HLSL compilation, properties, validation) | 15 | ✅ |
| CanvasCommandList | 2 | ✅ |
| CanvasImageSource | 1 | ✅ |
| CanvasSpriteBatch | 1 | ✅ |
| CanvasAnimatedControl | 1 | ✅ |
| CanvasGeometry | 1 | ✅ |
| CanvasPrintDocument | 1 | ✅ |

## Code Coverage

| Metric | Value |
|--------|:-----:|
| Sequence points | 83.0% |
| Branch coverage | 56.3% |
| Excluding auto-generated code | 85.9% |

## Supported Platforms

- Windows 11 (Windows 10 may work but is not a primary target)
- macOS, 3 most recent versions from 2023-2025
- Ubuntu latest LTS (other Linux distributions may work but are not primary targets)

> If you are looking for support of a specific platform, business sponsorship is the way to accelerate that work. Please reach out to us at [homepage](https://lextudio.com).

## Get Started

One NuGet package is available:

- [![NuGet](https://img.shields.io/nuget/v/LeXtudio.Win2D.UnoCompat.svg?label=LeXtudio.Win2D.UnoCompat)](https://www.nuget.org/packages/LeXtudio.Win2D.UnoCompat) Win2D-compatible drawing and text primitives backed by SkiaSharp.

### Default usage

```shell
dotnet add package LeXtudio.Win2D.UnoCompat
```

```xml
xmlns:canvas="using:Microsoft.Graphics.Canvas.UI.Xaml"

<canvas:CanvasControl Draw="Canvas_Draw" />
```

```csharp
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas.Text;
using Windows.UI;

private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
{
    var ds = args.DrawingSession;
    ds.DrawLine(12, 12, 240, 12, Colors.DeepSkyBlue, 2);
    ds.DrawRectangle(12, 32, 180, 72, Colors.OrangeRed, 1);
    ds.DrawText("Hello from Win2D.UnoCompat", 16, 56, Colors.White, new CanvasTextFormat
    {
        FontFamily = "Segoe UI",
        FontSize = 18
    });
}
```

## Current Status

Stable preview (v0.x.y) releases are available on NuGet.

This package is a compatibility shim that covers **~89% of the Win2D public API surface** (~529 / ~600 public members across 124 types). The remaining gaps are in niche areas not yet needed by LeXtudio Uno tools.

## TODO Items Before v1.0.0

- [x] Drawing session parity (shapes, geometry, text, layers, transform, blend)
- [x] Bitmap load/save/create/copy with pixel-level access
- [x] Effects pipeline (blur, shadow, color matrix, composite, crop, transform, color management, custom PixelShaderEffect)
- [x] Brushes (solid, image, linear/radial gradient)
- [x] Geometry operations (combine, stroke, hit-test, path builder)
- [x] Controls (CanvasControl, CanvasAnimatedControl)
- [x] Image sources (CanvasImageSource, CanvasVirtualImageSource)
- [x] Sprite batching, printing, SVG, ink, typography
- [x] Win2D official C# test suite: 46/46 passing
- [ ] Add visual regression samples
- [ ] Publish v1.0.0 stable release

## License

Win2D.UnoCompat is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## Copyright

Copyright (c) 2026 LeXtudio, Inc. All rights reserved.