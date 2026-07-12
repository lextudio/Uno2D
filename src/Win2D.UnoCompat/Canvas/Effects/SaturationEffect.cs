using SkiaSharp;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class SaturationEffect : CanvasEffect
    {
        public float Saturation { get; set; } = 1f;

        internal override SKImage GetImage()
        {
            float s = Saturation;
            float ir = 0.213f * (1 - s);
            float ig = 0.715f * (1 - s);
            float ib = 0.072f * (1 - s);
            float[] matrix =
            [
                ir + s, ig, ib, 0, 0,
                ir, ig + s, ib, 0, 0,
                ir, ig, ib + s, 0, 0,
                0, 0, 0, 1, 0,
            ];
            using var paint = new SKPaint
            {
                ColorFilter = SKColorFilter.CreateColorMatrix(matrix),
            };
            return ApplyPaintFilter(Source, paint);
        }
    }
}
