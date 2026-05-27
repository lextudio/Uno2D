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
            float x = (float)bounds.X;
            if (format.HorizontalAlignment == CanvasHorizontalAlignment.Center)
                x += (float)bounds.Width / 2f;
            else if (format.HorizontalAlignment == CanvasHorizontalAlignment.Right)
                x += (float)bounds.Width;

            float y = (float)bounds.Y;
            var metrics = font.Metrics;
            float textHeight = metrics.Descent - metrics.Ascent;

            if (format.VerticalAlignment == CanvasVerticalAlignment.Center)
                y += (float)bounds.Height / 2f + (metrics.Descent - textHeight / 2f);
            else if (format.VerticalAlignment == CanvasVerticalAlignment.Bottom)
                y += (float)bounds.Height - metrics.Descent;
            else
                y -= metrics.Ascent;

            _canvas.DrawText(text, x, y, font, paint);
        }

        public void DrawText(string text, float x, float y, Color color, CanvasTextFormat format)
        {
            using SKFont font = CreateTextFont(format);
            using var paint = CreateTextPaint(color);
            y -= font.Metrics.Ascent;
            _canvas.DrawText(text, x, y, font, paint);
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
