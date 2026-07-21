using SkiaSharp;
using System.IO;
using Windows.Foundation;
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
        public event EventHandler<CanvasPrintTaskOptionsChangedEventArgs>? PrintTaskOptionsChanged;
        public event EventHandler<CanvasPreviewEventArgs>? Preview;
        public event EventHandler<CanvasPrintEventArgs>? Print;

        public CanvasPrintDocument(CanvasPrintDevice? device = null)
        {
            Device = device ?? CanvasPrintDevice.GetDefault();
        }

        public CanvasPrintDevice Device { get; }

        public void InvalidatePreview()
        {
        }

        public void SetPageCount(uint count)
        {
        }

        public void SetIntermediatePageCount(uint count)
        {
        }

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

    public sealed class CanvasPrintTaskOptionsChangedEventArgs
    {
        public int CurrentPreviewPageNumber { get; }
        public int NewPreviewPageNumber { get; set; }
        public object? PrintTaskOptions { get; }

        public CanvasPrintDeferral GetDeferral() => new();
    }

    public sealed class CanvasPreviewEventArgs
    {
        public int PageNumber { get; }
        public object? PrintTaskOptions { get; }
        public CanvasDrawingSession? DrawingSession { get; }

        public CanvasPrintDeferral GetDeferral() => new();
    }

    public sealed class CanvasPrintEventArgs
    {
        public object? PrintTaskOptions { get; }
        public float Dpi { get; set; } = 96f;

        public CanvasPrintDeferral GetDeferral() => new();
        public CanvasDrawingSession? CreateDrawingSession() => null;
    }

    public sealed class CanvasPrintDeferral
    {
        public void Complete()
        {
        }
    }
}
