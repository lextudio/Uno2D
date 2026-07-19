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
            var device = resourceCreator switch
            {
                CanvasDevice d => d,
                ICanvasResourceCreator c => c.Device,
                _ => CanvasDevice.GetSharedDevice(),
            };
            using var skImage = RenderImage(image, sourceRectangle, 96, device);
            using var skBitmap = new SKBitmap(skImage.Width, skImage.Height);
            skImage.ReadPixels(skBitmap.Info, skBitmap.GetPixels(), skBitmap.RowBytes, 0, 0);
            var histogram = new float[numberOfBins];
            int totalPixels = skBitmap.Width * skBitmap.Height;
            for (int y = 0; y < skBitmap.Height; y++)
            {
                for (int x = 0; x < skBitmap.Width; x++)
                {
                    SKColor color = skBitmap.GetPixel(x, y);
                    float value = channelSelect switch
                    {
                        EffectChannelSelect.Red => color.Red / 255f,
                        EffectChannelSelect.Green => color.Green / 255f,
                        EffectChannelSelect.Blue => color.Blue / 255f,
                        EffectChannelSelect.Alpha => color.Alpha / 255f,
                        _ => color.Red / 255f,
                    };
                    int bin = (int)(value * numberOfBins);
                    if (bin >= numberOfBins) bin = numberOfBins - 1;
                    histogram[bin] += 1f / totalPixels;
                }
            }
            return histogram;
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
            var skFormat = ToSkFormat(fileFormat);
            int skQuality = fileFormat == CanvasBitmapFileFormat.Jpeg || fileFormat == CanvasBitmapFileFormat.JpegXR
                ? (int)(quality * 100)
                : 100;
            using var data = skImage.Encode(skFormat, skQuality) ?? skImage.Encode(SKEncodedImageFormat.Png, 100);
            if (data is null)
                throw new InvalidOperationException("Failed to encode image.");

            byte[] bytes = data.ToArray();
            stream.Seek(0);
            var writerStream = stream.AsStreamForWrite();
            writerStream.SetLength(0);
            writerStream.Write(bytes, 0, bytes.Length);
            writerStream.Flush();
            stream.Seek(0);
            System.GC.KeepAlive(writerStream);
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

            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            using var surface = SKSurface.Create(info);
            if (surface is null)
                throw new InvalidOperationException("Unable to create temporary surface.");

            var canvas = surface.Canvas;
            var skImage = CanvasEffect.ResolveImage(image);
            if (imageRectangle.IsEmpty || (imageRectangle.Width == 0 && imageRectangle.Height == 0))
            {
                canvas.DrawImage(skImage, new SKRect(0, 0, width, height));
            }
            else
            {
                var srcRect = new SKRect((float)imageRectangle.X, (float)imageRectangle.Y,
                    (float)(imageRectangle.X + imageRectangle.Width), (float)(imageRectangle.Y + imageRectangle.Height));
                canvas.DrawImage(skImage, srcRect, new SKRect(0, 0, width, height));
            }

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
