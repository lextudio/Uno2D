using Windows.Foundation;
using Windows.UI;

namespace Microsoft.Graphics.Canvas
{
    public sealed class CanvasSpriteBatch : IDisposable
    {
        private readonly CanvasDrawingSession? _drawingSession;
        private readonly List<Sprite> _sprites = new();
        private bool _disposed;

        public CanvasSpriteBatch(CanvasDevice device)
        {
            ArgumentNullException.ThrowIfNull(device);
            Device = device;
        }

        public CanvasSpriteBatch(CanvasDrawingSession drawingSession)
        {
            _drawingSession = drawingSession ?? throw new ArgumentNullException(nameof(drawingSession));
            Device = drawingSession.Device;
        }

        public CanvasDevice Device { get; }

        public float Dpi => 96f;

        public int MaximumSpritesPerBatch { get; set; } = 2048;

        public CanvasSpriteSortMode SortMode { get; set; } = CanvasSpriteSortMode.None;

        public bool IsFailed { get; private set; }

        public int Count => _sprites.Count;

        public void Draw(CanvasBitmap bitmap, Rect destinationRect)
        {
            Draw(bitmap, destinationRect, bitmap.Bounds, Color.FromArgb(255, 255, 255, 255));
        }

        public void Draw(CanvasBitmap bitmap, Rect destinationRect, Rect sourceRect, Color tint)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(bitmap);
            if (_sprites.Count >= MaximumSpritesPerBatch)
            {
                IsFailed = true;
                throw new InvalidOperationException("The sprite batch exceeded MaximumSpritesPerBatch.");
            }

            _sprites.Add(new Sprite(bitmap, destinationRect, sourceRect, tint));
        }

        public void DrawFromSpriteSheet(CanvasBitmap spriteSheet, Rect destinationRect, float sourceLeft, float sourceTop, float sourceWidth, float sourceHeight, Color tint)
        {
            Draw(spriteSheet, destinationRect, new Rect(sourceLeft, sourceTop, sourceWidth, sourceHeight), tint);
        }

        public void Flush()
        {
            if (_drawingSession is null)
                throw new InvalidOperationException("This CanvasSpriteBatch was not created with a drawing session.");

            Flush(_drawingSession);
        }

        public void Flush(CanvasDrawingSession drawingSession)
        {
            ThrowIfDisposed();
            ArgumentNullException.ThrowIfNull(drawingSession);
            foreach (Sprite sprite in _sprites)
                drawingSession.DrawImage(sprite.Bitmap, sprite.DestinationRect, sprite.SourceRect, sprite.Tint);
            _sprites.Clear();
        }

        public float ConvertDipsToPixels(float dips)
        {
            return dips * Dpi / 96f;
        }

        public float ConvertPixelsToDips(float pixels)
        {
            return pixels * 96f / Dpi;
        }

        public static bool IsSupported(CanvasDevice device)
        {
            return true;
        }

        public void Dispose()
        {
            _sprites.Clear();
            _disposed = true;
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(CanvasSpriteBatch));
        }

        private readonly record struct Sprite(CanvasBitmap Bitmap, Rect DestinationRect, Rect SourceRect, Color Tint);
    }
}
