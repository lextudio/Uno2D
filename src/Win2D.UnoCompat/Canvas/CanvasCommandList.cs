using SkiaSharp;
using Windows.Foundation;
using Windows.Graphics.Effects;
using Microsoft.Graphics.Canvas.Effects;

namespace Microsoft.Graphics.Canvas
{
    public sealed class CanvasCommandList : IDisposable, ICanvasImage, IGraphicsEffectSource
    {
        private readonly SKSurface _surface;
        private bool _disposed;

        public CanvasCommandList(CanvasDevice device, int width = 1024, int height = 1024, float dpi = 96f)
        {
            ArgumentNullException.ThrowIfNull(device);
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            Size = new Size(width, height);
            Dpi = dpi;
            Device = device;
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            _surface = SKSurface.Create(info) ?? throw new InvalidOperationException("Unable to create command list surface.");
            _surface.Canvas.Clear(SKColors.Transparent);
        }

        public Size Size { get; }

        public float Dpi { get; }

        public CanvasDevice Device { get; }

        public CanvasDrawingSession CreateDrawingSession()
        {
            ThrowIfDisposed();
            return new CanvasDrawingSession(_surface.Canvas, Device);
        }

        public void Draw(CanvasDrawingSession drawingSession)
        {
            ArgumentNullException.ThrowIfNull(drawingSession);
            ThrowIfDisposed();
            _surface.Canvas.Flush();
            using SKImage image = _surface.Snapshot();
            drawingSession.DrawSkImage(image, 0, 0);
        }

        public Rect GetBounds()
        {
            return new Rect(0, 0, Size.Width, Size.Height);
        }

        internal SKImage GetImage()
        {
            ThrowIfDisposed();
            _surface.Canvas.Flush();
            return _surface.Snapshot();
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
                throw new ObjectDisposedException(nameof(CanvasCommandList));
        }
    }
}
