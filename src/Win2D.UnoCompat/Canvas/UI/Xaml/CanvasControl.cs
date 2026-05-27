using SkiaSharp.Views.Windows;
using System;

namespace Microsoft.Graphics.Canvas.UI.Xaml
{
    public delegate void TypedCanvasDrawEventHandler(CanvasControl sender, CanvasDrawEventArgs args);

    public sealed class CanvasDrawEventArgs : EventArgs
    {
        public CanvasDrawEventArgs(Canvas.CanvasDrawingSession drawingSession)
        {
            DrawingSession = drawingSession;
        }

        public Canvas.CanvasDrawingSession DrawingSession { get; }
    }

    public sealed class CanvasControl : SKXamlCanvas
    {
        public event TypedCanvasDrawEventHandler? Draw;

        public CanvasControl()
        {
            PaintSurface += OnPaintSurface;
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            // Unify drawing coordinates with XAML pointer/layout coordinates (DIP).
            // SK surface size is device pixels; ActualWidth/ActualHeight are DIP.
            // Without this scaling, content appears cramped to top-left and hit-testing drifts.
            if (TryComputeDipScale(e.Info.Width, e.Info.Height, ActualWidth, ActualHeight, out float sx, out float sy))
            {
                e.Surface.Canvas.Scale(sx, sy);
            }

            using var ds = new Canvas.CanvasDrawingSession(e.Surface.Canvas);
            Draw?.Invoke(this, new CanvasDrawEventArgs(ds));
        }

        public new void Invalidate()
        {
            base.Invalidate();
        }

        public static bool TryComputeDipScale(
            int pixelWidth,
            int pixelHeight,
            double actualWidthDip,
            double actualHeightDip,
            out float sx,
            out float sy)
        {
            sx = 1f;
            sy = 1f;
            if (pixelWidth <= 0 || pixelHeight <= 0 || actualWidthDip <= 0 || actualHeightDip <= 0)
                return false;

            sx = pixelWidth / (float)actualWidthDip;
            sy = pixelHeight / (float)actualHeightDip;
            return sx > 0f && sy > 0f;
        }
    }
}
