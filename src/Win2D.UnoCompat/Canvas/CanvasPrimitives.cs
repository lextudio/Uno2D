namespace Microsoft.Graphics.Canvas
{
    public enum CanvasBitmapFileFormat
    {
        Png
    }

    public enum CanvasDashStyle
    {
        Solid,
        Dash
    }

    public enum CanvasCapStyle
    {
        Flat,
        Round,
        Square
    }

    public enum CanvasLineJoin
    {
        Miter,
        Bevel,
        Round
    }

    public sealed class CanvasStrokeStyle
    {
        public CanvasDashStyle DashStyle { get; set; }
        public CanvasCapStyle StartCap { get; set; }
        public CanvasCapStyle EndCap { get; set; }
        public CanvasLineJoin LineJoin { get; set; }
    }

    internal static class CanvasPrimitivesExtensions
    {
        public static SkiaSharp.SKStrokeCap GetSkStrokeCap(this CanvasStrokeStyle style, CanvasCapStyle cap) => cap switch
        {
            CanvasCapStyle.Round => SkiaSharp.SKStrokeCap.Round,
            CanvasCapStyle.Square => SkiaSharp.SKStrokeCap.Square,
            _ => SkiaSharp.SKStrokeCap.Butt,
        };

        public static SkiaSharp.SKStrokeJoin GetSkStrokeJoin(this CanvasStrokeStyle style, CanvasLineJoin join) => join switch
        {
            CanvasLineJoin.Bevel => SkiaSharp.SKStrokeJoin.Bevel,
            CanvasLineJoin.Round => SkiaSharp.SKStrokeJoin.Round,
            _ => SkiaSharp.SKStrokeJoin.Miter,
        };
    }
}
