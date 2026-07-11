namespace Microsoft.Graphics.Canvas
{
    public enum CanvasAlphaMode
    {
        Premultiplied,
        Straight,
        Ignore,
    }

    public enum CanvasBufferPrecision
    {
        Precision8Bit,
        Precision16Bit,
        Precision16BitFloat,
        Precision32BitFloat,
    }

    public enum CanvasImageInterpolation
    {
        NearestNeighbor,
        Linear,
        Cubic,
        MultiSampleLinear,
        Anisotropic,
        HighQualityCubic,
    }

    public sealed class CanvasStrokeStyle
    {
        public Geometry.CanvasDashStyle DashStyle { get; set; }
        public Geometry.CanvasCapStyle StartCap { get; set; }
        public Geometry.CanvasCapStyle EndCap { get; set; }
        public Geometry.CanvasLineJoin LineJoin { get; set; }
        public Geometry.CanvasCapStyle DashCap { get; set; } = Geometry.CanvasCapStyle.Square;
        public float DashOffset { get; set; }
        public float[]? CustomDashStyle { get; set; }
        public Geometry.CanvasStrokeTransformBehavior TransformBehavior { get; set; } = Geometry.CanvasStrokeTransformBehavior.Default;
    }

    internal static class CanvasPrimitivesExtensions
    {
        public static SkiaSharp.SKStrokeCap GetSkStrokeCap(this CanvasStrokeStyle style, Geometry.CanvasCapStyle cap) => cap switch
        {
            Geometry.CanvasCapStyle.Round => SkiaSharp.SKStrokeCap.Round,
            Geometry.CanvasCapStyle.Square => SkiaSharp.SKStrokeCap.Square,
            Geometry.CanvasCapStyle.Triangle => SkiaSharp.SKStrokeCap.Square,
            _ => SkiaSharp.SKStrokeCap.Butt,
        };

        public static SkiaSharp.SKStrokeJoin GetSkStrokeJoin(this CanvasStrokeStyle style, Geometry.CanvasLineJoin join) => join switch
        {
            Geometry.CanvasLineJoin.Bevel => SkiaSharp.SKStrokeJoin.Bevel,
            Geometry.CanvasLineJoin.Round => SkiaSharp.SKStrokeJoin.Round,
            _ => SkiaSharp.SKStrokeJoin.Miter,
        };

        public static SkiaSharp.SKPathEffect? GetDashEffect(this CanvasStrokeStyle style)
        {
            if (style.CustomDashStyle is { Length: > 0 } custom)
                return SkiaSharp.SKPathEffect.CreateDash(custom, style.DashOffset);

            return style.DashStyle switch
            {
                Geometry.CanvasDashStyle.Dash => SkiaSharp.SKPathEffect.CreateDash(new float[] { 4, 4 }, style.DashOffset),
                Geometry.CanvasDashStyle.Dot => SkiaSharp.SKPathEffect.CreateDash(new float[] { 1, 3 }, style.DashOffset),
                Geometry.CanvasDashStyle.DashDot => SkiaSharp.SKPathEffect.CreateDash(new float[] { 4, 3, 1, 3 }, style.DashOffset),
                Geometry.CanvasDashStyle.DashDotDot => SkiaSharp.SKPathEffect.CreateDash(new float[] { 4, 3, 1, 3, 1, 3 }, style.DashOffset),
                _ => null,
            };
        }
    }

    public sealed class CanvasActiveLayer : IDisposable
    {
        private SkiaSharp.SKCanvas? _canvas;

        internal CanvasActiveLayer(SkiaSharp.SKCanvas canvas)
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

    public sealed class CanvasTextRenderingParameters
    {
        public CanvasTextRenderingMode RenderingMode { get; set; } = CanvasTextRenderingMode.Default;
        public CanvasTextGridFit GridFit { get; set; } = CanvasTextGridFit.Default;
    }

    public enum CanvasTextRenderingMode
    {
        Default,
        Aliased,
        GdiClassic,
        GdiNatural,
        Natural,
        NaturalSymetric,
    }

    public enum CanvasTextGridFit
    {
        Default,
        Disabled,
        Enabled,
    }
}
