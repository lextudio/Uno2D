using SkiaSharp;
using Windows.UI;
using Microsoft.Graphics.Canvas.Brushes;

namespace Microsoft.Graphics.Canvas.Geometry
{
    public sealed class CanvasCachedGeometry : IDisposable
    {
        private readonly SKPath _path;
        private bool _disposed;

        public CanvasCachedGeometry(CanvasGeometry geometry)
        {
            ArgumentNullException.ThrowIfNull(geometry);
            _path = new SKPath(geometry.GetPath());
        }

        public CanvasCachedGeometry(CanvasGeometry geometry, float strokeWidth, CanvasStrokeStyle? strokeStyle = null)
        {
            ArgumentNullException.ThrowIfNull(geometry);
            if (strokeWidth <= 0)
                throw new ArgumentOutOfRangeException(nameof(strokeWidth));

            using CanvasGeometry stroked = geometry.Stroke(strokeWidth, strokeStyle);
            _path = new SKPath(stroked.GetPath());
        }

        public void Fill(CanvasDrawingSession drawingSession, Color color)
        {
            ArgumentNullException.ThrowIfNull(drawingSession);
            ThrowIfDisposed();
            drawingSession.FillSkPath(_path, color);
        }

        public void Fill(CanvasDrawingSession drawingSession, ICanvasBrush brush)
        {
            ArgumentNullException.ThrowIfNull(drawingSession);
            ThrowIfDisposed();
            drawingSession.FillSkPath(_path, brush);
        }

        public void Draw(CanvasDrawingSession drawingSession, Color color, float strokeWidth = 1f, CanvasStrokeStyle? strokeStyle = null)
        {
            ArgumentNullException.ThrowIfNull(drawingSession);
            ThrowIfDisposed();
            drawingSession.DrawSkPath(_path, color, strokeWidth, strokeStyle);
        }

        internal SKPath GetPath() => _path;

        public void Dispose()
        {
            if (!_disposed)
            {
                _path.Dispose();
                _disposed = true;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(CanvasCachedGeometry));
        }
    }
}
