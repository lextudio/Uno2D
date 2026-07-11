using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Windows.Foundation;
using Windows.UI;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasBitmapTests
{
    [Fact]
    public void CreateFromColors_ReportsSizeAndPixels()
    {
        Color[] colors =
        [
            Color.FromArgb(255, 255, 0, 0),
            Color.FromArgb(255, 0, 255, 0),
            Color.FromArgb(255, 0, 0, 255),
            Color.FromArgb(128, 255, 255, 255),
        ];

        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(CanvasDevice.GetSharedDevice(), colors, 2, 2);

        bitmap.SizeInPixels.Should().Be(new Size(2, 2));
        bitmap.Bounds.Should().Be(new Rect(0, 0, 2, 2));
        bitmap.GetPixelColors().Should().Equal(colors);
    }

    [Fact]
    public async Task SaveAsync_ProducesPngThatCanBeLoaded()
    {
        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [Color.FromArgb(255, 12, 34, 56)],
            1,
            1);
        await using var stream = new MemoryStream();

        await bitmap.SaveAsync(stream, CanvasBitmapFileFormat.Png);
        stream.Position = 0;
        using CanvasBitmap reloaded = await CanvasBitmap.LoadAsync(CanvasDevice.GetSharedDevice(), stream);

        reloaded.SizeInPixels.Should().Be(new Size(1, 1));
        reloaded.GetPixelColors()[0].Should().Be(Color.FromArgb(255, 12, 34, 56));
    }

    [Fact]
    public void CopyPixels_ExposesBgraBytes()
    {
        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [Color.FromArgb(255, 10, 20, 30)],
            1,
            1);
        byte[] pixels = new byte[4];

        bitmap.CopyPixels(pixels);

        pixels.Should().Equal(30, 20, 10, 255);
    }

    [Fact]
    public void CopyPixels_CopiesSubRectangle()
    {
        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [
                Color.FromArgb(255, 1, 2, 3),
                Color.FromArgb(255, 4, 5, 6),
                Color.FromArgb(255, 7, 8, 9),
                Color.FromArgb(255, 10, 11, 12),
            ],
            2,
            2);
        byte[] pixels = new byte[4];

        bitmap.CopyPixels(pixels, 1, 1, 1, 1);

        pixels.Should().Equal(12, 11, 10, 255);
    }

    [Fact]
    public async Task SaveAsync_ProducesJpegThatCanBeLoaded()
    {
        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [Color.FromArgb(255, 120, 130, 140)],
            1,
            1);
        await using var stream = new MemoryStream();

        await bitmap.SaveAsync(stream, CanvasBitmapFileFormat.Jpeg);
        stream.Length.Should().BeGreaterThan(0);
        stream.Position = 0;
        using CanvasBitmap reloaded = await CanvasBitmap.LoadAsync(CanvasDevice.GetSharedDevice(), stream);

        reloaded.SizeInPixels.Should().Be(new Size(1, 1));
    }

    [Fact]
    public void DrawImage_DrawsBitmapIntoRenderTarget()
    {
        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [Color.FromArgb(255, 0, 0, 255)],
            1,
            1);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 3, 3, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawImage(bitmap, new Rect(1, 1, 1, 1));

        byte[] pixels = target.GetPixelBytes();
        int center = ((1 * 3) + 1) * 4;
        pixels[center + 0].Should().Be(255);
        pixels[center + 1].Should().Be(0);
        pixels[center + 2].Should().Be(0);
        pixels[center + 3].Should().Be(255);
    }

    [Fact]
    public void CreateFromRenderTarget_CapturesPixels()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 2, 2, 96);
        using (CanvasDrawingSession ds = target.CreateDrawingSession())
        {
            ds.Clear(Color.FromArgb(0, 0, 0, 0));
            ds.FillRectangle(1, 0, 1, 1, Color.FromArgb(255, 40, 50, 60));
        }

        using CanvasBitmap bitmap = CanvasBitmap.CreateFromRenderTarget(target);
        byte[] pixels = new byte[4];
        bitmap.CopyPixels(pixels, 1, 0, 1, 1);

        pixels.Should().Equal(60, 50, 40, 255);
    }
}
