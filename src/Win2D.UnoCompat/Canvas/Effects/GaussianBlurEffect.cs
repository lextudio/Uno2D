using SkiaSharp;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class GaussianBlurEffect : CanvasEffect
    {
        public float BlurAmount { get; set; } = 3f;

        public EffectBorderMode BorderMode { get; set; } = EffectBorderMode.Soft;

        public EffectOptimization Optimization { get; set; } = EffectOptimization.Balanced;

        public new ICanvasImage? Source { get; set; }

        internal override SKImage GetImage()
        {
            using var paint = new SKPaint
            {
                ImageFilter = SKImageFilter.CreateBlur(BlurAmount, BlurAmount),
            };
            return ApplyPaintFilter(Source, paint);
        }
    }
}
