using SkiaSharp;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;

namespace Microsoft.Graphics.Canvas.Brushes
{
    public sealed class CanvasLinearGradientBrush : ICanvasBrush, ISkiaCanvasBrush, IDisposable
    {
        public CanvasLinearGradientBrush(CanvasDevice device, Color startColor, Color endColor)
            : this(device, new[]
            {
                new CanvasGradientStop { Position = 0f, Color = startColor },
                new CanvasGradientStop { Position = 1f, Color = endColor },
            })
        {
        }

        public CanvasLinearGradientBrush(CanvasDevice device, CanvasGradientStop[] stops)
        {
            ArgumentNullException.ThrowIfNull(device);
            GradientStops = stops ?? throw new ArgumentNullException(nameof(stops));
        }

        public Point StartPoint { get; set; }

        public Point EndPoint { get; set; } = new(1, 0);

        public CanvasGradientStop[] GradientStops { get; set; }

        public CanvasEdgeBehavior ExtendX { get; set; } = CanvasEdgeBehavior.Clamp;

        public CanvasEdgeBehavior ExtendY { get; set; } = CanvasEdgeBehavior.Clamp;

        public Matrix3x2 Transform { get; set; } = Matrix3x2.Identity;

        public float Opacity { get; set; } = 1f;

        SKPaint ISkiaCanvasBrush.CreatePaint(SKPaintStyle style, bool isAntialias, SKBlendMode blendMode)
        {
            if (GradientStops.Length == 0)
                throw new InvalidOperationException("CanvasLinearGradientBrush requires at least one gradient stop.");

            SKPaint paint = CanvasBrushHelpers.CreatePaint(style, isAntialias, blendMode);
            paint.Shader = SKShader.CreateLinearGradient(
                new SKPoint((float)StartPoint.X, (float)StartPoint.Y),
                new SKPoint((float)EndPoint.X, (float)EndPoint.Y),
                GradientStops.Select(stop => CanvasBrushHelpers.ToSkColor(stop.Color, Opacity)).ToArray(),
                GradientStops.Select(stop => stop.Position).ToArray(),
                CanvasBrushHelpers.ToSkTileMode(ExtendX),
                CanvasBrushHelpers.ToSkMatrix(Transform));
            return paint;
        }

        public void Dispose()
        {
        }
    }
}
