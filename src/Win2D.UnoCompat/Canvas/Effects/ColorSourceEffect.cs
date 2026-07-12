using SkiaSharp;
using Windows.Graphics.Effects;
using Windows.UI;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class ColorSourceEffect : CanvasEffect, IGraphicsEffect
    {
        public Color Color { get; set; }
        public string Name { get; set; } = string.Empty;

        internal override SKImage GetImage()
        {
            var info = new SKImageInfo(1, 1, SKColorType.Bgra8888, SKAlphaType.Premul);
            using SKSurface surface = SKSurface.Create(info)
                ?? throw new InvalidOperationException("Unable to create effect surface.");
            surface.Canvas.Clear(new SKColor(Color.R, Color.G, Color.B, Color.A));
            return surface.Snapshot();
        }
    }
}
