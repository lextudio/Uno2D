using SkiaSharp;
using Windows.Graphics.Effects;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class ColorManagementEffect : CanvasEffect, IGraphicsEffect
    {
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
        public ColorManagementProfile? SourceProfile { get; set; }
        public ColorManagementProfile? DestinationProfile { get; set; }
        public CanvasImageInterpolation Quality { get; set; } = CanvasImageInterpolation.Linear;
        public string Name { get; set; } = string.Empty;

        internal override SKImage GetImage()
        {
            return RequireSourceImage(Source);
        }
    }
}
