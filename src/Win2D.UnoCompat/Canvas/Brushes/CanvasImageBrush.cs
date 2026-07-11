using SkiaSharp;
using System.Numerics;
using Windows.Foundation;

namespace Microsoft.Graphics.Canvas.Brushes
{
    public sealed class CanvasImageBrush : ICanvasBrush, ISkiaCanvasBrush, IDisposable
    {
        public CanvasImageBrush(CanvasDevice device)
        {
            ArgumentNullException.ThrowIfNull(device);
        }

        public CanvasBitmap? Image { get; set; }

        public CanvasEdgeBehavior ExtendX { get; set; } = CanvasEdgeBehavior.Clamp;

        public CanvasEdgeBehavior ExtendY { get; set; } = CanvasEdgeBehavior.Clamp;

        public Rect? SourceRect { get; set; }

        public Matrix3x2 Transform { get; set; } = Matrix3x2.Identity;

        public float Opacity { get; set; } = 1f;

        SKPaint ISkiaCanvasBrush.CreatePaint(SKPaintStyle style, bool isAntialias, SKBlendMode blendMode)
        {
            if (Image is null)
                throw new InvalidOperationException("CanvasImageBrush.Image must be set before drawing.");

            SKPaint paint = CanvasBrushHelpers.CreatePaint(style, isAntialias, blendMode);
            SKMatrix localMatrix = CanvasBrushHelpers.ToSkMatrix(Transform);
            paint.Color = new SKColor(255, 255, 255, (byte)(255 * Math.Clamp(Opacity, 0f, 1f)));

            SKBitmap bitmap = Image.Bitmap;
            if (SourceRect is { } source)
            {
                SKRectI subsetRect = new(
                    (int)source.X,
                    (int)source.Y,
                    (int)(source.X + source.Width),
                    (int)(source.Y + source.Height));
                var subset = new SKBitmap();
                if (bitmap.ExtractSubset(subset, subsetRect))
                    bitmap = subset;
            }

            paint.Shader = SKShader.CreateBitmap(
                bitmap,
                CanvasBrushHelpers.ToSkTileMode(ExtendX),
                CanvasBrushHelpers.ToSkTileMode(ExtendY),
                localMatrix);
            return paint;
        }

        public void Dispose()
        {
        }
    }
}
