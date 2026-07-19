using SkiaSharp;
using System.Numerics;
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

        public CanvasCommandList(ICanvasResourceCreator resourceCreator)
            : this(GetDevice(resourceCreator))
        {
        }

        private static CanvasDevice GetDevice(ICanvasResourceCreator resourceCreator)
        {
            ArgumentNullException.ThrowIfNull(resourceCreator);
            return resourceCreator.Device;
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

        public Rect GetBounds(ICanvasResourceCreator resourceCreator)
        {
            ArgumentNullException.ThrowIfNull(resourceCreator);
            return GetBounds();
        }

        public Rect GetBounds(ICanvasResourceCreator resourceCreator, Matrix3x2 transform)
        {
            ArgumentNullException.ThrowIfNull(resourceCreator);
            Rect bounds = GetBounds();
            SKRect skBounds = new((float)bounds.X, (float)bounds.Y, (float)(bounds.X + bounds.Width), (float)(bounds.Y + bounds.Height));
            SKMatrix matrix = new(transform.M11, transform.M21, transform.M31, transform.M12, transform.M22, transform.M32, 0, 0, 1);
            SKRect mapped = matrix.MapRect(skBounds);
            return new Rect(mapped.Left, mapped.Top, mapped.Width, mapped.Height);
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
