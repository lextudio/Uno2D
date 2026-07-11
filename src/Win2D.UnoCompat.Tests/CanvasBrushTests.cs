using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Windows.Foundation;
using Windows.UI;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasBrushTests
{
    [Fact]
    public void SolidColorBrush_FillsRectangleWithOpacity()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 2, 2, 96);
        using var brush = new CanvasSolidColorBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 20, 40, 60))
        {
            Opacity = 0.5f,
        };
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.FillRectangle(new Rect(0, 0, 2, 2), brush);

        byte[] pixels = target.GetPixelBytes();
        pixels[0].Should().BeInRange((byte)29, (byte)31);
        pixels[1].Should().BeInRange((byte)19, (byte)21);
        pixels[2].Should().BeInRange((byte)9, (byte)11);
        pixels[3].Should().BeInRange((byte)126, (byte)128);
    }

    [Fact]
    public void ImageBrush_RepeatsBitmapWhenExtendWrap()
    {
        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [
                Color.FromArgb(255, 255, 0, 0),
                Color.FromArgb(255, 0, 255, 0),
            ],
            2,
            1);
        using var brush = new CanvasImageBrush(CanvasDevice.GetSharedDevice())
        {
            Image = bitmap,
            ExtendX = CanvasEdgeBehavior.Wrap,
            ExtendY = CanvasEdgeBehavior.Wrap,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 4, 1, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.FillRectangle(new Rect(0, 0, 4, 1), brush);

        byte[] pixels = target.GetPixelBytes();
        PixelAt(pixels, 4, 0, 0).Should().Be((255, 0, 0, 255));
        PixelAt(pixels, 4, 1, 0).Should().Be((0, 255, 0, 255));
        PixelAt(pixels, 4, 2, 0).Should().Be((255, 0, 0, 255));
        PixelAt(pixels, 4, 3, 0).Should().Be((0, 255, 0, 255));
    }

    [Fact]
    public void LinearGradientBrush_InterpolatesAcrossRectangle()
    {
        using var brush = new CanvasLinearGradientBrush(
            CanvasDevice.GetSharedDevice(),
            Color.FromArgb(255, 255, 0, 0),
            Color.FromArgb(255, 0, 0, 255))
        {
            StartPoint = new Point(0, 0),
            EndPoint = new Point(9, 0),
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 1, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.FillRectangle(new Rect(0, 0, 10, 1), brush);

        byte[] pixels = target.GetPixelBytes();
        PixelAt(pixels, 10, 0, 0).R.Should().BeGreaterThan(PixelAt(pixels, 10, 9, 0).R);
        PixelAt(pixels, 10, 9, 0).B.Should().BeGreaterThan(PixelAt(pixels, 10, 0, 0).B);
    }

    [Fact]
    public void RadialGradientBrush_RendersCenterDifferentlyFromEdge()
    {
        using var brush = new CanvasRadialGradientBrush(
            CanvasDevice.GetSharedDevice(),
            Color.FromArgb(255, 255, 255, 255),
            Color.FromArgb(255, 0, 0, 0))
        {
            Center = new Point(5, 5),
            Radius = 5,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 11, 11, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.FillRectangle(new Rect(0, 0, 11, 11), brush);

        byte[] pixels = target.GetPixelBytes();
        PixelAt(pixels, 11, 5, 5).R.Should().BeGreaterThan(PixelAt(pixels, 11, 0, 0).R);
    }

    [Fact]
    public void BrushOverloads_WorkForGeometryAndText()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 64, 32, 96);
        using var brush = new CanvasSolidColorBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 255, 255, 255));
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 10));

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.FillGeometry(geometry, brush);
        ds.DrawText("A", new Rect(16, 0, 32, 32), brush, new CanvasTextFormat { FontSize = 24 });

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    private static (byte R, byte G, byte B, byte A) PixelAt(byte[] pixels, int width, int x, int y)
    {
        int offset = ((y * width) + x) * 4;
        return (pixels[offset + 2], pixels[offset + 1], pixels[offset], pixels[offset + 3]);
    }
}
