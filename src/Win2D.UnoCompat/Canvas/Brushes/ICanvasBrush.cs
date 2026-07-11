using SkiaSharp;

namespace Microsoft.Graphics.Canvas.Brushes
{
    public interface ICanvasBrush
    {
    }

    public interface ICanvasSolidColorBrush : ICanvasBrush
    {
        global::Windows.UI.Color Color { get; set; }
        float Opacity { get; set; }
    }

    internal interface ISkiaCanvasBrush : ICanvasBrush
    {
        SKPaint CreatePaint(SKPaintStyle style, bool isAntialias, SKBlendMode blendMode);
    }
}
