using SkiaSharp;
using System;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;

namespace Microsoft.Graphics.Canvas
{
    public sealed class CanvasDrawingSession : IDisposable
    {
        private readonly SKCanvas _canvas;

        public CanvasDrawingSession(SKCanvas canvas)
        {
            _canvas = canvas;
        }

        public void Clear(Color color)
        {
            _canvas.Clear(ToSkColor(color));
        }

        public void FillCircle(float x, float y, float radius, Color color)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Fill);
            _canvas.DrawCircle(x, y, radius, paint);
        }

        public void DrawCircle(float x, float y, float radius, Color color, float strokeWidth)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth);
            _canvas.DrawCircle(x, y, radius, paint);
        }

        public void DrawLine(float x1, float y1, float x2, float y2, Color color, float strokeWidth, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = CreatePaint(color, SKPaintStyle.Stroke, strokeWidth, strokeStyle);
            _canvas.DrawLine(x1, y1, x2, y2, paint);
        }

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

        public void DrawText(string text, Rect bounds, Color color, CanvasTextFormat format)
        {
            using SKFont font = CreateTextFont(format);
            using var paint = CreateTextPaint(color);
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
            // Horizontal placement from the shared helper; vertical placement from FONT metrics, not
            // the per-string ink top. Aligning vertically by textBounds.Top would put each string's
            // own ink top at the requested y, so runs with different glyph heights (e.g. all-lowercase
            // "namespace" vs a run containing capitals) would sit at different vertical positions on
            // the same line. Using the font ascent/descent gives every run a common baseline —
            // matching real Win2D/DirectWrite.
            (float drawX, _) = ComputePointAlignedTextOrigin(
                textBounds,
                x,
                y,
                format.HorizontalAlignment,
                format.VerticalAlignment);
            SKFontMetrics metrics = font.Metrics;
            float ascent = -metrics.Ascent;   // distance above baseline (positive)
            float descent = metrics.Descent;  // distance below baseline (positive)
            float baseline = format.VerticalAlignment switch
            {
                CanvasVerticalAlignment.Center => y + (ascent - descent) / 2f,
                CanvasVerticalAlignment.Bottom => y - descent,
                _ => y + ascent, // Top: top of the font box at y
            };
            _canvas.DrawText(text, drawX, baseline, font, paint);
        }

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

        private static SKPaint CreateTextPaint(Color color)
        {
            return new SKPaint
            {
                Color = ToSkColor(color),
                IsAntialias = true
            };
        }

        private static SKPaint CreatePaint(Color color, SKPaintStyle style, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            var paint = new SKPaint
            {
                Color = ToSkColor(color),
                Style = style,
                StrokeWidth = strokeWidth,
                IsAntialias = true
            };

            if (strokeStyle is { })
            {
                paint.StrokeCap = strokeStyle.GetSkStrokeCap(strokeStyle.StartCap);
                paint.StrokeJoin = strokeStyle.GetSkStrokeJoin(strokeStyle.LineJoin);
                if (strokeStyle.DashStyle == CanvasDashStyle.Dash)
                    paint.PathEffect = SKPathEffect.CreateDash(new float[] { 4, 4 }, 0);
            }

            return paint;
        }

        private static SKColor ToSkColor(Color color)
            => new(color.R, color.G, color.B, color.A);

        public void Dispose()
        {
        }
    }
}
