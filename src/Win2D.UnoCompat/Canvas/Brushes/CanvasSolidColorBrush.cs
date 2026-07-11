using SkiaSharp;
using Windows.UI;

namespace Microsoft.Graphics.Canvas.Brushes
{
    public sealed class CanvasSolidColorBrush : ICanvasSolidColorBrush, ISkiaCanvasBrush, IDisposable
    {
        public CanvasSolidColorBrush(CanvasDevice device, Color color)
        {
            ArgumentNullException.ThrowIfNull(device);
            Color = color;
        }

        public Color Color { get; set; }

        public float Opacity { get; set; } = 1f;

        SKPaint ISkiaCanvasBrush.CreatePaint(SKPaintStyle style, bool isAntialias, SKBlendMode blendMode)
        {
            SKPaint paint = CanvasBrushHelpers.CreatePaint(style, isAntialias, blendMode);
            paint.Color = CanvasBrushHelpers.ToSkColor(Color, Opacity);
            return paint;
        }

        public void Dispose()
        {
        }
    }
}
