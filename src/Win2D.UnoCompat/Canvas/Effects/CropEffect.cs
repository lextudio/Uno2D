using SkiaSharp;
using Windows.Foundation;
using Windows.Graphics.Effects;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class CropEffect : CanvasEffect, IGraphicsEffect
    {
        public Rect SourceRectangle { get; set; }
        public string Name { get; set; } = string.Empty;

        internal override SKImage GetImage()
        {
            using SKImage input = RequireSourceImage(Source);
            var rect = new SKRectI((int)SourceRectangle.X, (int)SourceRectangle.Y, 
                (int)(SourceRectangle.X + SourceRectangle.Width), (int)(SourceRectangle.Y + SourceRectangle.Height));
            return input.Subset(rect) ?? input;
        }
    }
}
