using SkiaSharp;
using Windows.Graphics.Effects;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class ArithmeticCompositeEffect : CanvasEffect, IGraphicsEffect
    {
        public float MultiplyAmount { get; set; } = 1f;
        public float Source1Amount { get; set; } = 0f;
        public float Source2Amount { get; set; } = 0f;
        public float Offset { get; set; } = 0f;
        public bool ClampOutput { get; set; }
        public string Name { get; set; } = string.Empty;
        public ICanvasImage? Source1 { get; set; }
        public ICanvasImage? Source2 { get; set; }

        public new ICanvasImage? Source { get; set; }

        internal override SKImage GetImage()
        {
            using SKImage input = RequireSourceImage(Source1);
            return input;
        }
    }
}
