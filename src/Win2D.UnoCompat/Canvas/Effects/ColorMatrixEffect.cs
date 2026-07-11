using SkiaSharp;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class ColorMatrixEffect : CanvasEffect
    {
        public float[] ColorMatrix { get; set; } =
        [
            1, 0, 0, 0, 0,
            0, 1, 0, 0, 0,
            0, 0, 1, 0, 0,
            0, 0, 0, 1, 0,
        ];

        public CanvasAlphaMode AlphaMode { get; set; } = CanvasAlphaMode.Premultiplied;

        public bool ClampOutput { get; set; }

        public new ICanvasImage? Source { get; set; }

        internal override SKImage GetImage()
        {
            if (ColorMatrix.Length != 20)
                throw new InvalidOperationException("ColorMatrixEffect.ColorMatrix must contain 20 values.");

            using var paint = new SKPaint
            {
                ColorFilter = SKColorFilter.CreateColorMatrix(ColorMatrix),
            };
            return ApplyPaintFilter(Source, paint);
        }
    }
}
