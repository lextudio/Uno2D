using SkiaSharp;
using System.Numerics;
using Windows.UI;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class ShadowEffect : CanvasEffect
    {
        public float BlurAmount { get; set; } = 3f;

        public Vector2 Offset { get; set; } = new(2, 2);

        public Color Color { get; set; } = Color.FromArgb(255, 0, 0, 0);

        public CanvasColor ShadowColorHdr { get; set; } = CanvasColor.FromArgb(1f, 0f, 0f, 0f);

        public EffectOptimization Optimization { get; set; } = EffectOptimization.Balanced;

        internal override SKImage GetImage()
        {
            using var paint = new SKPaint
            {
                ImageFilter = SKImageFilter.CreateDropShadowOnly(
                    Offset.X,
                    Offset.Y,
                    BlurAmount,
                    BlurAmount,
                    ToSkColor(Color)),
            };
            return ApplyPaintFilter(Source, paint);
        }
    }

    public struct CanvasColor
    {
        public float A, R, G, B;
        public static CanvasColor FromArgb(float a, float r, float g, float b) => new() { A = a, R = r, G = g, B = b };
    }
}
