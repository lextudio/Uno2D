using SkiaSharp;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class CompositeEffect : CanvasEffect
    {
        public IList<ICanvasImage> Sources { get; } = new List<ICanvasImage>();

        public CanvasComposite Mode { get; set; } = CanvasComposite.SourceOver;

        internal override SKImage GetImage()
        {
            if (Sources.Count == 0)
                throw new InvalidOperationException("CompositeEffect requires at least one source.");

            using SKImage first = ResolveImage(Sources[0]);
            int width = first.Width;
            int height = first.Height;
            var resolved = new List<SKImage> { first };
            for (int i = 1; i < Sources.Count; i++)
            {
                SKImage image = ResolveImage(Sources[i]);
                resolved.Add(image);
                width = Math.Max(width, image.Width);
                height = Math.Max(height, image.Height);
            }

            try
            {
                var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
                using SKSurface surface = SKSurface.Create(info)
                    ?? throw new InvalidOperationException("Unable to create composite effect surface.");
                surface.Canvas.Clear(SKColors.Transparent);
                using var paint = new SKPaint { BlendMode = ToSkBlendMode(Mode) };
                surface.Canvas.DrawImage(resolved[0], 0, 0);
                for (int i = 1; i < resolved.Count; i++)
                    surface.Canvas.DrawImage(resolved[i], 0, 0, paint);

                surface.Canvas.Flush();
                return surface.Snapshot();
            }
            finally
            {
                for (int i = 1; i < resolved.Count; i++)
                    resolved[i].Dispose();
            }
        }

        private static SKBlendMode ToSkBlendMode(CanvasComposite composite) => composite switch
        {
            CanvasComposite.Add => SKBlendMode.Plus,
            CanvasComposite.Copy => SKBlendMode.Src,
            CanvasComposite.DestinationOver => SKBlendMode.DstOver,
            _ => SKBlendMode.SrcOver,
        };
    }
}
