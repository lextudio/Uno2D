using SkiaSharp;
using Windows.Foundation;
using Windows.UI;

namespace Microsoft.Graphics.Canvas
{
    public class CanvasImageSource : IDisposable
    {
        private readonly SKSurface _surface;
        private bool _isDirty = true;
        private bool _disposed;

        public CanvasImageSource(CanvasDevice device, float width, float height, float dpi)
        {
            ArgumentNullException.ThrowIfNull(device);
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            Width = width;
            Height = height;
            Dpi = dpi;
            var info = new SKImageInfo((int)MathF.Ceiling(width), (int)MathF.Ceiling(height), SKColorType.Bgra8888, SKAlphaType.Premul);
            _surface = SKSurface.Create(info) ?? throw new InvalidOperationException("Unable to create image source surface.");
            _surface.Canvas.Clear(SKColors.Transparent);
        }

        public float Width { get; }
        public float Height { get; }
        public float Dpi { get; }
        public bool IsDirty => _isDirty;
        public object? ImageSource => null;

        public CanvasDrawingSession CreateDrawingSession(Color clearColor)
        {
            ThrowIfDisposed();
            _surface.Canvas.Clear(new SKColor(clearColor.R, clearColor.G, clearColor.B, clearColor.A));
            _isDirty = true;
            return new CanvasDrawingSession(_surface.Canvas);
        }

        public void Invalidate()
        {
            ThrowIfDisposed();
            _isDirty = true;
        }

        public CanvasBitmap CreateBitmap()
        {
            ThrowIfDisposed();
            _surface.Canvas.Flush();
            _isDirty = false;
            using SKImage image = _surface.Snapshot();
            using SKBitmap bitmap = SKBitmap.FromImage(image);
            return CanvasBitmap.CreateFromSkBitmap(bitmap, Dpi);
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _surface.Dispose();
                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(CanvasImageSource));
        }
    }
}
