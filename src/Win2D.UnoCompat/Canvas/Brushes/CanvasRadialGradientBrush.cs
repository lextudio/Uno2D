using SkiaSharp;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;

namespace Microsoft.Graphics.Canvas.Brushes
{
    public sealed class CanvasRadialGradientBrush : ICanvasBrush, ISkiaCanvasBrush, IDisposable
    {
        public CanvasRadialGradientBrush(CanvasDevice device, Color startColor, Color endColor)
            : this(device, new[]
            {
                new CanvasGradientStop { Position = 0f, Color = startColor },
                new CanvasGradientStop { Position = 1f, Color = endColor },
            })
        {
        }

        public CanvasRadialGradientBrush(CanvasDevice device, CanvasGradientStop[] stops)
        {
            ArgumentNullException.ThrowIfNull(device);
            GradientStops = stops ?? throw new ArgumentNullException(nameof(stops));
        }

        public Point Center { get; set; }

        public float Radius { get; set; } = 1f;

        public CanvasGradientStop[] GradientStops { get; set; }

        public CanvasEdgeBehavior ExtendX { get; set; } = CanvasEdgeBehavior.Clamp;

        public CanvasEdgeBehavior ExtendY { get; set; } = CanvasEdgeBehavior.Clamp;

        public Matrix3x2 Transform { get; set; } = Matrix3x2.Identity;

        public float Opacity { get; set; } = 1f;

        SKPaint ISkiaCanvasBrush.CreatePaint(SKPaintStyle style, bool isAntialias, SKBlendMode blendMode)
        {
            if (GradientStops.Length == 0)
                throw new InvalidOperationException("CanvasRadialGradientBrush requires at least one gradient stop.");
            if (Radius <= 0)
                throw new InvalidOperationException("CanvasRadialGradientBrush.Radius must be greater than zero.");

            SKPaint paint = CanvasBrushHelpers.CreatePaint(style, isAntialias, blendMode);
            paint.Shader = SKShader.CreateRadialGradient(
                new SKPoint((float)Center.X, (float)Center.Y),
                Radius,
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
