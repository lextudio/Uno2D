using SkiaSharp;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class BlendEffect : CanvasEffect
    {
        public ICanvasImage? Background { get; set; }
        public ICanvasImage? Foreground { get; set; }
        public CanvasBlend Mode { get; set; } = CanvasBlend.SourceOver;

        internal override SKImage GetImage()
        {
            using SKImage background = RequireSourceImage(Background);
            using SKImage foreground = RequireSourceImage(Foreground);
            int width = Math.Max(background.Width, foreground.Width);
            int height = Math.Max(background.Height, foreground.Height);
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using SKSurface surface = SKSurface.Create(info)
                ?? throw new InvalidOperationException("Unable to create blend effect surface.");
            surface.Canvas.Clear(SKColors.Transparent);
            surface.Canvas.DrawImage(background, 0, 0);
            using var paint = new SKPaint { BlendMode = ToSkBlendMode(Mode) };
            surface.Canvas.DrawImage(foreground, 0, 0, paint);
            surface.Canvas.Flush();
            return surface.Snapshot();
        }

        private static SKBlendMode ToSkBlendMode(CanvasBlend blend) => blend switch
        {
            CanvasBlend.Copy => SKBlendMode.Src,
            CanvasBlend.Add => SKBlendMode.Plus,
            CanvasBlend.Modulate => SKBlendMode.Modulate,
            CanvasBlend.Multiply => SKBlendMode.Multiply,
            _ => SKBlendMode.SrcOver,
        };
    }
}
