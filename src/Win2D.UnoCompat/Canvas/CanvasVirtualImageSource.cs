using Windows.Foundation;
using Windows.UI;

namespace Microsoft.Graphics.Canvas
{
    public sealed class CanvasVirtualImageSource : CanvasImageSource
    {
        private readonly List<Rect> _invalidRegions = new();

        public CanvasVirtualImageSource(CanvasDevice device, float width, float height, float dpi, int tileSize = 512)
            : base(device, width, height, dpi)
        {
            if (tileSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(tileSize));

            TileSize = tileSize;
        }

        public int TileSize { get; }

        public IReadOnlyList<Rect> InvalidRegions => _invalidRegions;

        public CanvasDrawingSession CreateDrawingSession(Rect region)
        {
            _invalidRegions.Add(region);
            return CreateDrawingSession(Color.FromArgb(0, 0, 0, 0));
        }

        public void Invalidate(Rect region)
        {
            _invalidRegions.Add(region);
            Invalidate();
        }
    }
}
