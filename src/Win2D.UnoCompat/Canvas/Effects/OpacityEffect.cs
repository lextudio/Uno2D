using SkiaSharp;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class OpacityEffect : CanvasEffect
    {
        public float Opacity { get; set; } = 1f;

        public new ICanvasImage? Source { get; set; }

        public static bool IsSupported => true;

        internal override SKImage GetImage()
        {
            using var paint = new SKPaint
            {
                Color = new SKColor(255, 255, 255, (byte)(255 * Math.Clamp(Opacity, 0f, 1f))),
            };
            return ApplyPaintFilter(Source, paint);
        }
    }
}
