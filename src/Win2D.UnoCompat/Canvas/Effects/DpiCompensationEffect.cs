using SkiaSharp;
using Windows.Graphics.Effects;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class DpiCompensationEffect : CanvasEffect, IGraphicsEffect
    {
        public float SourceDpi { get; set; } = 96f;
        public CanvasImageInterpolation InterpolationMode { get; set; } = CanvasImageInterpolation.Linear;
        public EffectBorderMode BorderMode { get; set; } = EffectBorderMode.Hard;
        public string Name { get; set; } = string.Empty;

        internal override SKImage GetImage()
        {
            return RequireSourceImage(Source);
        }
    }
}
