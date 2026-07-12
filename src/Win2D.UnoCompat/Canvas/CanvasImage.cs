using SkiaSharp;
using Microsoft.Graphics.Canvas.Effects;
using Windows.Foundation;
using Windows.Graphics.Effects;
using Windows.Storage.Streams;

namespace Microsoft.Graphics.Canvas
{
    public static class CanvasImage
    {
        public static float[] ComputeHistogram(
            ICanvasImage image,
            Rect sourceRectangle,
            ICanvasResourceCreator resourceCreator,
            EffectChannelSelect channelSelect,
            int numberOfBins)
        {
            return new float[numberOfBins];
        }

        public static async Task SaveAsync(ICanvasImage image, IRandomAccessStream stream, CanvasBitmapFileFormat fileFormat)
        {
            await SaveAsync(image, new Rect(0, 0, 0, 0), 96, CanvasDevice.GetSharedDevice(), stream, fileFormat);
        }

        public static async Task SaveAsync(ICanvasImage image, Rect imageRectangle, float dpi, CanvasDevice device, IRandomAccessStream stream, CanvasBitmapFileFormat fileFormat)
        {
            await SaveAsync(image, imageRectangle, dpi, device, stream, fileFormat, 1.0f, CanvasBufferPrecision.Precision8UIntNormalized);
        }

        public static async Task SaveAsync(ICanvasImage image, Rect imageRectangle, float dpi, CanvasDevice device, IRandomAccessStream stream, CanvasBitmapFileFormat fileFormat, float quality, CanvasBufferPrecision bufferPrecision)
        {
            using var skImage = RenderImage(image, imageRectangle, dpi, device);
            using var data = skImage.Encode(ToSkFormat(fileFormat), (int)(quality * 100));
            if (data is null)
                throw new InvalidOperationException("Failed to encode image.");

            var dotnetStream = stream.AsStreamForWrite();
            dotnetStream.SetLength(0);
            await data.AsStream().CopyToAsync(dotnetStream).ConfigureAwait(false);
            await dotnetStream.FlushAsync().ConfigureAwait(false);
            stream.Seek(0);
        }

        private static SKImage RenderImage(ICanvasImage image, Rect imageRectangle, float dpi, CanvasDevice device)
        {
            int width, height;
            if (imageRectangle.IsEmpty || (imageRectangle.Width == 0 && imageRectangle.Height == 0))
            {
                if (image is CanvasBitmap bitmap)
                {
                    width = bitmap.Bitmap.Width;
                    height = bitmap.Bitmap.Height;
                }
                else if (image is CanvasRenderTarget rt)
                {
                    width = rt.Width;
                    height = rt.Height;
                }
                else if (image is CanvasCommandList cl)
                {
                    width = (int)cl.Size.Width;
                    height = (int)cl.Size.Height;
                }
                else
                {
                    width = 1;
                    height = 1;
                }
            }
            else
            {
                width = (int)imageRectangle.Width;
                height = (int)imageRectangle.Height;
            }

            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using var surface = SKSurface.Create(info);
            if (surface is null)
                throw new InvalidOperationException("Unable to create temporary surface.");

            var canvas = surface.Canvas;
            var skImage = CanvasEffect.ResolveImage(image);
            canvas.DrawImage(skImage, new SKRect(0, 0, width, height));

            return surface.Snapshot();
        }

        private static SKEncodedImageFormat ToSkFormat(CanvasBitmapFileFormat format) => format switch
        {
            CanvasBitmapFileFormat.Bmp => SKEncodedImageFormat.Bmp,
            CanvasBitmapFileFormat.Gif => SKEncodedImageFormat.Gif,
            CanvasBitmapFileFormat.Jpeg => SKEncodedImageFormat.Jpeg,
            CanvasBitmapFileFormat.JpegXR => SKEncodedImageFormat.Jpeg,
            CanvasBitmapFileFormat.Png => SKEncodedImageFormat.Png,
            CanvasBitmapFileFormat.Tiff => SKEncodedImageFormat.Wbmp,
            CanvasBitmapFileFormat.Dds => SKEncodedImageFormat.Png,
            CanvasBitmapFileFormat.Hdr => SKEncodedImageFormat.Png,
            _ => SKEncodedImageFormat.Png,
        };
    }
}
