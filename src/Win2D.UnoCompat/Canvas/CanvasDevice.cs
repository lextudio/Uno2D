using SkiaSharp;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;

namespace Microsoft.Graphics.Canvas
{
    public sealed class CanvasDevice
    {
        private static readonly CanvasDevice _shared = new();

        public static CanvasDevice GetSharedDevice() => _shared;

        public static CanvasDevice CreateFromDirect3D11Device(object direct3DDevice)
        {
            return new CanvasDevice();
        }

        public event EventHandler<object>? DeviceLost;

        public CanvasDebugLevel DebugLevel { get; set; } = CanvasDebugLevel.None;

        public bool ForceSoftwareRenderer { get; set; }

        public bool LowPriority { get; set; }

        public long MaximumBitmapSizeInPixels { get; set; } = 16384;

        public long MaximumCacheSize { get; set; } = 0;

        public CanvasDevice Device => this;

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

        public object? GetDeviceLostReason()
        {
            return null;
        }

        public bool IsDeviceLost()
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

    public sealed class CanvasRenderTarget : IDisposable
    {
        private readonly SKSurface _surface;
        private readonly CanvasDevice _device;

        public CanvasRenderTarget(CanvasDevice device, int width, int height, float dpi)
        {
            _device = device ?? throw new ArgumentNullException(nameof(device));
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            _surface = SKSurface.Create(info) ?? throw new InvalidOperationException("Unable to create Skia surface.");
            Dpi = dpi;
        }

        public CanvasDevice Device => _device;

        public float Dpi { get; }

        public CanvasDrawingSession CreateDrawingSession()
        {
            return new CanvasDrawingSession(_surface.Canvas, _device);
        }

        public static CanvasRenderTarget CreateFromDirect3D11Surface(object direct3DSurface)
        {
            return new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 1, 1, 96f);
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
