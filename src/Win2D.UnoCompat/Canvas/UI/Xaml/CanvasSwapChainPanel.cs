using SkiaSharp;
using Windows.Foundation;

namespace Microsoft.Graphics.Canvas.UI.Xaml
{
    public sealed class CanvasSwapChainPanel : CanvasControl, IDisposable
    {
        private readonly CanvasSwapChain _swapChain = new();

        public object? SwapChain => _swapChain.FrontBuffer;

        public Size BackBufferSize => _swapChain.BackBufferSize;

        public CanvasDrawingSession CreateDrawingSession(int width, int height) => _swapChain.CreateDrawingSession(width, height);

        public void Present()
        {
            _swapChain.Present();
            Invalidate();
        }

        public CanvasBitmap CreateBitmap() => _swapChain.CreateBitmap();

        public new void Dispose()
        {
            _swapChain.Dispose();
        }
    }

    public sealed class CanvasSwapChain : IDisposable
    {
        private SKSurface? _backBuffer;
        private SKImage? _frontBuffer;

        public SKImage? FrontBuffer => _frontBuffer;
        public Size BackBufferSize { get; private set; }

        public CanvasDrawingSession CreateDrawingSession(int width, int height)
        {
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            _backBuffer?.Dispose();
            BackBufferSize = new Size(width, height);
            _backBuffer = SKSurface.Create(new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul))
                ?? throw new InvalidOperationException("Unable to create swap chain back buffer.");
            _backBuffer.Canvas.Clear(SKColors.Transparent);
            return new CanvasDrawingSession(_backBuffer.Canvas);
        }

        public void Present()
        {
            if (_backBuffer is null)
                throw new InvalidOperationException("No back buffer has been created.");

            _backBuffer.Canvas.Flush();
            _frontBuffer?.Dispose();
            _frontBuffer = _backBuffer.Snapshot();
        }

        public CanvasBitmap CreateBitmap()
        {
            if (_frontBuffer is null)
                throw new InvalidOperationException("No front buffer has been presented.");

            using SKBitmap bitmap = SKBitmap.FromImage(_frontBuffer);
            return CanvasBitmap.CreateFromSkBitmap(bitmap, 96f);
        }

        public void Dispose()
        {
            _frontBuffer?.Dispose();
            _backBuffer?.Dispose();
        }
    }
}
