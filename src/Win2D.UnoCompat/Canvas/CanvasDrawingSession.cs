using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.Graphics.DirectX;
using Windows.UI;
using Windows.Graphics.Effects;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.Svg;

namespace Microsoft.Graphics.Canvas
{
    public sealed class CanvasDrawingSession : IDisposable
    {
        private readonly SKCanvas _canvas;
        private readonly CanvasDevice? _device;

        // State tracking for properties that affect paint creation
        private CanvasAntialiasing _antialiasing = CanvasAntialiasing.Antialiased;
        private CanvasTextAntialiasing _textAntialiasing = CanvasTextAntialiasing.Auto;
        private CanvasBlend _blend = CanvasBlend.SourceOver;
        private CanvasUnits _units = CanvasUnits.Dips;
        private bool _disposed;

        public CanvasDrawingSession(SKCanvas canvas, CanvasDevice? device = null, float dpi = 96f)
        {
            _canvas = canvas;
            _device = device;
            Dpi = dpi;
        }

        // ── State properties ──────────────────────────────────────────

        public System.Numerics.Matrix3x2 Transform
        {
            get => FromSKMatrix(_canvas.TotalMatrix);
            set => _canvas.SetMatrix(ToSKMatrix(value));
        }

        public CanvasAntialiasing Antialiasing
        {
            get => _antialiasing;
            set => _antialiasing = value;
        }

        public CanvasTextAntialiasing TextAntialiasing
        {
            get => _textAntialiasing;
            set => _textAntialiasing = value;
        }

        public CanvasBlend Blend
        {
            get => _blend;
            set => _blend = value;
        }

        public CanvasUnits Units
        {
            get => _units;
            set => _units = value;
        }

        public float Dpi { get; set; } = 96f;

        public CanvasDevice Device => _device ?? CanvasDevice.GetSharedDevice();

        public CanvasBufferPrecision EffectBufferPrecision { get; set; } = CanvasBufferPrecision.Precision8Bit;

        public float EffectTileSize { get; set; } = 0f;

        public CanvasTextRenderingParameters? TextRenderingParameters { get; set; }

        // ── Control ───────────────────────────────────────────────────

        public void Flush()
        {
            _canvas.Flush();
        }

        internal void DrawSkImage(SKImage image, float x, float y)
        {
            using var paint = CreateImagePaint();
            _canvas.DrawImage(image, x, y, paint);
        }

        // ── Clear ─────────────────────────────────────────────────────

        public void Clear(Color color)
        {
            _canvas.Clear(ToSkColor(color));
        }

        public void Clear(System.Numerics.Vector4 color)
        {
            _canvas.Clear(new SKColor(
                (byte)Math.Clamp(color.X * 255, 0, 255),
                (byte)Math.Clamp(color.Y * 255, 0, 255),
                (byte)Math.Clamp(color.Z * 255, 0, 255),
                (byte)Math.Clamp(color.W * 255, 0, 255)));
        }

        // ── Circle ────────────────────────────────────────────────────

        public void FillCircle(float x, float y, float radius, Color color)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Fill);
            _canvas.DrawCircle(x, y, radius, paint);
        }

        public void FillCircle(Vector2 center, float radius, Color color)
        {
            FillCircle(center.X, center.Y, radius, color);
        }

        public void FillCircle(float x, float y, float radius, ICanvasBrush brush)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Fill);
            _canvas.DrawCircle(x, y, radius, paint);
        }

        public void FillCircle(Vector2 center, float radius, ICanvasBrush brush)
        {
            FillCircle(center.X, center.Y, radius, brush);
        }

        public void DrawCircle(float x, float y, float radius, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawCircle(x, y, radius, paint);
        }

        public void DrawCircle(Vector2 center, float radius, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            DrawCircle(center.X, center.Y, radius, color, strokeWidth, strokeStyle);
        }

        public void DrawCircle(float x, float y, float radius, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawCircle(x, y, radius, paint);
        }

        public void DrawCircle(Vector2 center, float radius, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            DrawCircle(center.X, center.Y, radius, brush, strokeWidth, strokeStyle);
        }

        // ── Ellipse ───────────────────────────────────────────────────

        public void FillEllipse(float cx, float cy, float rx, float ry, Color color)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Fill);
            _canvas.DrawOval(cx, cy, rx, ry, paint);
        }

        public void FillEllipse(Vector2 center, float rx, float ry, Color color)
        {
            FillEllipse(center.X, center.Y, rx, ry, color);
        }

        public void FillEllipse(float cx, float cy, float rx, float ry, ICanvasBrush brush)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Fill);
            _canvas.DrawOval(cx, cy, rx, ry, paint);
        }

        public void FillEllipse(Vector2 center, float rx, float ry, ICanvasBrush brush)
        {
            FillEllipse(center.X, center.Y, rx, ry, brush);
        }

        public void DrawEllipse(float cx, float cy, float rx, float ry, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawOval(cx, cy, rx, ry, paint);
        }

        public void DrawEllipse(Vector2 center, float rx, float ry, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            DrawEllipse(center.X, center.Y, rx, ry, color, strokeWidth, strokeStyle);
        }

        public void DrawEllipse(float cx, float cy, float rx, float ry, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawOval(cx, cy, rx, ry, paint);
        }

        public void DrawEllipse(Vector2 center, float rx, float ry, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            DrawEllipse(center.X, center.Y, rx, ry, brush, strokeWidth, strokeStyle);
        }

        public void FillEllipse(Rect rect, Color color)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Fill);
            _canvas.DrawOval(ToSkRect(rect), paint);
        }

        public void FillEllipse(Rect rect, ICanvasBrush brush)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Fill);
            _canvas.DrawOval(ToSkRect(rect), paint);
        }

        public void DrawEllipse(Rect rect, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawOval(ToSkRect(rect), paint);
        }

        public void DrawEllipse(Rect rect, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawOval(ToSkRect(rect), paint);
        }

        // ── Line ──────────────────────────────────────────────────────

        public void DrawLine(float x1, float y1, float x2, float y2, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawLine(x1, y1, x2, y2, paint);
        }

        public void DrawLine(Vector2 point0, Vector2 point1, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            DrawLine(point0.X, point0.Y, point1.X, point1.Y, color, strokeWidth, strokeStyle);
        }

        public void DrawLine(float x1, float y1, float x2, float y2, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawLine(x1, y1, x2, y2, paint);
        }

        public void DrawLine(Vector2 point0, Vector2 point1, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            DrawLine(point0.X, point0.Y, point1.X, point1.Y, brush, strokeWidth, strokeStyle);
        }

        public void DrawRectangle(float x, float y, float width, float height, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawRect(x, y, width, height, paint);
        }

        public void DrawRectangle(Rect rect, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            DrawRectangle((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, color, strokeWidth, strokeStyle);
        }

        public void DrawRectangle(float x, float y, float width, float height, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawRect(x, y, width, height, paint);
        }

        public void DrawRectangle(Rect rect, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            DrawRectangle((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, brush, strokeWidth, strokeStyle);
        }

        public void FillRectangle(float x, float y, float width, float height, Color color)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Fill);
            _canvas.DrawRect(x, y, width, height, paint);
        }

        public void FillRectangle(Rect rect, Color color)
        {
            FillRectangle((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, color);
        }

        public void FillRectangle(float x, float y, float width, float height, ICanvasBrush brush)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Fill);
            _canvas.DrawRect(x, y, width, height, paint);
        }

        public void FillRectangle(Rect rect, ICanvasBrush brush)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Fill);
            _canvas.DrawRect(ToSkRect(rect), paint);
        }

        public void FillRectangle(Rect rect, ICanvasBrush brush, ICanvasBrush opacityBrush)
        {
            using var layerPaint = CreatePaint(opacityBrush, SKPaintStyle.Fill);
            _canvas.SaveLayer(layerPaint);
            FillRectangle(rect, brush);
            _canvas.Restore();
        }

        public void FillRectangle(float x, float y, float width, float height, ICanvasBrush brush, ICanvasBrush opacityBrush)
        {
            using var layerPaint = CreatePaint(opacityBrush, SKPaintStyle.Fill);
            _canvas.SaveLayer(layerPaint);
            FillRectangle(x, y, width, height, brush);
            _canvas.Restore();
        }

        public void DrawRoundedRectangle(float x, float y, float width, float height, float radiusX, float radiusY, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawRoundRect(new SKRect(x, y, x + width, y + height), radiusX, radiusY, paint);
        }

        public void DrawRoundedRectangle(Rect rect, float radiusX, float radiusY, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            DrawRoundedRectangle((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, radiusX, radiusY, color, strokeWidth, strokeStyle);
        }

        public void DrawRoundedRectangle(float x, float y, float width, float height, float radiusX, float radiusY, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawRoundRect(new SKRect(x, y, x + width, y + height), radiusX, radiusY, paint);
        }

        public void DrawRoundedRectangle(Rect rect, float radiusX, float radiusY, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            DrawRoundedRectangle((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, radiusX, radiusY, brush, strokeWidth, strokeStyle);
        }

        public void FillRoundedRectangle(float x, float y, float width, float height, float radiusX, float radiusY, Color color)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Fill);
            _canvas.DrawRoundRect(new SKRect(x, y, x + width, y + height), radiusX, radiusY, paint);
        }

        public void FillRoundedRectangle(Rect rect, float radiusX, float radiusY, Color color)
        {
            FillRoundedRectangle((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, radiusX, radiusY, color);
        }

        public void FillRoundedRectangle(float x, float y, float width, float height, float radiusX, float radiusY, ICanvasBrush brush)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Fill);
            _canvas.DrawRoundRect(new SKRect(x, y, x + width, y + height), radiusX, radiusY, paint);
        }

        public void FillRoundedRectangle(Rect rect, float radiusX, float radiusY, ICanvasBrush brush)
        {
            FillRoundedRectangle((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height, radiusX, radiusY, brush);
        }

        public void FillGeometry(CanvasGeometry geometry, Color color)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Fill);
            _canvas.DrawPath(geometry.GetPath(), paint);
        }

        public void FillGeometry(CanvasGeometry geometry, Vector2 offset, Color color)
        {
            _canvas.Save();
            _canvas.Translate(offset.X, offset.Y);
            FillGeometry(geometry, color);
            _canvas.Restore();
        }

        public void FillGeometry(CanvasGeometry geometry, float offsetX, float offsetY, Color color)
        {
            FillGeometry(geometry, new Vector2(offsetX, offsetY), color);
        }

        public void FillGeometry(CanvasGeometry geometry, ICanvasBrush brush)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Fill);
            _canvas.DrawPath(geometry.GetPath(), paint);
        }

        public void FillGeometry(CanvasGeometry geometry, ICanvasBrush brush, ICanvasBrush opacityBrush)
        {
            using var layerPaint = CreatePaint(opacityBrush, SKPaintStyle.Fill);
            _canvas.SaveLayer(layerPaint);
            FillGeometry(geometry, brush);
            _canvas.Restore();
        }

        public void FillGeometry(CanvasGeometry geometry, Vector2 offset, ICanvasBrush brush)
        {
            _canvas.Save();
            _canvas.Translate(offset.X, offset.Y);
            FillGeometry(geometry, brush);
            _canvas.Restore();
        }

        public void FillGeometry(CanvasGeometry geometry, float offsetX, float offsetY, ICanvasBrush brush)
        {
            FillGeometry(geometry, new Vector2(offsetX, offsetY), brush);
        }

        public void DrawGeometry(CanvasGeometry geometry, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawPath(geometry.GetPath(), paint);
        }

        public void DrawGeometry(CanvasGeometry geometry, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawPath(geometry.GetPath(), paint);
        }

        public void DrawGeometry(CanvasGeometry geometry, Vector2 offset, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            _canvas.Save();
            _canvas.Translate(offset.X, offset.Y);
            DrawGeometry(geometry, color, strokeWidth, strokeStyle);
            _canvas.Restore();
        }

        public void DrawGeometry(CanvasGeometry geometry, float offsetX, float offsetY, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            DrawGeometry(geometry, new Vector2(offsetX, offsetY), color, strokeWidth, strokeStyle);
        }

        public void DrawGeometry(CanvasGeometry geometry, Vector2 offset, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            _canvas.Save();
            _canvas.Translate(offset.X, offset.Y);
            DrawGeometry(geometry, brush, strokeWidth, strokeStyle);
            _canvas.Restore();
        }

        public void DrawGeometry(CanvasGeometry geometry, float offsetX, float offsetY, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            DrawGeometry(geometry, new Vector2(offsetX, offsetY), brush, strokeWidth, strokeStyle);
        }

        public void DrawCachedGeometry(CanvasCachedGeometry cachedGeometry, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            ArgumentNullException.ThrowIfNull(cachedGeometry);
            using SKPath path = cachedGeometry.GetPath();
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawPath(path, paint);
        }

        public void DrawCachedGeometry(CanvasCachedGeometry cachedGeometry, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            ArgumentNullException.ThrowIfNull(cachedGeometry);
            using SKPath path = cachedGeometry.GetPath();
            using var paint = CreatePaint(brush, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawPath(path, paint);
        }

        public void DrawCachedGeometry(CanvasCachedGeometry cachedGeometry, Vector2 offset, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            _canvas.Save();
            _canvas.Translate(offset.X, offset.Y);
            DrawCachedGeometry(cachedGeometry, color, strokeWidth, strokeStyle);
            _canvas.Restore();
        }

        public void DrawCachedGeometry(CanvasCachedGeometry cachedGeometry, float offsetX, float offsetY, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            DrawCachedGeometry(cachedGeometry, new Vector2(offsetX, offsetY), color, strokeWidth, strokeStyle);
        }

        public void DrawCachedGeometry(CanvasCachedGeometry cachedGeometry, Vector2 offset, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            _canvas.Save();
            _canvas.Translate(offset.X, offset.Y);
            DrawCachedGeometry(cachedGeometry, brush, strokeWidth, strokeStyle);
            _canvas.Restore();
        }

        public void DrawCachedGeometry(CanvasCachedGeometry cachedGeometry, float offsetX, float offsetY, ICanvasBrush brush, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            DrawCachedGeometry(cachedGeometry, new Vector2(offsetX, offsetY), brush, strokeWidth, strokeStyle);
        }

        public void DrawSvg(CanvasSvgDocument svgDocument, Rect destinationRect)
        {
            ArgumentNullException.ThrowIfNull(svgDocument);
            svgDocument.Draw(this, destinationRect);
        }

        internal void FillSkPath(SKPath path, Color color)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Fill);
            _canvas.DrawPath(path, paint);
        }

        internal void FillSkPath(SKPath path, ICanvasBrush brush)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Fill);
            _canvas.DrawPath(path, paint);
        }

        internal void DrawSkPath(SKPath path, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawPath(path, paint);
        }

        // ── Glyph Run ─────────────────────────────────────────────────

        public void DrawGlyphRun(Vector2 point, float[] fontOffsets, ushort[] glyphIndices, float[] glyphAdvances, CanvasFontFace fontFace, float fontSize, Color color)
        {
            using var paint = CreateTextPaint(color);
            using var skFont = new SKFont
            {
                Size = fontSize,
                Typeface = fontFace?.Typeface ?? SKTypeface.Default,
            };
            using var builder = new SKTextBlobBuilder();
            var run = builder.AllocatePositionedRun(skFont, glyphIndices.Length);
            float currentX = point.X;
            for (int i = 0; i < glyphIndices.Length; i++)
            {
                float ox = fontOffsets != null && i * 2 < fontOffsets.Length ? fontOffsets[i * 2] : 0;
                float oy = fontOffsets != null && i * 2 + 1 < fontOffsets.Length ? fontOffsets[i * 2 + 1] : 0;
                run.Glyphs[i] = glyphIndices[i];
                run.Positions[i] = new SKPoint(currentX + ox, point.Y + oy);
                currentX += glyphAdvances != null && i < glyphAdvances.Length ? glyphAdvances[i] : 0;
            }
            using var blob = builder.Build();
            _canvas.DrawText(blob, 0, 0, paint);
        }

        public void DrawGlyphRun(Vector2 point, float[] fontOffsets, ushort[] glyphIndices, float[] glyphAdvances, CanvasFontFace fontFace, float fontSize, ICanvasBrush brush)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Fill);
            using var skFont = new SKFont
            {
                Size = fontSize,
                Typeface = fontFace?.Typeface ?? SKTypeface.Default,
            };
            using var builder = new SKTextBlobBuilder();
            var run = builder.AllocatePositionedRun(skFont, glyphIndices.Length);
            float currentX = point.X;
            for (int i = 0; i < glyphIndices.Length; i++)
            {
                float ox = fontOffsets != null && i * 2 < fontOffsets.Length ? fontOffsets[i * 2] : 0;
                float oy = fontOffsets != null && i * 2 + 1 < fontOffsets.Length ? fontOffsets[i * 2 + 1] : 0;
                run.Glyphs[i] = glyphIndices[i];
                run.Positions[i] = new SKPoint(currentX + ox, point.Y + oy);
                currentX += glyphAdvances != null && i < glyphAdvances.Length ? glyphAdvances[i] : 0;
            }
            using var blob = builder.Build();
            _canvas.DrawText(blob, 0, 0, paint);
        }

        public void DrawGlyphRun(Vector2 point, float[] fontOffsets, ushort[] glyphIndices, float[] glyphAdvances, CanvasFontFace fontFace, float fontSize, Color color, uint bidiLevel, CanvasTextMeasuringMode measuringMode = CanvasTextMeasuringMode.Natural, CanvasGlyphOrientation glyphOrientation = CanvasGlyphOrientation.Upright)
        {
            DrawGlyphRun(point, fontOffsets, glyphIndices, glyphAdvances, fontFace, fontSize, color);
        }

        public void DrawGlyphRun(Vector2 point, float[] fontOffsets, ushort[] glyphIndices, float[] glyphAdvances, CanvasFontFace fontFace, float fontSize, Color color, uint bidiLevel, ushort colorPaletteIndex, CanvasTextMeasuringMode measuringMode = CanvasTextMeasuringMode.Natural, CanvasGlyphOrientation glyphOrientation = CanvasGlyphOrientation.Upright)
        {
            DrawGlyphRun(point, fontOffsets, glyphIndices, glyphAdvances, fontFace, fontSize, color);
        }

        // ── Ink ───────────────────────────────────────────────────────

        public void DrawInk(IEnumerable<Ink.CanvasInkStroke> inkStrokes)
        {
            ArgumentNullException.ThrowIfNull(inkStrokes);
            foreach (Ink.CanvasInkStroke stroke in inkStrokes)
            {
                for (int i = 1; i < stroke.Points.Count; i++)
                {
                    Vector2 a = stroke.Points[i - 1];
                    Vector2 b = stroke.Points[i];
                    DrawLine(a.X, a.Y, b.X, b.Y, stroke.Color, stroke.Width);
                }
            }
        }

        public void DrawInk(IEnumerable<Ink.CanvasInkStroke> inkStrokes, bool highContrast)
        {
            DrawInk(inkStrokes);
        }

        // ── Text ──────────────────────────────────────────────────────

        public void DrawText(string text, Rect bounds, Color color, CanvasTextFormat format)
        {
            using SKFont font = CreateTextFont(format);
            using var paint = CreateTextPaint(color);
            text = format.ApplyTrimming(text, (float)bounds.Width);
            SKRect textBounds = default;
            font.MeasureText(text, out textBounds);
            (float drawX, float drawY) = ComputeTopLeftAlignedTextOrigin(
                textBounds,
                (float)bounds.X,
                (float)bounds.Y,
                (float)bounds.Width,
                (float)bounds.Height,
                format.HorizontalAlignment,
                format.VerticalAlignment);
            _canvas.DrawText(text, drawX, drawY - textBounds.Top, font, paint);
        }

        public void DrawText(string text, Rect bounds, ICanvasBrush brush, CanvasTextFormat format)
        {
            using SKFont font = CreateTextFont(format);
            using var paint = CreatePaint(brush, SKPaintStyle.Fill);
            text = format.ApplyTrimming(text, (float)bounds.Width);
            SKRect textBounds = default;
            font.MeasureText(text, out textBounds);
            (float drawX, float drawY) = ComputeTopLeftAlignedTextOrigin(
                textBounds,
                (float)bounds.X,
                (float)bounds.Y,
                (float)bounds.Width,
                (float)bounds.Height,
                format.HorizontalAlignment,
                format.VerticalAlignment);
            _canvas.DrawText(text, drawX, drawY - textBounds.Top, font, paint);
        }

        public void DrawText(string text, float x, float y, Color color, CanvasTextFormat format)
        {
            using SKFont font = CreateTextFont(format);
            using var paint = CreateTextPaint(color);
            SKRect textBounds = default;
            font.MeasureText(text, out textBounds);
            (float drawX, _) = ComputePointAlignedTextOrigin(
                textBounds,
                x,
                y,
                format.HorizontalAlignment,
                format.VerticalAlignment);
            SKFontMetrics metrics = font.Metrics;
            float ascent = -metrics.Ascent;
            float descent = metrics.Descent;
            float baseline = format.VerticalAlignment switch
            {
                CanvasVerticalAlignment.Center => y + (ascent - descent) / 2f,
                CanvasVerticalAlignment.Bottom => y - descent,
                _ => y + ascent,
            };
            _canvas.DrawText(text, drawX, baseline, font, paint);
        }

        public void DrawText(string text, Vector2 point, Color color)
        {
            DrawText(text, point.X, point.Y, color, new CanvasTextFormat());
        }

        public void DrawText(string text, Vector2 point, Color color, CanvasTextFormat format)
        {
            DrawText(text, point.X, point.Y, color, format);
        }

        public void DrawText(string text, float x, float y, Color color)
        {
            DrawText(text, x, y, color, new CanvasTextFormat());
        }

        public void DrawText(string text, Rect bounds, Color color)
        {
            DrawText(text, bounds, color, new CanvasTextFormat());
        }

        public void DrawText(string text, Rect bounds, ICanvasBrush brush)
        {
            DrawText(text, bounds, brush, new CanvasTextFormat());
        }

        public void DrawText(string text, Vector2 point, ICanvasBrush brush)
        {
            DrawText(text, point.X, point.Y, brush, new CanvasTextFormat());
        }

        public void DrawText(string text, float x, float y, ICanvasBrush brush)
        {
            DrawText(text, x, y, brush, new CanvasTextFormat());
        }

        public void DrawText(string text, Vector2 point, ICanvasBrush brush, CanvasTextFormat format)
        {
            DrawText(text, point.X, point.Y, brush, format);
        }

        public void DrawText(string text, float x, float y, ICanvasBrush brush, CanvasTextFormat format)
        {
            using SKFont font = CreateTextFont(format);
            using var paint = CreatePaint(brush, SKPaintStyle.Fill);
            SKRect textBounds = default;
            font.MeasureText(text, out textBounds);
            (float drawX, _) = ComputePointAlignedTextOrigin(
                textBounds,
                x,
                y,
                format.HorizontalAlignment,
                format.VerticalAlignment);
            SKFontMetrics metrics = font.Metrics;
            float ascent = -metrics.Ascent;
            float descent = metrics.Descent;
            float baseline = format.VerticalAlignment switch
            {
                CanvasVerticalAlignment.Center => y + (ascent - descent) / 2f,
                CanvasVerticalAlignment.Bottom => y - descent,
                _ => y + ascent,
            };
            _canvas.DrawText(text, drawX, baseline, font, paint);
        }

        public void DrawText(string text, float x, float y, float w, float h, ICanvasBrush brush, CanvasTextFormat format)
        {
            DrawText(text, new Rect(x, y, w, h), brush, format);
        }

        public void DrawText(string text, float x, float y, float w, float h, Color color, CanvasTextFormat format)
        {
            DrawText(text, new Rect(x, y, w, h), color, format);
        }

        public void DrawTextLayout(CanvasTextLayout textLayout, float x, float y, Color color)
        {
            using SKFont font = CreateTextFont(textLayout.Format);
            using var paint = CreateTextPaint(color);
            _canvas.DrawText(textLayout.Text, x, y + font.Metrics.Ascent, font, paint);
        }

        public void DrawTextLayout(CanvasTextLayout textLayout, Vector2 point, Color color)
        {
            DrawTextLayout(textLayout, point.X, point.Y, color);
        }

        public void DrawTextLayout(CanvasTextLayout textLayout, float x, float y, ICanvasBrush brush)
        {
            using SKFont font = CreateTextFont(textLayout.Format);
            using var paint = CreatePaint(brush, SKPaintStyle.Fill);
            _canvas.DrawText(textLayout.Text, x, y + font.Metrics.Ascent, font, paint);
        }

        public void DrawTextLayout(CanvasTextLayout textLayout, Vector2 point, ICanvasBrush brush)
        {
            DrawTextLayout(textLayout, point.X, point.Y, brush);
        }

        // ── Image ────────────────────────────────────────────────────

        public void DrawImage(CanvasBitmap bitmap, Rect destinationRect)
        {
            ArgumentNullException.ThrowIfNull(bitmap);
            using var paint = CreateImagePaint();
            _canvas.DrawBitmap(bitmap.Bitmap, ToSkRect(destinationRect), paint);
        }

        public void DrawImage(CanvasBitmap bitmap, float x, float y)
        {
            ArgumentNullException.ThrowIfNull(bitmap);
            using var paint = CreateImagePaint();
            _canvas.DrawBitmap(bitmap.Bitmap, x, y, paint);
        }

        public void DrawImage(CanvasBitmap bitmap, Rect destinationRect, Rect sourceRect)
        {
            ArgumentNullException.ThrowIfNull(bitmap);
            using var paint = CreateImagePaint();
            _canvas.DrawBitmap(bitmap.Bitmap, ToSkRect(sourceRect), ToSkRect(destinationRect), paint);
        }

        public void DrawImage(CanvasBitmap bitmap, Rect destinationRect, Rect sourceRect, Color tint)
        {
            ArgumentNullException.ThrowIfNull(bitmap);
            using var paint = CreateImagePaint();
            paint.ColorFilter = SKColorFilter.CreateBlendMode(ToSkColor(tint), SKBlendMode.Modulate);
            _canvas.DrawBitmap(bitmap.Bitmap, ToSkRect(sourceRect), ToSkRect(destinationRect), paint);
        }

        public void DrawImage(ICanvasImage image)
        {
            DrawImage(image, new Rect(0, 0, 0, 0));
        }

        public void DrawImage(ICanvasImage image, Rect destinationRect)
        {
            if (image is IGraphicsEffectSource effectSource)
            {
                var sourceDevice = CanvasEffect.GetDevice(effectSource);
                if (sourceDevice is not null && sourceDevice != _device)
                    throw new ArgumentException("Effect source #0 is associated with a different device.");
            }

            if (image is PixelShaderEffect pse)
                pse.ValidateAndThrow(_device);

            if (image is CanvasBitmap bitmap)
            {
                if (!IsPixelFormatDrawable(bitmap.Format))
                    throw new ArgumentException("The bitmap pixel format is unsupported. 0x88982F80");
            }

            var imageRect = new SKRect((float)destinationRect.X, (float)destinationRect.Y, (float)(destinationRect.X + destinationRect.Width), (float)(destinationRect.Y + destinationRect.Height));
            var skImage = CanvasEffect.ResolveImage(image);
            _canvas.DrawImage(skImage, imageRect);
        }

        public void DrawImage(ICanvasImage image, float x, float y)
        {
            DrawImage(image, new Rect(x, y, 0, 0));
        }

        public void DrawImage(ICanvasImage image, Rect destinationRect, Rect sourceRect)
        {
            var src = ToSkRect(sourceRect);
            var dst = ToSkRect(destinationRect);
            var skImage = CanvasEffect.ResolveImage(image);
            using var paint = CreateImagePaint();
            _canvas.DrawImage(skImage, src, dst, paint);
        }

        public void DrawImage(ICanvasImage image, Rect destinationRect, float opacity)
        {
            var skImage = CanvasEffect.ResolveImage(image);
            using var paint = CreateImagePaint();
            paint.Color = new SKColor(255, 255, 255, (byte)(opacity * 255));
            _canvas.DrawImage(skImage, ToSkRect(destinationRect), paint);
        }

        public void DrawImage(ICanvasImage image, float x, float y, float opacity)
        {
            DrawImage(image, new Rect(x, y, 0, 0));
        }

        public void DrawImage(ICanvasImage image, Rect destinationRect, Rect sourceRect, float opacity)
        {
            DrawImage(image, destinationRect, sourceRect);
        }

        public void DrawImage(ICanvasImage image, Rect destinationRect, Rect sourceRect, float opacity, CanvasImageInterpolation interpolation)
        {
            var src = ToSkRect(sourceRect);
            var dst = ToSkRect(destinationRect);
            var skImage = CanvasEffect.ResolveImage(image);
            using var paint = CreateImagePaint();
            paint.Color = new SKColor(255, 255, 255, (byte)(opacity * 255));
            _canvas.DrawImage(skImage, src, dst, paint);
        }

        public void DrawImage(ICanvasImage image, Vector2[] destinationQuad, Rect sourceRectangle, float opacity = 1f, CanvasImageInterpolation interpolation = CanvasImageInterpolation.Linear)
        {
            ArgumentNullException.ThrowIfNull(image);
            if (destinationQuad is null || destinationQuad.Length != 4)
                throw new ArgumentException("destinationQuad must contain exactly 4 points.", nameof(destinationQuad));

            var skImage = CanvasEffect.ResolveImage(image);
            var src = ToSkRect(sourceRectangle);

            // Full projective (perspective) transform: source-rect space -> unit square -> destination quad.
            // Skia's rasterizer perspective-corrects texture sampling for images drawn under a projective CTM,
            // so this renders true perspective, not just an affine approximation.
            SKMatrix rectToUnitSquare = ComputeRectToUnitSquare(src);
            SKMatrix squareToQuad = ComputeUnitSquareToQuad(
                new SKPoint(destinationQuad[0].X, destinationQuad[0].Y),
                new SKPoint(destinationQuad[1].X, destinationQuad[1].Y),
                new SKPoint(destinationQuad[2].X, destinationQuad[2].Y),
                new SKPoint(destinationQuad[3].X, destinationQuad[3].Y));
            SKMatrix matrix = squareToQuad.PreConcat(rectToUnitSquare);

            using var paint = CreateImagePaint();
            paint.Color = new SKColor(255, 255, 255, (byte)Math.Clamp(opacity * 255, 0, 255));
            paint.FilterQuality = interpolation == CanvasImageInterpolation.NearestNeighbor ? SKFilterQuality.None : SKFilterQuality.Medium;

            _canvas.Save();
            _canvas.Concat(in matrix);
            _canvas.ClipRect(src);
            _canvas.DrawImage(skImage, 0, 0, paint);
            _canvas.Restore();
        }

        // Maps the source rect's own coordinate space onto the unit square [0,1]x[0,1].
        private static SKMatrix ComputeRectToUnitSquare(SKRect rect)
        {
            float width = rect.Width == 0 ? 1 : rect.Width;
            float height = rect.Height == 0 ? 1 : rect.Height;
            return new SKMatrix(
                1f / width, 0, -rect.Left / width,
                0, 1f / height, -rect.Top / height,
                0, 0, 1);
        }

        // Maps the unit square [0,1]x[0,1] onto an arbitrary (possibly non-affine) quadrilateral,
        // per Paul Heckbert's "Fundamentals of Texture Mapping and Image Warping" (1989), §3.
        private static SKMatrix ComputeUnitSquareToQuad(SKPoint p0, SKPoint p1, SKPoint p2, SKPoint p3)
        {
            float dx1 = p1.X - p2.X, dx2 = p3.X - p2.X, dx3 = p0.X - p1.X + p2.X - p3.X;
            float dy1 = p1.Y - p2.Y, dy2 = p3.Y - p2.Y, dy3 = p0.Y - p1.Y + p2.Y - p3.Y;

            if (dx3 == 0 && dy3 == 0)
            {
                // Affine case: destination is a parallelogram.
                return new SKMatrix(
                    p1.X - p0.X, p2.X - p1.X, p0.X,
                    p1.Y - p0.Y, p2.Y - p1.Y, p0.Y,
                    0, 0, 1);
            }

            float denom = dx1 * dy2 - dx2 * dy1;
            float g = denom == 0 ? 0 : (dx3 * dy2 - dx2 * dy3) / denom;
            float h = denom == 0 ? 0 : (dx1 * dy3 - dx3 * dy1) / denom;

            return new SKMatrix(
                p1.X - p0.X + g * p1.X, p3.X - p0.X + h * p3.X, p0.X,
                p1.Y - p0.Y + g * p1.Y, p3.Y - p0.Y + h * p3.Y, p0.Y,
                g, h, 1);
        }

        private static bool IsPixelFormatDrawable(DirectXPixelFormat format)
        {
            return format switch
            {
                DirectXPixelFormat.A8UIntNormalized or
                DirectXPixelFormat.R8UIntNormalized or
                DirectXPixelFormat.R8G8UIntNormalized => false,
                _ => true,
            };
        }

        public void DrawImage(CanvasEffect effect, Rect destinationRect)
        {
            ArgumentNullException.ThrowIfNull(effect);
            using SKImage image = effect.GetImage();
            using var paint = CreateImagePaint();
            _canvas.DrawImage(image, ToSkRect(destinationRect), paint);
        }

        public void DrawImage(CanvasEffect effect, float x, float y)
        {
            ArgumentNullException.ThrowIfNull(effect);
            using SKImage image = effect.GetImage();
            using var paint = CreateImagePaint();
            _canvas.DrawImage(image, x, y, paint);
        }

        public void DrawImage(CanvasEffect effect, Vector2 offset)
        {
            DrawImage(effect, offset.X, offset.Y);
        }

        public void DrawImage(CanvasEffect effect, Rect destinationRect, Rect sourceRect)
        {
            ArgumentNullException.ThrowIfNull(effect);
            using SKImage image = effect.GetImage();
            using var paint = CreateImagePaint();
            _canvas.DrawImage(image, ToSkRect(sourceRect), ToSkRect(destinationRect), paint);
        }

        // ── Sprite Batch ──────────────────────────────────────────────

        public CanvasSpriteBatch CreateSpriteBatch()
        {
            return new CanvasSpriteBatch(this);
        }

        public CanvasSpriteBatch CreateSpriteBatch(CanvasSpriteSortMode sortMode)
        {
            return new CanvasSpriteBatch(this) { SortMode = sortMode };
        }

        public CanvasLayer CreateLayer(float opacity)
        {
            using var layerPaint = new SKPaint { Color = new SKColor(0, 0, 0, (byte)(opacity * 255)) };
            _canvas.SaveLayer(layerPaint);
            return new CanvasLayer(_canvas);
        }

        public CanvasLayer CreateLayer(ICanvasBrush opacityBrush)
        {
            using var paint = CreatePaint(opacityBrush, SKPaintStyle.Fill);
            _canvas.SaveLayer(paint);
            return new CanvasLayer(_canvas);
        }

        public CanvasLayer CreateLayer(float opacity, Rect clipRect)
        {
            using var layerPaint = new SKPaint { Color = new SKColor(0, 0, 0, (byte)(opacity * 255)) };
            _canvas.SaveLayer(ToSkRect(clipRect), layerPaint);
            return new CanvasLayer(_canvas);
        }

        public CanvasLayer CreateLayer(ICanvasBrush opacityBrush, Rect clipRect)
        {
            using var paint = CreatePaint(opacityBrush, SKPaintStyle.Fill);
            _canvas.SaveLayer(ToSkRect(clipRect), paint);
            return new CanvasLayer(_canvas);
        }

        public CanvasLayer CreateLayer(float opacity, CanvasGeometry clipGeometry)
        {
            _canvas.Save();
            _canvas.ClipPath(clipGeometry.GetPath());
            using var layerPaint = new SKPaint { Color = new SKColor(0, 0, 0, (byte)(opacity * 255)) };
            _canvas.SaveLayer(layerPaint);
            return new CanvasLayer(_canvas, extraSave: true);
        }

        public CanvasLayer CreateLayer(ICanvasBrush opacityBrush, CanvasGeometry clipGeometry)
        {
            _canvas.Save();
            _canvas.ClipPath(clipGeometry.GetPath());
            using var paint = CreatePaint(opacityBrush, SKPaintStyle.Fill);
            _canvas.SaveLayer(paint);
            return new CanvasLayer(_canvas, extraSave: true);
        }

        public CanvasActiveLayer CreateLayer(float opacity, CanvasLayerOptions options)
        {
            using var layerPaint = new SKPaint
            {
                Color = new SKColor(0, 0, 0, (byte)(opacity * 255)),
            };
            _canvas.SaveLayer(layerPaint);
            return new CanvasActiveLayer(_canvas);
        }

        // ── Private helpers ───────────────────────────────────────────

        private static (float x, float y) ComputeTopLeftAlignedTextOrigin(
            SKRect textBounds,
            float rectX,
            float rectY,
            float rectWidth,
            float rectHeight,
            CanvasHorizontalAlignment hAlign,
            CanvasVerticalAlignment vAlign)
        {
            float x = rectX;
            float y = rectY;

            if (hAlign == CanvasHorizontalAlignment.Center)
                x += (rectWidth - textBounds.Width) / 2f;
            else if (hAlign == CanvasHorizontalAlignment.Right)
                x += rectWidth - textBounds.Width;

            if (vAlign == CanvasVerticalAlignment.Center)
                y += (rectHeight - textBounds.Height) / 2f;
            else if (vAlign == CanvasVerticalAlignment.Bottom)
                y += rectHeight - textBounds.Height;

            return (x, y);
        }

        private static (float x, float y) ComputePointAlignedTextOrigin(
            SKRect textBounds,
            float anchorX,
            float anchorY,
            CanvasHorizontalAlignment hAlign,
            CanvasVerticalAlignment vAlign)
        {
            float x = anchorX;
            float y = anchorY;

            if (hAlign == CanvasHorizontalAlignment.Center)
                x -= textBounds.Width / 2f;
            else if (hAlign == CanvasHorizontalAlignment.Right)
                x -= textBounds.Width;

            if (vAlign == CanvasVerticalAlignment.Center)
                y -= textBounds.Height / 2f;
            else if (vAlign == CanvasVerticalAlignment.Bottom)
                y -= textBounds.Height;

            return (x, y);
        }

        private static SKFont CreateTextFont(CanvasTextFormat format)
        {
            return new SKFont(format.ResolveTypeface(), format.FontSize);
        }

        private SKPaint CreateTextPaint(Color color)
        {
            return new SKPaint
            {
                Color = ToSkColor(color),
                IsAntialias = _textAntialiasing != CanvasTextAntialiasing.Aliased,
            };
        }

        private SKPaint CreatePaint(Color color, SKPaintStyle style, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            var paint = new SKPaint
            {
                Color = ToSkColor(color),
                Style = style,
                StrokeWidth = strokeWidth,
                IsAntialias = _antialiasing != CanvasAntialiasing.Aliased,
                BlendMode = ToSkBlendMode(_blend),
            };

            if (strokeStyle is { })
            {
                paint.StrokeCap = strokeStyle.GetSkStrokeCap(strokeStyle.StartCap);
                paint.StrokeJoin = strokeStyle.GetSkStrokeJoin(strokeStyle.LineJoin);
                paint.StrokeCap = strokeStyle.GetSkStrokeCap(strokeStyle.DashCap);
                var dash = strokeStyle.GetDashEffect();
                if (dash is not null)
                    paint.PathEffect = dash;
            }

            return paint;
        }

        private SKPaint CreatePaint(ICanvasBrush brush, SKPaintStyle style)
        {
            ArgumentNullException.ThrowIfNull(brush);
            if (brush is not ISkiaCanvasBrush skiaBrush)
                throw new ArgumentException("Unsupported brush implementation.", nameof(brush));

            return skiaBrush.CreatePaint(style, _antialiasing != CanvasAntialiasing.Aliased, ToSkBlendMode(_blend));
        }

        private SKPaint CreatePaint(ICanvasBrush brush, SKPaintStyle style, float strokeWidth, CanvasStrokeStyle? strokeStyle)
        {
            var paint = CreatePaint(brush, style);
            paint.StrokeWidth = strokeWidth;
            if (strokeStyle is not null)
            {
                paint.StrokeCap = strokeStyle.GetSkStrokeCap(strokeStyle.StartCap);
                paint.StrokeJoin = strokeStyle.GetSkStrokeJoin(strokeStyle.LineJoin);
                paint.StrokeCap = strokeStyle.GetSkStrokeCap(strokeStyle.DashCap);
                var dash = strokeStyle.GetDashEffect();
                if (dash is not null)
                    paint.PathEffect = dash;
            }
            return paint;
        }

        private SKPaint CreateImagePaint()
        {
            return new SKPaint
            {
                IsAntialias = _antialiasing != CanvasAntialiasing.Aliased,
                BlendMode = ToSkBlendMode(_blend),
            };
        }

        private static SKColor ToSkColor(Color color)
            => new(color.R, color.G, color.B, color.A);

        private static SKRect ToSkRect(Rect rect)
            => new((float)rect.X, (float)rect.Y, (float)(rect.X + rect.Width), (float)(rect.Y + rect.Height));

        private static SKBlendMode ToSkBlendMode(CanvasBlend blend) => blend switch
        {
            CanvasBlend.Copy => SKBlendMode.Src,
            CanvasBlend.Add => SKBlendMode.Plus,
            CanvasBlend.Subtract => SKBlendMode.SrcOut,
            CanvasBlend.ReverseSubtract => SKBlendMode.DstOut,
            CanvasBlend.Modulate => SKBlendMode.Modulate,
            CanvasBlend.Multiply => SKBlendMode.Multiply,
            _ => SKBlendMode.SrcOver,
        };

        // ── Matrix conversion ──────────────────────────────────────────

        // Matrix3x2 is row-major: [x' y'] = [x y 1] * [M11 M12; M21 M22; M31 M32]
        // SKMatrix is column-major convention: [x'; y'; 1] = SKMatrix * [x; y; 1]
        //   x' = ScaleX * x + SkewX * y + TransX
        //   y' = SkewY  * x + ScaleY * y + TransY
        // Mapping: ScaleX=M11, SkewX=M21, TransX=M31, SkewY=M12, ScaleY=M22, TransY=M32

        private static System.Numerics.Matrix3x2 FromSKMatrix(SKMatrix m)
        {
            return new System.Numerics.Matrix3x2(
                m.ScaleX, m.SkewY,
                m.SkewX, m.ScaleY,
                m.TransX, m.TransY);
        }

        private static SKMatrix ToSKMatrix(System.Numerics.Matrix3x2 m)
        {
            return new SKMatrix
            {
                ScaleX = m.M11,
                SkewX = m.M21,
                TransX = m.M31,
                SkewY = m.M12,
                ScaleY = m.M22,
                TransY = m.M32,
                Persp0 = 0,
                Persp1 = 0,
                Persp2 = 1,
            };
        }

        // ── IDisposable ───────────────────────────────────────────────

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }

    // ── CanvasLayer ───────────────────────────────────────────────────
    // Represents a transparency layer. Dispose pops the layer stack.

    public sealed class CanvasLayer : IDisposable
    {
        private SKCanvas? _canvas;
        private readonly bool _extraSave;

        internal CanvasLayer(SKCanvas canvas, bool extraSave = false)
        {
            _canvas = canvas;
            _extraSave = extraSave;
        }

        public void Dispose()
        {
            if (_canvas is not null)
            {
                _canvas.Restore();
                if (_extraSave)
                    _canvas.Restore();
                _canvas = null;
            }
        }
    }
}
