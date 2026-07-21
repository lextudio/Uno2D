using SkiaSharp;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas.Effects;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.Graphics.DirectX;

namespace Microsoft.Graphics.Canvas
{
    public sealed class CanvasDevice : ICanvasResourceCreator, IDisposable
    {
        public static int NextDeviceFeatureLevel = 1;
        public int FeatureLevel => _featureLevel;
        private static readonly CanvasDevice _shared = new();
        private int _featureLevel = 1;

        public CanvasDevice()
        {
        }

        public CanvasDevice(bool forceSoftwareRenderer)
        {
            ForceSoftwareRenderer = forceSoftwareRenderer;
        }

        public static CanvasDevice GetSharedDevice() => _shared;

        public static CanvasDevice GetSharedDevice(bool forceSoftwareRenderer)
        {
            _shared.ForceSoftwareRenderer = forceSoftwareRenderer;
            return _shared;
        }

        public static CanvasDevice CreateFromDirect3D11Device(object direct3DDevice)
        {
            var device = new CanvasDevice();
            device._featureLevel = NextDeviceFeatureLevel;
            NextDeviceFeatureLevel = 1;
            return device;
        }

        public event EventHandler<object>? DeviceLost;
        public static CanvasDebugLevel DebugLevel { get; set; } = CanvasDebugLevel.None;

        public bool ForceSoftwareRenderer { get; set; }
        public bool LowPriority { get; set; }
        public long MaximumBitmapSizeInPixels { get; set; } = 16384;
        public long MaximumCacheSize { get; set; } = 0;
        public CanvasDevice Device => this;
        public bool IsRemoteDevice { get; set; }
        public int MaxTextureSize { get; set; } = 16384;
        public int MaxCubeTextureSize { get; set; } = 16384;
        public int MaxTextureArraySlices { get; set; } = 256;
        public int MaxVolumeTextureExtent { get; set; } = 2048;

        public void Trim()
        {
        }

        public CanvasLock Lock()
        {
            return new CanvasLock();
        }

        public bool IsBufferPrecisionSupported(CanvasBufferPrecision precision)
        {
            return true;
        }

        public bool IsPixelFormatSupported(CanvasBitmapFileFormat format)
        {
            return true;
        }

        public bool IsPixelFormatSupported(DirectXPixelFormat format)
        {
            return format switch
            {
                DirectXPixelFormat.R8G8B8A8UIntNormalized or
                DirectXPixelFormat.B8G8R8A8UIntNormalized or
                DirectXPixelFormat.B8G8R8X8UIntNormalized or
                DirectXPixelFormat.R8G8B8A8UIntNormalizedSrgb or
                DirectXPixelFormat.B8G8R8A8UIntNormalizedSrgb or
                DirectXPixelFormat.R10G10B10A2UIntNormalized or
                DirectXPixelFormat.A8UIntNormalized or
                DirectXPixelFormat.R8UIntNormalized or
                DirectXPixelFormat.R8G8UIntNormalized or
                DirectXPixelFormat.BC1UIntNormalized or
                DirectXPixelFormat.BC2UIntNormalized or
                DirectXPixelFormat.BC3UIntNormalized => true,
                _ => false,
            };
        }

        public object? GetDeviceLostReason()
        {
            return null;
        }

        public int GetDeviceLostHResult()
        {
            return 0;
        }

        public bool IsDeviceLost()
        {
            return false;
        }

        public static bool IsDeviceLost(int hresult)
        {
            return false;
        }

        public void RaiseDeviceLost()
        {
            DeviceLost?.Invoke(this, EventArgs.Empty);
        }

        internal void RaiseDeviceLostInternal()
        {
            DeviceLost?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
        }
    }

    public sealed class CanvasLock : IDisposable
    {
        public void Dispose()
        {
        }
    }

    public enum CanvasDebugLevel
    {
        None,
        Warning,
        Information,
    }

    public enum CanvasDpiRounding
    {
        Round,
        Truncate,
    }

    public sealed class CanvasRenderTarget : IDisposable, ICanvasImage
    {
        private readonly SKSurface _surface;
        private readonly CanvasDevice _device;
        private readonly int _width;
        private readonly int _height;

        private static int ToIntPixels(float dips, float dpi)
        {
            return (int)Math.Round(dips * dpi / 96f);
        }

        public CanvasRenderTarget(CanvasDevice device, int width, int height, float dpi)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            _width = width;
            _height = height;
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            _surface = SKSurface.Create(info) ?? throw new InvalidOperationException("Unable to create Skia surface.");
            Dpi = dpi;
        }

        public CanvasRenderTarget(ICanvasResourceCreatorWithDpi resourceCreator, int width, int height)
            : this(resourceCreator.Device, width, height, resourceCreator.Dpi)
        {
        }

        public CanvasRenderTarget(CanvasDevice device, float width, float height, float dpi)
            : this(device, (int)Math.Round(width), (int)Math.Round(height), dpi)
        {
        }

        public CanvasRenderTarget(CanvasDevice device, int width, int height, float dpi, DirectXPixelFormat format, CanvasAlphaMode alphaMode)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            if (!device.IsPixelFormatSupported(format))
                throw new ArgumentException("The bitmap pixel format is unsupported. 0x88982F80");
            if (!IsRenderTargetPixelFormat(format))
                throw new ArgumentException("The bitmap pixel format is unsupported. 0x88982F80");
            CanvasBitmap.ValidateAlphaMode(format, alphaMode);
            _width = width;
            _height = height;
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            _surface = SKSurface.Create(info) ?? throw new InvalidOperationException("Unable to create Skia surface.");
            Dpi = dpi;
        }

        internal static bool IsRenderTargetPixelFormat(DirectXPixelFormat format)
        {
            return format switch
            {
                DirectXPixelFormat.R8G8B8A8UIntNormalized or
                DirectXPixelFormat.B8G8R8A8UIntNormalized or
                DirectXPixelFormat.R8G8B8A8UIntNormalizedSrgb or
                DirectXPixelFormat.B8G8R8A8UIntNormalizedSrgb or
                DirectXPixelFormat.R16G16B16A16UIntNormalized or
                DirectXPixelFormat.R16G16B16A16Float or
                DirectXPixelFormat.R32G32B32A32Float or
                DirectXPixelFormat.A8UIntNormalized => true,
                _ => false,
            };
        }

        public CanvasDevice Device => _device;

        public float Dpi { get; }

        public Rect Bounds => new(0, 0, _width, _height);

        public Size Size => new(_width, _height);

        public Size SizeInPixels => new(ConvertDipsToPixels(_width), ConvertDipsToPixels(_height));

        public int Width => _width;

        public int Height => _height;

        public DirectXPixelFormat Format => DirectXPixelFormat.B8G8R8A8UIntNormalized;

        public CanvasAlphaMode AlphaMode => CanvasAlphaMode.Premultiplied;

        public int DpiX => (int)Dpi;

        public int DpiY => (int)Dpi;

        public int PixelWidth => _width;

        public int PixelHeight => _height;

        public float ConvertDipsToPixels(float dips) => dips * Dpi / 96f;

        public float ConvertPixelsToDips(int pixels) => pixels * 96f / Dpi;

        public CanvasDrawingSession CreateDrawingSession()
        {
            return new CanvasDrawingSession(_surface.Canvas, _device, Dpi);
        }

        public static CanvasRenderTarget CreateFromDirect3D11Surface(object direct3DSurface)
        {
            return new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 1, 1, 96f);
        }

        public static CanvasRenderTarget CreateFromDirect3D11Surface(CanvasDevice device, object direct3DSurface, float dpi)
        {
            return new CanvasRenderTarget(device, 1, 1, dpi);
        }

        public static CanvasRenderTarget CreateFromDirect3D11Surface(CanvasDevice device, object direct3DSurface, float dpi, CanvasAlphaMode alpha)
        {
            return new CanvasRenderTarget(device, 1, 1, dpi);
        }

        internal SKBitmap SnapshotBitmap()
        {
            SKPixmap pix = _surface.PeekPixels();
            if (pix is null)
                throw new InvalidOperationException("Unable to access surface pixels.");

            var bitmap = new SKBitmap(pix.Info);
            if (!pix.ReadPixels(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes))
            {
                bitmap.Dispose();
                throw new InvalidOperationException("Unable to copy surface pixels.");
            }

            return bitmap;
        }

        public byte[] GetPixelBytes()
        {
            SKPixmap pix = _surface.PeekPixels();
            if (pix is null)
                return Array.Empty<byte>();

            int length = pix.RowBytes * pix.Height;
            byte[] result = new byte[length];
            System.Runtime.InteropServices.Marshal.Copy(pix.GetPixels(), result, 0, length);
            return result;
        }

        public Color[] GetPixelColors()
        {
            SKPixmap pix = _surface.PeekPixels();
            if (pix is null)
                return Array.Empty<Color>();

            var colors = new Color[_width * _height];
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    SKColor color = pix.GetPixelColor(x, y);
                    colors[y * _width + x] = Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
                }
            }
            return colors;
        }

        public Color[] GetPixelColors(int left, int top, int width, int height)
        {
            SKPixmap pix = _surface.PeekPixels();
            if (pix is null)
                return Array.Empty<Color>();

            var colors = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    SKColor color = pix.GetPixelColor(left + x, top + y);
                    colors[y * width + x] = Color.FromArgb(color.Alpha, color.Red, color.Green, color.Blue);
                }
            }
            return colors;
        }

        internal SKImage GetImage()
        {
            return _surface.Snapshot();
        }

        public Task SaveAsync(string fileName)
        {
            return SaveAsync(fileName, CanvasBitmapFileFormat.Png);
        }

        public async Task SaveAsync(string fileName, CanvasBitmapFileFormat format)
        {
            using var stream = File.Create(fileName);
            await SaveAsync(stream, format).ConfigureAwait(false);
        }

        public async Task SaveAsync(object stream, CanvasBitmapFileFormat format)
        {
            SKPixmap pix = _surface.PeekPixels();
            if (pix is null)
                throw new InvalidOperationException("Unable to access surface pixels.");

            using var image = SKImage.FromPixels(pix);
            SKEncodedImageFormat encodedFormat = format switch
            {
                CanvasBitmapFileFormat.Png => SKEncodedImageFormat.Png,
                _ => SKEncodedImageFormat.Png,
            };

            using var data = image.Encode(encodedFormat, 100);
            if (data is null)
                throw new InvalidOperationException("Failed to encode image.");

            if (stream is System.IO.Stream dotnetStream)
            {
                await data.AsStream().CopyToAsync(dotnetStream).ConfigureAwait(false);
                await dotnetStream.FlushAsync().ConfigureAwait(false);
                return;
            }

            if (stream is IRandomAccessStream randomAccessStream)
            {
                var writerStream = randomAccessStream.AsStreamForWrite();
                writerStream.SetLength(0);
                await data.AsStream().CopyToAsync(writerStream).ConfigureAwait(false);
                await writerStream.FlushAsync().ConfigureAwait(false);
                return;
            }

            var writeStream = GetWindowsRuntimeStreamForWrite(stream);
            if (writeStream is not null)
            {
                await data.AsStream().CopyToAsync(writeStream).ConfigureAwait(false);
                await writeStream.FlushAsync().ConfigureAwait(false);
                return;
            }

            throw new ArgumentException("Unsupported stream type.", nameof(stream));

            static Stream? GetWindowsRuntimeStreamForWrite(object? runtimeStream)
            {
                if (runtimeStream is null)
                    return null;

                var helperType = Type.GetType("System.IO.WindowsRuntimeStreamExtensions, System.Runtime.WindowsRuntime")
                    ?? Type.GetType("WindowsRuntimeStreamExtensions, System.Runtime.WindowsRuntime")
                    ?? Type.GetType("System.IO.WindowsRuntimeStreamExtensions, System.Runtime.WindowsRuntime.UI.Xaml")
                    ?? Type.GetType("WindowsRuntimeStreamExtensions, System.Runtime.WindowsRuntime.UI.Xaml");

                if (helperType is null)
                    return null;

                var method = helperType.GetMethod("AsStreamForWrite", BindingFlags.Public | BindingFlags.Static, null, new[] { runtimeStream.GetType() }, null)
                    ?? helperType.GetMethod("AsStreamForWrite", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(object) }, null);

                if (method is null)
                    return null;

                return method.Invoke(null, new[] { runtimeStream }) as Stream;
            }
        }

        public void Dispose()
        {
            _surface.Dispose();
        }
    }
}
