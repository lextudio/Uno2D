using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Windows.Foundation;
using Windows.UI;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasCachedGeometryTests
{
    [Fact]
    public void CachedGeometry_FillsCachedPath()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(1, 1, 2, 2));
        using var cached = new CanvasCachedGeometry(geometry);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 4, 4, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        cached.Fill(ds, Color.FromArgb(255, 1, 2, 3));

        PixelAt(target.GetPixelBytes(), 4, 1, 1).Should().Be((1, 2, 3, 255));
        PixelAt(target.GetPixelBytes(), 4, 0, 0).A.Should().Be(0);
    }

    [Fact]
    public void CachedGeometry_WithStrokeConstructor_FillsStrokedOutline()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(2, 2, 4, 4));
        using var cached = new CanvasCachedGeometry(geometry, 2);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 8, 8, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        cached.Fill(ds, Color.FromArgb(255, 90, 80, 70));

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    private static (byte R, byte G, byte B, byte A) PixelAt(byte[] pixels, int width, int x, int y)
    {
        int offset = ((y * width) + x) * 4;
        return (pixels[offset + 2], pixels[offset + 1], pixels[offset], pixels[offset + 3]);
    }
}
