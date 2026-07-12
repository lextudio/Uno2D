using SkiaSharp;
using System.Collections.Generic;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Windows.Graphics.Effects;

namespace Microsoft.Graphics.Canvas.Effects
{
    public interface ICanvasImage : IGraphicsEffectSource
    {
    }

    public abstract class CanvasEffect : ICanvasImage, IDisposable
    {
        public IGraphicsEffectSource? Source { get; set; }

        public CanvasBufferPrecision BufferPrecision { get; set; } = CanvasBufferPrecision.Precision8Bit;

        public bool CacheOutput { get; set; }

        public string Name { get; set; } = string.Empty;

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

        internal static CanvasDevice? GetDevice(IGraphicsEffectSource? source)
        {
            return source switch
            {
                CanvasBitmap b => b.Device,
                CanvasRenderTarget r => r.Device,
                CanvasCommandList c => c.Device,
                CanvasEffect e => GetDevice(e.Source),
                _ => null,
            };
        }

        internal static SKImage ResolveImage(IGraphicsEffectSource source)
        {
            return source switch
            {
                CanvasBitmap bitmap => SKImage.FromBitmap(bitmap.Bitmap),
                CanvasRenderTarget rt => rt.GetImage(),
                CanvasCommandList cl => cl.GetImage(),
                CanvasEffect effect => effect.GetImage(),
                _ => throw new InvalidCastException("Effect source #0 is an unsupported type. To draw an effect using Win2D, all its sources must be Win2D ICanvasImage objects."),
            };
        }

        internal static SKImage RequireSourceImage(IGraphicsEffectSource? source)
        {
            ValidateSourceTree(source, 0);
            return ResolveImage(source!);
        }

        private static readonly HashSet<Type> SelfContainedEffects = new()
        {
            typeof(ColorSourceEffect),
        };

        private static void ValidateSourceTree(IGraphicsEffectSource? source, int depth)
        {
            if (source is null)
                throw new ArgumentException("Effect source #0 is null.");

            if (source is CanvasEffect effect)
            {
                if (!SelfContainedEffects.Contains(effect.GetType()))
                    ValidateSourceTree(effect.Source, depth + 1);
                return;
            }

            if (source is not CanvasBitmap and not CanvasRenderTarget and not CanvasCommandList)
                throw new InvalidCastException("Effect source #0 is an unsupported type. To draw an effect using Win2D, all its sources must be Win2D ICanvasImage objects.");
        }

        internal static SKImage ApplyPaintFilter(IGraphicsEffectSource? source, SKPaint paint)
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
