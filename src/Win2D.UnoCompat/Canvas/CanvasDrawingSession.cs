using SkiaSharp;
using System;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
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

        public CanvasDrawingSession(SKCanvas canvas, CanvasDevice? device = null)
        {
            _canvas = canvas;
            _device = device;
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

        // ── Circle ────────────────────────────────────────────────────

        public void FillCircle(float x, float y, float radius, Color color)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Fill);
            _canvas.DrawCircle(x, y, radius, paint);
        }

        public void FillCircle(float x, float y, float radius, ICanvasBrush brush)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Fill);
            _canvas.DrawCircle(x, y, radius, paint);
        }

        public void DrawCircle(float x, float y, float radius, Color color, float strokeWidth = 1f)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth);
            _canvas.DrawCircle(x, y, radius, paint);
        }

        // ── Ellipse ───────────────────────────────────────────────────

        public void FillEllipse(float cx, float cy, float rx, float ry, Color color)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Fill);
            _canvas.DrawOval(cx, cy, rx, ry, paint);
        }

        public void DrawEllipse(float cx, float cy, float rx, float ry, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawOval(cx, cy, rx, ry, paint);
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

        // ── Line ──────────────────────────────────────────────────────

        public void DrawLine(float x1, float y1, float x2, float y2, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawLine(x1, y1, x2, y2, paint);
        }

        // ── Rectangle ─────────────────────────────────────────────────

        public void DrawRectangle(float x, float y, float width, float height, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawRect(x, y, width, height, paint);
        }

        public void FillRectangle(float x, float y, float width, float height, Color color)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Fill);
            _canvas.DrawRect(x, y, width, height, paint);
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

        // ── Rounded Rectangle ─────────────────────────────────────────

        public void DrawRoundedRectangle(float x, float y, float width, float height, float radiusX, float radiusY, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawRoundRect(new SKRect(x, y, x + width, y + height), radiusX, radiusY, paint);
        }

        public void FillRoundedRectangle(float x, float y, float width, float height, float radiusX, float radiusY, Color color)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Fill);
            _canvas.DrawRoundRect(new SKRect(x, y, x + width, y + height), radiusX, radiusY, paint);
        }

        // ── Geometry ──────────────────────────────────────────────────

        public void FillGeometry(CanvasGeometry geometry, Color color)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Fill);
            _canvas.DrawPath(geometry.GetPath(), paint);
        }

        public void FillGeometry(CanvasGeometry geometry, ICanvasBrush brush)
        {
            using var paint = CreatePaint(brush, SKPaintStyle.Fill);
            _canvas.DrawPath(geometry.GetPath(), paint);
        }

        public void DrawGeometry(CanvasGeometry geometry, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawPath(geometry.GetPath(), paint);
        }

        public void DrawCachedGeometry(CanvasCachedGeometry cachedGeometry, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            ArgumentNullException.ThrowIfNull(cachedGeometry);
            using SKPath path = cachedGeometry.GetPath();
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawPath(path, paint);
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

        public void DrawTextLayout(CanvasTextLayout textLayout, float x, float y, Color color)
        {
            using SKFont font = CreateTextFont(textLayout.Format);
            using var paint = CreateTextPaint(color);
            _canvas.DrawText(textLayout.Text, x, y + font.Metrics.Ascent, font, paint);
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

        public void DrawImage(CanvasEffect effect, Rect destinationRect)
        {
            ArgumentNullException.ThrowIfNull(effect);
            using SKImage image = effect.GetImage();
            using var paint = CreateImagePaint();
            _canvas.DrawImage(image, ToSkRect(destinationRect), paint);
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

        // ── Layer ──────────────────────────────────────────────────────

        public CanvasLayer CreateLayer(float opacity)
        {
            using var layerPaint = new SKPaint { Color = new SKColor(0, 0, 0, (byte)(opacity * 255)) };
            _canvas.SaveLayer(layerPaint);
            return new CanvasLayer(_canvas);
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

        internal CanvasLayer(SKCanvas canvas)
        {
            _canvas = canvas;
        }

        public void Dispose()
        {
            if (_canvas is not null)
            {
                _canvas.Restore();
                _canvas = null;
            }
        }
    }
}
