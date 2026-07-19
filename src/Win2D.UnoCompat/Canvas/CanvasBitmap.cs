using SkiaSharp;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.UI;
using Windows.Graphics.DirectX;
using Windows.Storage.Streams;

// COM interface for IBuffer byte access
[System.Runtime.InteropServices.Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
[System.Runtime.InteropServices.InterfaceType(System.Runtime.InteropServices.ComInterfaceType.InterfaceIsIUnknown)]
unsafe interface IMemoryBufferByteAccess
{
    void GetBuffer(out byte* buffer, out uint capacity);
}

namespace Microsoft.Graphics.Canvas
{
    public sealed class CanvasBitmap : Microsoft.Graphics.Canvas.Effects.ICanvasImage, IDisposable
    {
        private readonly SKBitmap _bitmap;
        private readonly CanvasDevice _device;
        private byte[]? _pixelData;
        private bool _disposed;

        private CanvasBitmap(SKBitmap bitmap, float dpi, CanvasDevice? device = null, byte[]? pixelData = null)
        {
            _bitmap = bitmap;
            _device = device ?? CanvasDevice.GetSharedDevice();
            Dpi = dpi;
            _pixelData = pixelData;
        }

        internal static CanvasBitmap CreateFromSkBitmap(SKBitmap bitmap, float dpi, CanvasDevice? device = null)
        {
            ArgumentNullException.ThrowIfNull(bitmap);
            return new CanvasBitmap(bitmap.Copy(), dpi, device);
        }

        public float Dpi { get; }

        public CanvasDevice Device => _device;

        public CanvasAlphaMode AlphaMode { get; set; } = CanvasAlphaMode.Premultiplied;

        public DirectXPixelFormat Format { get; set; } = DirectXPixelFormat.B8G8R8A8UIntNormalized;

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
            return Task.FromResult(new CanvasBitmap(bitmap, 96f, device));
        }

        public static async Task<CanvasBitmap> LoadAsync(CanvasDevice device, IRandomAccessStream stream)
        {
            ArgumentNullException.ThrowIfNull(device);
            ArgumentNullException.ThrowIfNull(stream);

            stream.Seek(0);
            using var dotnetStream = stream.AsStreamForRead();
            return await LoadAsync(device, dotnetStream).ConfigureAwait(false);
        }

        private static int GetBytesPerPixel(DirectXPixelFormat format) => format switch
        {
            DirectXPixelFormat.A8UIntNormalized => 1,
            DirectXPixelFormat.R8UIntNormalized => 1,
            DirectXPixelFormat.R8G8UIntNormalized => 2,
            DirectXPixelFormat.R8G8B8A8UIntNormalized => 4,
            DirectXPixelFormat.B8G8R8A8UIntNormalized => 4,
            DirectXPixelFormat.B8G8R8X8UIntNormalized => 4,
            DirectXPixelFormat.R8G8B8A8UIntNormalizedSrgb => 4,
            DirectXPixelFormat.B8G8R8A8UIntNormalizedSrgb => 4,
            DirectXPixelFormat.R10G10B10A2UIntNormalized => 4,
            DirectXPixelFormat.R16G16B16A16UIntNormalized => 8,
            DirectXPixelFormat.R16G16B16A16Float => 8,
            DirectXPixelFormat.R32G32B32A32Float => 16,
            DirectXPixelFormat.BC1UIntNormalized => 0,
            DirectXPixelFormat.BC2UIntNormalized => 0,
            DirectXPixelFormat.BC3UIntNormalized => 0,
            _ => 4,
        };

        public static CanvasBitmap CreateFromBytes(CanvasDevice device, byte[] pixels, int width, int height, DirectXPixelFormat format)
        {
            return CreateFromBytes(device, pixels, width, height, format, 96f, ResolveDefaultAlphaMode(format, CanvasAlphaMode.Premultiplied));
        }

        public static CanvasBitmap CreateFromBytes(CanvasDevice device, byte[] pixels, int width, int height, DirectXPixelFormat format, float dpi = 96f, CanvasAlphaMode alphaMode = CanvasAlphaMode.Premultiplied)
        {
            ArgumentNullException.ThrowIfNull(device);
            ArgumentNullException.ThrowIfNull(pixels);
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            if (!device.IsPixelFormatSupported(format))
                throw new ArgumentException("The bitmap pixel format is unsupported. 0x88982F80");

            ValidateAlphaMode(format, alphaMode);

            int bpp = GetBytesPerPixel(format);
            int required = checked(width * height * Math.Max(bpp, 1));
            if (pixels.Length < required)
                throw new ArgumentException("The pixel buffer is too small.", nameof(pixels));

            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            var bitmap = new SKBitmap(info);
            var bgra = ConvertToBGRA(format, pixels, width, height, bpp);
            System.Runtime.InteropServices.Marshal.Copy(bgra, 0, bitmap.GetPixels(), bgra.Length);
            return new CanvasBitmap(bitmap, dpi, device, pixels[..required]) { Format = format, AlphaMode = alphaMode };
        }

        private static byte[] ConvertToBGRA(DirectXPixelFormat format, byte[] pixels, int width, int height, int bpp)
        {
            if (bpp <= 0) return new byte[width * height * 4];
            if (format == DirectXPixelFormat.B8G8R8A8UIntNormalized || format == DirectXPixelFormat.B8G8R8X8UIntNormalized || format == DirectXPixelFormat.B8G8R8A8UIntNormalizedSrgb)
                return pixels;

            int pixelCount = width * height;
            byte[] bgra = new byte[pixelCount * 4];
            for (int i = 0; i < pixelCount; i++)
            {
                int srcOff = i * Math.Max(bpp, 1);
                int dstOff = i * 4;
                switch (format)
                {
                    case DirectXPixelFormat.A8UIntNormalized:
                        bgra[dstOff + 3] = pixels[srcOff];
                        break;
                    case DirectXPixelFormat.R8UIntNormalized:
                        bgra[dstOff + 2] = pixels[srcOff];
                        bgra[dstOff + 3] = 255;
                        break;
                    case DirectXPixelFormat.R8G8UIntNormalized:
                        bgra[dstOff + 2] = pixels[srcOff];
                        bgra[dstOff + 1] = pixels[srcOff + 1];
                        bgra[dstOff + 3] = 255;
                        break;
                    case DirectXPixelFormat.R8G8B8A8UIntNormalized:
                    case DirectXPixelFormat.R8G8B8A8UIntNormalizedSrgb:
                        bgra[dstOff + 2] = pixels[srcOff];     // R
                        bgra[dstOff + 1] = pixels[srcOff + 1]; // G
                        bgra[dstOff] = pixels[srcOff + 2];     // B
                        bgra[dstOff + 3] = pixels[srcOff + 3]; // A
                        break;
                    default:
                        bgra[dstOff] = pixels[srcOff];         // B
                        bgra[dstOff + 1] = pixels[srcOff + 1]; // G
                        bgra[dstOff + 2] = pixels[srcOff + 2]; // R
                        bgra[dstOff + 3] = pixels.Length > srcOff + 3 ? pixels[srcOff + 3] : (byte)255;
                        break;
                }
            }
            return bgra;
        }

        private byte[] ConvertFromBGRA(byte[] bgra)
        {
            int bpp = GetBytesPerPixel(Format);
            if (bpp == 0) return bgra;
            int pixelCount = _bitmap.Width * _bitmap.Height;
            byte[] result = new byte[pixelCount * bpp];
            for (int i = 0; i < pixelCount; i++)
            {
                int srcOff = i * 4;
                int dstOff = i * bpp;
                switch (Format)
                {
                    case DirectXPixelFormat.A8UIntNormalized:
                        result[dstOff] = bgra[srcOff + 3];
                        break;
                    case DirectXPixelFormat.R8UIntNormalized:
                        result[dstOff] = bgra[srcOff + 2];
                        break;
                    case DirectXPixelFormat.R8G8UIntNormalized:
                        result[dstOff] = bgra[srcOff + 2];
                        result[dstOff + 1] = bgra[srcOff + 1];
                        break;
                    case DirectXPixelFormat.R8G8B8A8UIntNormalized:
                    case DirectXPixelFormat.R8G8B8A8UIntNormalizedSrgb:
                        result[dstOff] = bgra[srcOff + 2];     // R
                        result[dstOff + 1] = bgra[srcOff + 1]; // G
                        result[dstOff + 2] = bgra[srcOff];     // B
                        result[dstOff + 3] = bgra[srcOff + 3]; // A
                        break;
                    default:
                        result[dstOff] = bgra[srcOff];         // B
                        result[dstOff + 1] = bgra[srcOff + 1]; // G
                        result[dstOff + 2] = bgra[srcOff + 2]; // R
                        result[dstOff + 3] = bgra[srcOff + 3]; // A
                        break;
                }
            }
            return result;
        }

        internal static CanvasAlphaMode ResolveDefaultAlphaMode(DirectXPixelFormat format, CanvasAlphaMode alphaMode)
        {
            if (alphaMode != CanvasAlphaMode.Premultiplied)
                return alphaMode;

            foreach (var candidate in new[] { CanvasAlphaMode.Premultiplied, CanvasAlphaMode.Ignore, CanvasAlphaMode.Straight })
            {
                if (IsAlphaModeSupported(format, candidate))
                    return candidate;
            }

            return alphaMode;
        }

        internal static bool IsAlphaModeSupported(DirectXPixelFormat format, CanvasAlphaMode alphaMode)
        {
            bool premul = alphaMode == CanvasAlphaMode.Premultiplied;
            bool straight = alphaMode == CanvasAlphaMode.Straight;
            bool ignore = alphaMode == CanvasAlphaMode.Ignore;

            return format switch
            {
                DirectXPixelFormat.R8G8B8A8UIntNormalized => premul || ignore,
                DirectXPixelFormat.B8G8R8A8UIntNormalized => premul || ignore,
                DirectXPixelFormat.B8G8R8X8UIntNormalized => ignore,
                DirectXPixelFormat.R8G8B8A8UIntNormalizedSrgb => premul || ignore,
                DirectXPixelFormat.B8G8R8A8UIntNormalizedSrgb => premul || ignore,
                DirectXPixelFormat.R10G10B10A2UIntNormalized => premul || ignore,
                DirectXPixelFormat.R16G16B16A16UIntNormalized => premul || ignore,
                DirectXPixelFormat.R16G16B16A16Float => premul || ignore,
                DirectXPixelFormat.R32G32B32A32Float => premul || ignore,
                DirectXPixelFormat.A8UIntNormalized => premul || straight,
                DirectXPixelFormat.R8UIntNormalized => ignore,
                DirectXPixelFormat.R8G8UIntNormalized => ignore,
                DirectXPixelFormat.BC1UIntNormalized => premul || ignore,
                DirectXPixelFormat.BC2UIntNormalized => premul || ignore,
                DirectXPixelFormat.BC3UIntNormalized => premul || ignore,
                _ => true,
            };
        }

        internal static void ValidateAlphaMode(DirectXPixelFormat format, CanvasAlphaMode alphaMode)
        {
            if (!IsAlphaModeSupported(format, alphaMode))
                throw new ArgumentException("The bitmap pixel format is unsupported. 0x88982F80");
        }

        public static CanvasBitmap CreateFromBytes(CanvasDevice device, IBuffer buffer, int width, int height, DirectXPixelFormat format, float dpi = 96f, CanvasAlphaMode alphaMode = CanvasAlphaMode.Premultiplied)
        {
            if (device is null)
                throw new ArgumentException("Device cannot be null.", nameof(device));
            if (buffer is null)
                throw new ArgumentException("Buffer cannot be null.", nameof(buffer));
            if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            byte[] pixels = CopyBufferToBytes(buffer);
            return CreateFromBytes(device, pixels, width, height, format, dpi, alphaMode);
        }

        private static byte[] CopyBufferToBytes(IBuffer buffer)
        {
            byte[] result = new byte[buffer.Length];
            if (buffer.Length > 0)
            {
                System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.CopyTo(buffer, result);
            }
            return result;
        }

        private static void WriteBufferFromBytes(IBuffer buffer, byte[] data)
        {
            if (data.Length > 0)
            {
                buffer.Length = (uint)data.Length;
                System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.CopyTo(data, 0, buffer, 0, data.Length);
            }
        }

        public byte[] GetPixelBytes()
        {
            ThrowIfDisposed();
            if (_pixelData is not null)
                return _pixelData;
            using SKPixmap pixmap = _bitmap.PeekPixels();
            int byteCount = pixmap.RowBytes * pixmap.Height;
            byte[] bgra = new byte[byteCount];
            System.Runtime.InteropServices.Marshal.Copy(pixmap.GetPixels(), bgra, 0, byteCount);
            return ConvertFromBGRA(bgra);
        }

        private void RebuildPixelData()
        {
            int bpp = GetBytesPerPixel(Format);
            if (bpp == 0) { _pixelData = null; return; }
            using SKPixmap pixmap = _bitmap.PeekPixels();
            int byteCount = pixmap.RowBytes * pixmap.Height;
            byte[] bgra = new byte[byteCount];
            System.Runtime.InteropServices.Marshal.Copy(pixmap.GetPixels(), bgra, 0, byteCount);
            _pixelData = ConvertFromBGRA(bgra);
        }

        public void SetPixelBytes(byte[] pixels)
        {
            ArgumentNullException.ThrowIfNull(pixels);
            ThrowIfDisposed();
            int bpp = GetBytesPerPixel(Format);
            int required = checked(_bitmap.Width * _bitmap.Height * Math.Max(bpp, 1));
            if (pixels.Length < required)
                throw new ArgumentException("The pixel buffer is too small.", nameof(pixels));
            var bgra = ConvertToBGRA(Format, pixels, _bitmap.Width, _bitmap.Height, bpp);
            System.Runtime.InteropServices.Marshal.Copy(bgra, 0, _bitmap.GetPixels(), bgra.Length);
            _bitmap.NotifyPixelsChanged();
            _pixelData = pixels[..required];
        }

        public void SetPixelBytes(IBuffer buffer)
        {
            if (buffer is null)
                throw new ArgumentException("Buffer cannot be null.", nameof(buffer));
            ThrowIfDisposed();
            int bpp = GetBytesPerPixel(Format);
            int required = checked(_bitmap.Width * _bitmap.Height * Math.Max(bpp, 1));
            if (buffer.Capacity < (uint)required)
                throw new ArgumentException($"The array was expected to be of size {required}; actual array was of size {buffer.Capacity}.");
            byte[] data = CopyBufferToBytes(buffer);
            SetPixelBytes(data);
        }

        public void SetPixelBytes(IBuffer buffer, int x, int y, int width, int height)
        {
            if (buffer is null)
                throw new ArgumentException("Buffer cannot be null.", nameof(buffer));
            ThrowIfDisposed();
            if (x < 0 || x + width > _bitmap.Width)
                throw new ArgumentException("The bitmap pixel format is unsupported. 0x88982F80");
            if (y < 0 || y + height > _bitmap.Height)
                throw new ArgumentException("The bitmap pixel format is unsupported. 0x88982F80");
            if (width <= 0)
                throw new ArgumentException("The bitmap pixel format is unsupported. 0x88982F80");
            if (height <= 0)
                throw new ArgumentException("The bitmap pixel format is unsupported. 0x88982F80");
            byte[] data = CopyBufferToBytes(buffer);
            int bpp = GetBytesPerPixel(Format);
            int requiredBytes = checked(width * height * bpp);
            if (data.Length < requiredBytes)
                throw new ArgumentException($"The array was expected to be of size {requiredBytes}; actual array was of size {data.Length}.");
            SetPixelBytes(data, 0, width * bpp, x, y, width, height);
        }

        public void GetPixelBytes(IBuffer buffer)
        {
            if (buffer is null)
                throw new ArgumentException("Buffer cannot be null.", nameof(buffer));
            ThrowIfDisposed();

            int bpp = GetBytesPerPixel(Format);
            int byteCount = _bitmap.Width * _bitmap.Height * bpp;
            if (buffer.Capacity < (uint)byteCount)
                throw new ArgumentException($"The array was expected to be of size {byteCount}; actual array was of size {buffer.Capacity}.");

            byte[] result = _pixelData ?? GetPixelBytes();
            WriteBufferFromBytes(buffer, result);
        }

        public void GetPixelBytes(IBuffer buffer, int x, int y, int width, int height)
        {
            if (buffer is null)
                throw new ArgumentException("Buffer cannot be null.", nameof(buffer));
            ThrowIfDisposed();
            if (x < 0 || x + width > _bitmap.Width)
                throw new ArgumentException("The bitmap pixel format is unsupported. 0x88982F80");
            if (y < 0 || y + height > _bitmap.Height)
                throw new ArgumentException("The bitmap pixel format is unsupported. 0x88982F80");
            if (width <= 0)
                throw new ArgumentException("The bitmap pixel format is unsupported. 0x88982F80");
            if (height <= 0)
                throw new ArgumentException("The bitmap pixel format is unsupported. 0x88982F80");

            int bpp = GetBytesPerPixel(Format);
            int requiredBytes = checked(width * height * bpp);
            if (buffer.Capacity < (uint)requiredBytes)
                throw new ArgumentException($"The array was expected to be of size {requiredBytes}; actual array was of size {buffer.Capacity}.");

            byte[] result = new byte[requiredBytes];
            int offset = 0;
            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    SKColor color = _bitmap.GetPixel(x + column, y + row);
                    switch (Format)
                    {
                        case DirectXPixelFormat.A8UIntNormalized:
                            result[offset++] = color.Alpha;
                            break;
                        case DirectXPixelFormat.R8UIntNormalized:
                            result[offset++] = color.Red;
                            break;
                        case DirectXPixelFormat.R8G8UIntNormalized:
                            result[offset++] = color.Red;
                            result[offset++] = color.Green;
                            break;
                        default:
                            result[offset++] = color.Blue;
                            result[offset++] = color.Green;
                            result[offset++] = color.Red;
                            result[offset++] = color.Alpha;
                            break;
                    }
                }
            }

            WriteBufferFromBytes(buffer, result);
        }

        public void SetPixelBytes(byte[] sourceBytes, int sourceOffset, int sourceStride, int destinationX, int destinationY, int width, int height)
        {
            int bpp = GetBytesPerPixel(Format);
            for (int row = 0; row < height; row++)
            {
                int srcRow = sourceOffset + row * sourceStride;
                for (int col = 0; col < width; col++)
                {
                    int srcPos = srcRow + col * bpp;
                    byte[] pixel = new byte[bpp];
                    Array.Copy(sourceBytes, srcPos, pixel, 0, bpp);
                    SKColor color = Format switch
                    {
                        DirectXPixelFormat.A8UIntNormalized => new SKColor(0, 0, 0, pixel[0]),
                        DirectXPixelFormat.R8UIntNormalized => new SKColor(pixel[0], 0, 0, (byte)255),
                        DirectXPixelFormat.R8G8UIntNormalized => new SKColor(pixel[0], pixel[1], 0, (byte)255),
                        _ => new SKColor(pixel[2], pixel[1], pixel[0], pixel.Length > 3 ? pixel[3] : (byte)255),
                    };
                    _bitmap.SetPixel(destinationX + col, destinationY + row, color);
                }
            }
            _bitmap.NotifyPixelsChanged();
            RebuildPixelData();
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

            var bitmap = new SKBitmap(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Unpremul));
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = colors[y * width + x];
                    bitmap.SetPixel(x, y, new SKColor(color.R, color.G, color.B, color.A));
                }
            }

            return new CanvasBitmap(bitmap, dpi, device);
        }

        public static CanvasBitmap CreateFromRenderTarget(CanvasRenderTarget renderTarget)
        {
            ArgumentNullException.ThrowIfNull(renderTarget);
            return new CanvasBitmap(renderTarget.SnapshotBitmap(), renderTarget.Dpi, renderTarget.Device);
        }

        public static CanvasBitmap CreateFromDirect3D11Surface(CanvasDevice device, object direct3DSurface)
        {
            return CreateFromBytes(device, Array.Empty<byte>(), 1, 1, DirectXPixelFormat.B8G8R8A8UIntNormalized);
        }

        public static CanvasBitmap CreateFromDirect3D11Surface(CanvasDevice device, object direct3DSurface, float dpi)
        {
            return CreateFromBytes(device, Array.Empty<byte>(), 1, 1, DirectXPixelFormat.B8G8R8A8UIntNormalized, dpi);
        }

        public static CanvasBitmap CreateFromDirect3D11Surface(CanvasDevice device, object direct3DSurface, float dpi, CanvasAlphaMode alpha)
        {
            return CreateFromBytes(device, Array.Empty<byte>(), 1, 1, DirectXPixelFormat.B8G8R8A8UIntNormalized, dpi, alpha);
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

        public Color[] GetPixelColors(int left, int top, int width, int height)
        {
            ThrowIfDisposed();
            var colors = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    SKColor color = _bitmap.GetPixel(left + x, top + y);
                    colors[y * width + x] = Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
                }
            }
            return colors;
        }

        public void SetPixelColors(Color[] colors, int left, int top, int width, int height)
        {
            ArgumentNullException.ThrowIfNull(colors);
            ThrowIfDisposed();
            if (colors.Length < width * height)
                throw new ArgumentException("The color array does not contain enough pixels.", nameof(colors));

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color color = colors[y * width + x];
                    _bitmap.SetPixel(left + x, top + y, new SKColor(color.R, color.G, color.B, color.A));
                }
            }
        }

        public void CopyPixelsFromBitmap(CanvasBitmap sourceBitmap, int destX, int destY)
        {
            ArgumentNullException.ThrowIfNull(sourceBitmap);
            ThrowIfDisposed();
            for (int y = 0; y < sourceBitmap._bitmap.Height && destY + y < _bitmap.Height; y++)
            {
                for (int x = 0; x < sourceBitmap._bitmap.Width && destX + x < _bitmap.Width; x++)
                {
                    _bitmap.SetPixel(destX + x, destY + y, sourceBitmap._bitmap.GetPixel(x, y));
                }
            }
        }

        public void CopyPixelsFromBitmap(CanvasBitmap sourceBitmap, int destX, int destY, int srcLeft, int srcTop, int srcWidth, int srcHeight)
        {
            ArgumentNullException.ThrowIfNull(sourceBitmap);
            ThrowIfDisposed();
            for (int y = 0; y < srcHeight && destY + y < _bitmap.Height; y++)
            {
                for (int x = 0; x < srcWidth && destX + x < _bitmap.Width; x++)
                {
                    _bitmap.SetPixel(destX + x, destY + y, sourceBitmap._bitmap.GetPixel(srcLeft + x, srcTop + y));
                }
            }
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

        public async Task SaveAsync(Stream stream, CanvasBitmapFileFormat format, float quality)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ThrowIfDisposed();

            using SKImage image = SKImage.FromBitmap(_bitmap);
            using SKData data = image.Encode(ToSkFormat(format), (int)(quality * 100))
                ?? throw new InvalidOperationException("Failed to encode bitmap.");
            await data.AsStream().CopyToAsync(stream).ConfigureAwait(false);
            await stream.FlushAsync().ConfigureAwait(false);
        }

        public async Task SaveAsync(string fileName, CanvasBitmapFileFormat format, float quality)
        {
            using var stream = File.Create(fileName);
            await SaveAsync(stream, format, quality).ConfigureAwait(false);
        }

        public async Task SaveAsync(string fileName, CanvasBitmapFileFormat format)
        {
            using var stream = File.Create(fileName);
            await SaveAsync(stream, format).ConfigureAwait(false);
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
