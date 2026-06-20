# Win2D.UnoCompat

Win2D.UnoCompat is a desktop-first compatibility shim that provides a subset of the [Win2D](https://github.com/microsoft/Win2D) API surface for [Uno Platform](https://platform.uno) apps.

It translates `Microsoft.Graphics.Canvas` drawing and text APIs into [SkiaSharp](https://github.com/mono/SkiaSharp) operations so Uno projects can compile and run without the native Win2D package on non-Windows targets.

Current scope:

- Provide a lightweight `CanvasControl` for Uno Skia applications.
- Support basic drawing sessions, strokes, shapes, geometry, and text layout.
- Keep public namespaces close to Win2D so existing source code needs fewer changes.
- Track parity explicitly and avoid pretending unsupported Win2D APIs are complete.

## Screenshot & Video

TODO

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

Early preview (v0.x.y) releases are available on NuGet.

This package is a compatibility shim, not a full Win2D reimplementation. It currently focuses on the subset needed by LeXtudio Uno tools and related samples.

## TODO Items Before v1.0.0

- [ ] Expand drawing session parity
- [ ] Improve text layout fidelity
- [ ] Add more geometry operations
- [ ] Complete parity reporting in package documentation
- [ ] Add visual regression samples

## License

Win2D.UnoCompat is licensed under the MIT License. See [LICENSE](LICENSE) for details.

## Copyright

Copyright (c) 2026 LeXtudio, Inc. All rights reserved.
