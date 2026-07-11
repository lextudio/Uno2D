using SkiaSharp;
using Windows.UI;

namespace Microsoft.Graphics.Canvas.Printing
{
    public sealed class CanvasPrintDevice
    {
        private static readonly CanvasPrintDevice _default = new();
        public static CanvasPrintDevice GetDefault() => _default;
    }

    public sealed class CanvasPrintPageEventArgs : EventArgs
    {
        public CanvasPrintPageEventArgs(CanvasDrawingSession drawingSession, int pageNumber)
        {
            DrawingSession = drawingSession;
            PageNumber = pageNumber;
        }

        public CanvasDrawingSession DrawingSession { get; }
        public int PageNumber { get; }
    }

    public sealed class CanvasPrintDocument
    {
        public event EventHandler<CanvasPrintPageEventArgs>? PrintPage;

        public CanvasPrintDocument(CanvasPrintDevice? device = null)
        {
            Device = device ?? CanvasPrintDevice.GetDefault();
        }

        public CanvasPrintDevice Device { get; }

        public void RenderToPdf(Stream stream, int pageCount, float width, float height)
        {
            ArgumentNullException.ThrowIfNull(stream);
            if (pageCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(pageCount));

            using SKDocument document = SKDocument.CreatePdf(stream);
            for (int page = 1; page <= pageCount; page++)
            {
                using SKCanvas canvas = document.BeginPage(width, height);
                canvas.Clear(SKColors.White);
                using var ds = new CanvasDrawingSession(canvas);
                PrintPage?.Invoke(this, new CanvasPrintPageEventArgs(ds, page));
                document.EndPage();
            }
            document.Close();
        }
    }
}
