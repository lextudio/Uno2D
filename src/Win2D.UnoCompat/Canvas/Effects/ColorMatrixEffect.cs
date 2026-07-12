using SkiaSharp;
using Windows.Graphics.Effects;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class ColorMatrixEffect : CanvasEffect, IGraphicsEffect
    {
        public string Name { get; set; } = string.Empty;
        public float[] ColorMatrix { get; set; } =
        [
            1, 0, 0, 0, 0,
            0, 1, 0, 0, 0,
            0, 0, 1, 0, 0,
            0, 0, 0, 1, 0,
        ];

        private CanvasAlphaMode _alphaMode = CanvasAlphaMode.Premultiplied;
        public CanvasAlphaMode AlphaMode
        {
            get => _alphaMode;
            set
            {
                if (value == CanvasAlphaMode.Ignore)
                    throw new ArgumentException("AlphaMode.Ignore is not supported.");
                _alphaMode = value;
            }
        }

        public bool ClampOutput { get; set; }

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
