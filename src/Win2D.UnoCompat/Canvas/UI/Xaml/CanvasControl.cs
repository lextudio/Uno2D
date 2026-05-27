using SkiaSharp.Views.Windows;
using System;

namespace Microsoft.Graphics.Canvas.UI.Xaml
{
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
        public event EventHandler<CanvasDrawEventArgs>? Draw;

        public CanvasControl()
        {
            PaintSurface += OnPaintSurface;
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            using var ds = new Canvas.CanvasDrawingSession(e.Surface.Canvas);
            Draw?.Invoke(this, new CanvasDrawEventArgs(ds));
        }

        public new void Invalidate()
        {
            base.Invalidate();
        }
    }
}
