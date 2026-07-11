using SkiaSharp;
using Windows.Foundation;
using Windows.UI;

namespace Microsoft.Graphics.Canvas
{
    public sealed class CanvasBitmap : Microsoft.Graphics.Canvas.Effects.ICanvasImage, IDisposable
    {
        private readonly SKBitmap _bitmap;
        private bool _disposed;

        private CanvasBitmap(SKBitmap bitmap, float dpi)
        {
            _bitmap = bitmap;
            Dpi = dpi;
        }

        internal static CanvasBitmap CreateFromSkBitmap(SKBitmap bitmap, float dpi)
        {
            ArgumentNullException.ThrowIfNull(bitmap);
            return new CanvasBitmap(bitmap.Copy(), dpi);
        }

        public float Dpi { get; }

        public CanvasDevice Device => CanvasDevice.GetSharedDevice();

        public CanvasAlphaMode AlphaMode { get; set; } = CanvasAlphaMode.Premultiplied;

        public CanvasBitmapFileFormat Format { get; set; } = CanvasBitmapFileFormat.Auto;

        public string Description => $"CanvasBitmap ({_bitmap.Width}x{_bitmap.Height})";

        public Size Size => new(_bitmap.Width, _bitmap.Height);

        public Size SizeInPixels => new(_bitmap.Width, _bitmap.Height);

        public Rect Bounds => new(0, 0, _bitmap.Width, _bitmap.Height);

        internal SKBitmap Bitmap
        {
            get
            {
                ThrowIfDisposed();
                return _bitmap;
            }
        }

        public float ConvertDipsToPixels(float dips)
        {
            return dips * Dpi / 96f;
        }

        public float ConvertPixelsToDips(float pixels)
        {
            return pixels * 96f / Dpi;
        }

        public static Task<CanvasBitmap> LoadAsync(CanvasDevice device, byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(device);
            ArgumentNullException.ThrowIfNull(bytes);

            using var stream = new MemoryStream(bytes, writable: false);
            return LoadAsync(device, stream);
        }

        public static async Task<CanvasBitmap> LoadAsync(CanvasDevice device, string fileNameOrUri)
        {
            ArgumentNullException.ThrowIfNull(device);
            ArgumentException.ThrowIfNullOrWhiteSpace(fileNameOrUri);

            await using Stream stream = OpenReadStream(fileNameOrUri);
            return await LoadAsync(device, stream).ConfigureAwait(false);
        }

        public static Task<CanvasBitmap> LoadAsync(CanvasDevice device, Stream stream)
        {
            ArgumentNullException.ThrowIfNull(device);
            ArgumentNullException.ThrowIfNull(stream);

            SKBitmap bitmap = SKBitmap.Decode(stream)
                ?? throw new InvalidOperationException("Unable to decode bitmap stream.");
            return Task.FromResult(new CanvasBitmap(bitmap, 96f));
        }

        public static CanvasBitmap CreateFromBytes(CanvasDevice device, byte[] pixels, int width, int height, float dpi = 96f)
        {
            ArgumentNullException.ThrowIfNull(device);
            ArgumentNullException.ThrowIfNull(pixels);
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            int required = checked(width * height * 4);
            if (pixels.Length < required)
                throw new ArgumentException("The pixel buffer is too small.", nameof(pixels));

            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            var bitmap = new SKBitmap(info);
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, bitmap.GetPixels(), required);
            return new CanvasBitmap(bitmap, dpi);
        }

        public void SetPixelBytes(byte[] pixels)
        {
            ArgumentNullException.ThrowIfNull(pixels);
            ThrowIfDisposed();
            int required = checked(_bitmap.Width * _bitmap.Height * 4);
            if (pixels.Length < required)
                throw new ArgumentException("The pixel buffer is too small.", nameof(pixels));
            System.Runtime.InteropServices.Marshal.Copy(pixels, 0, _bitmap.GetPixels(), required);
            _bitmap.NotifyPixelsChanged();
        }

        public void SetPixelColors(Color[] colors)
        {
            ArgumentNullException.ThrowIfNull(colors);
            ThrowIfDisposed();
            if (colors.Length < _bitmap.Width * _bitmap.Height)
                throw new ArgumentException("The color array does not contain enough pixels.", nameof(colors));

            for (int y = 0; y < _bitmap.Height; y++)
            {
                for (int x = 0; x < _bitmap.Width; x++)
                {
                    Color color = colors[y * _bitmap.Width + x];
                    _bitmap.SetPixel(x, y, new SKColor(color.R, color.G, color.B, color.A));
                }
            }
        }

        public void CopyPixelsFromBitmap(CanvasBitmap source)
        {
            ArgumentNullException.ThrowIfNull(source);
            ThrowIfDisposed();
            source._bitmap.CopyTo(_bitmap);
        }

        public static CanvasBitmap CreateFromColors(CanvasDevice device, Color[] colors, int width, int height, float dpi = 96f)
        {
            ArgumentNullException.ThrowIfNull(device);
            ArgumentNullException.ThrowIfNull(colors);
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));
            if (colors.Length < width * height)
                throw new ArgumentException("The color array does not contain enough pixels.", nameof(colors));

            var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul));
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = colors[y * width + x];
                    bitmap.SetPixel(x, y, new SKColor(color.R, color.G, color.B, color.A));
                }
            }

            return new CanvasBitmap(bitmap, dpi);
        }

        public static CanvasBitmap CreateFromRenderTarget(CanvasRenderTarget renderTarget)
        {
            ArgumentNullException.ThrowIfNull(renderTarget);
            return new CanvasBitmap(renderTarget.SnapshotBitmap(), renderTarget.Dpi);
        }

        public Rect GetBounds(CanvasDrawingSession drawingSession)
        {
            ArgumentNullException.ThrowIfNull(drawingSession);
            return Bounds;
        }

        public Color[] GetPixelColors()
        {
            ThrowIfDisposed();

            var colors = new Color[_bitmap.Width * _bitmap.Height];
            for (int y = 0; y < _bitmap.Height; y++)
            {
                for (int x = 0; x < _bitmap.Width; x++)
                {
                    SKColor color = _bitmap.GetPixel(x, y);
                    colors[y * _bitmap.Width + x] = Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
                }
            }

            return colors;
        }

        public void CopyPixels(byte[] pixels)
        {
            ArgumentNullException.ThrowIfNull(pixels);
            ThrowIfDisposed();

            using SKPixmap pixmap = _bitmap.PeekPixels();
            int byteCount = pixmap.RowBytes * pixmap.Height;
            if (pixels.Length < byteCount)
                throw new ArgumentException("The destination buffer is too small.", nameof(pixels));

            System.Runtime.InteropServices.Marshal.Copy(pixmap.GetPixels(), pixels, 0, byteCount);
        }

        public void CopyPixels(byte[] pixels, int x, int y, int width, int height)
        {
            ArgumentNullException.ThrowIfNull(pixels);
            ThrowIfDisposed();
            if (x < 0 || x >= _bitmap.Width)
                throw new ArgumentOutOfRangeException(nameof(x));
            if (y < 0 || y >= _bitmap.Height)
                throw new ArgumentOutOfRangeException(nameof(y));
            if (width <= 0 || x + width > _bitmap.Width)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0 || y + height > _bitmap.Height)
                throw new ArgumentOutOfRangeException(nameof(height));

            int requiredBytes = checked(width * height * 4);
            if (pixels.Length < requiredBytes)
                throw new ArgumentException("The destination buffer is too small.", nameof(pixels));

            int offset = 0;
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    SKColor color = _bitmap.GetPixel(x + column, y + row);
                    pixels[offset++] = color.Blue;
                    pixels[offset++] = color.Green;
                    pixels[offset++] = color.Red;
                    pixels[offset++] = color.Alpha;
                }
            }
        }

        public async Task SaveAsync(Stream stream, CanvasBitmapFileFormat format)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ThrowIfDisposed();

            using SKImage image = SKImage.FromBitmap(_bitmap);
            using SKData data = image.Encode(ToSkFormat(format), 100)
                ?? throw new InvalidOperationException("Failed to encode bitmap.");
            await data.AsStream().CopyToAsync(stream).ConfigureAwait(false);
            await stream.FlushAsync().ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _bitmap.Dispose();
                _disposed = true;
            }
        }

        private static Stream OpenReadStream(string fileNameOrUri)
        {
            if (fileNameOrUri.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                return File.OpenRead(new Uri(fileNameOrUri).LocalPath);

            if (fileNameOrUri.StartsWith("ms-appx:///", StringComparison.OrdinalIgnoreCase))
            {
                string path = fileNameOrUri["ms-appx:///".Length..].Replace('/', Path.DirectorySeparatorChar);
                return File.OpenRead(path);
            }

            if (Uri.TryCreate(fileNameOrUri, UriKind.Absolute, out Uri? uri) && uri.IsFile)
                return File.OpenRead(uri.LocalPath);

            return File.OpenRead(fileNameOrUri);
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

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(CanvasBitmap));
        }
    }
}
