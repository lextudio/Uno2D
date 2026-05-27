# Win2D.UnoCompat

A small compatibility shim project that provides a subset of the Win2D API surface for use with Uno Platform apps.

This library translates `Microsoft.Graphics.Canvas` drawing and text APIs into `SkiaSharp` operations so Uno projects can compile and run without the native Win2D package on non-Windows targets.

## Purpose

- Provide a reusable `Win2D.UnoCompat` library for the `Win-Icon-Finder` project.
- Offer a lightweight `CanvasControl` and drawing session abstraction using SkiaSharp.
- Support basic Win2D-style rendering, strokes, shapes, and text layout.

## Requirements

- .NET 8.0
- SkiaSharp 3.119.1
- Uno Platform compatible runtime

## Build

From the project root:

```bash
cd /Users/lextm/uno-tools/Win2D.UnoCompat/src/Win2D.UnoCompat
dotnet build -p:Platform=x64
```

## Project layout

- `Canvas/` — core Win2D-compatible drawing primitives and geometry helpers
- `Canvas/Text/` — text format and layout support
- `Canvas/UI/Xaml/` — `CanvasControl` implementation for Uno/SkiaSharp

## License

This project is licensed under the MIT License. See `LICENSE`.

## Copyright

Copyright (c) 2026 LeXtudio Inc. All rights reserved.
