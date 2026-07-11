using SkiaSharp;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;

namespace Microsoft.Graphics.Canvas.Effects
{
    public interface ICanvasImage
    {
    }

    public abstract class CanvasEffect : ICanvasImage, IDisposable
    {
        public ICanvasImage? Source { get; set; }

        public CanvasBufferPrecision BufferPrecision { get; set; } = CanvasBufferPrecision.Precision8Bit;

        public bool CacheOutput { get; set; }

        public string? Name { get; set; }

        internal abstract SKImage GetImage();

        public virtual void Dispose()
        {
        }

        public Rect GetBounds(CanvasDrawingSession drawingSession)
        {
            using SKImage image = GetImage();
            return new Rect(0, 0, image.Width, image.Height);
        }

        public Rect[] GetInvalidRectangles()
        {
            return Array.Empty<Rect>();
        }

        public Rect GetRequiredSourceRectangle(Rect imageRectangle, Rect sourceRectangle)
        {
            return sourceRectangle;
        }

        public Rect[] GetRequiredSourceRectangles(Rect imageRectangle, Rect[] sourceRectangles)
        {
            return sourceRectangles;
        }

        public void InvalidateSourceRectangle(Rect invalidRectangle)
        {
        }

        internal static SKImage ResolveImage(ICanvasImage source)
        {
            return source switch
            {
                CanvasBitmap bitmap => SKImage.FromBitmap(bitmap.Bitmap),
                CanvasEffect effect => effect.GetImage(),
                _ => throw new ArgumentException("Unsupported canvas image source.", nameof(source)),
            };
        }

        internal static SKImage RequireSourceImage(ICanvasImage? source)
        {
            if (source is null)
                throw new InvalidOperationException("Effect source must be set before rendering.");

            return ResolveImage(source);
        }

        internal static SKImage ApplyPaintFilter(ICanvasImage? source, SKPaint paint)
        {
            using SKImage input = RequireSourceImage(source);
            var info = new SKImageInfo(input.Width, input.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using SKSurface surface = SKSurface.Create(info)
                ?? throw new InvalidOperationException("Unable to create effect surface.");
            surface.Canvas.Clear(SKColors.Transparent);
            surface.Canvas.DrawImage(input, 0, 0, paint);
            surface.Canvas.Flush();
            return surface.Snapshot();
        }

        internal static SKMatrix ToSkMatrix(Matrix3x2 matrix)
        {
            return new SKMatrix
            {
                ScaleX = matrix.M11,
                SkewX = matrix.M21,
                TransX = matrix.M31,
                SkewY = matrix.M12,
                ScaleY = matrix.M22,
                TransY = matrix.M32,
                Persp0 = 0,
                Persp1 = 0,
                Persp2 = 1,
            };
        }

        internal static SKColor ToSkColor(Color color)
            => new(color.R, color.G, color.B, color.A);
    }

    public enum EffectBorderMode
    {
        Soft,
        Hard,
    }

    public enum CanvasComposite
    {
        SourceOver,
        Add,
        Copy,
        DestinationOver,
    }
}
