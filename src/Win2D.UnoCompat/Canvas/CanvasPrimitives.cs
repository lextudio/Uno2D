namespace Microsoft.Graphics.Canvas
{
    public enum CanvasBitmapFileFormat
    {
        Png
    }

    public sealed class CanvasStrokeStyle
    {
        public Geometry.CanvasDashStyle DashStyle { get; set; }
        public Geometry.CanvasCapStyle StartCap { get; set; }
        public Geometry.CanvasCapStyle EndCap { get; set; }
        public Geometry.CanvasLineJoin LineJoin { get; set; }
    }

    internal static class CanvasPrimitivesExtensions
    {
        public static SkiaSharp.SKStrokeCap GetSkStrokeCap(this CanvasStrokeStyle style, Geometry.CanvasCapStyle cap) => cap switch
        {
            Geometry.CanvasCapStyle.Round => SkiaSharp.SKStrokeCap.Round,
            Geometry.CanvasCapStyle.Square => SkiaSharp.SKStrokeCap.Square,
            _ => SkiaSharp.SKStrokeCap.Butt,
        };

        public static SkiaSharp.SKStrokeJoin GetSkStrokeJoin(this CanvasStrokeStyle style, Geometry.CanvasLineJoin join) => join switch
        {
            Geometry.CanvasLineJoin.Bevel => SkiaSharp.SKStrokeJoin.Bevel,
            Geometry.CanvasLineJoin.Round => SkiaSharp.SKStrokeJoin.Round,
            _ => SkiaSharp.SKStrokeJoin.Miter,
        };
    }
}
