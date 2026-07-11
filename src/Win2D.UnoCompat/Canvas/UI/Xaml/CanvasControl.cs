using SkiaSharp.Views.Windows;
using System;
using Windows.Foundation;
using Windows.UI;

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

    public class CanvasControl : SKXamlCanvas
    {
        public event TypedCanvasDrawEventHandler? Draw;
        public event TypedCanvasCreateResourcesEventHandler? CreateResources;

        private bool _resourcesCreated;

        public CanvasControl()
        {
            PaintSurface += OnPaintSurface;
        }

        private void OnPaintSurface(object? sender, SKPaintSurfaceEventArgs e)
        {
            EnsureResourcesCreated();

            // Unify drawing coordinates with XAML pointer/layout coordinates (DIP).
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

        public void RemoveFromVisualTree()
        {
            PaintSurface -= OnPaintSurface;
        }

        public CanvasDevice Device => CanvasDevice.GetSharedDevice();

        public CanvasDevice? CustomDevice { get; set; }

        public new float Dpi => 96f;

        public float DpiScale => 1f;

        public bool ForceSoftwareRenderer { get; set; }

        public bool ReadyToDraw => true;

        public Size Size => new(ActualWidth, ActualHeight);

        public bool UseSharedDevice { get; set; } = true;

        public Color ClearColor { get; set; } = Color.FromArgb(255, 255, 255, 255);

        public float ConvertDipsToPixels(float dips)
        {
            return dips * Dpi / 96f;
        }

        public float ConvertPixelsToDips(float pixels)
        {
            return pixels * 96f / Dpi;
        }

        private void EnsureResourcesCreated()
        {
            if (_resourcesCreated) return;
            _resourcesCreated = true;
            CreateResources?.Invoke(this, new CanvasCreateResourcesEventArgs(CanvasDevice.GetSharedDevice()));
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
