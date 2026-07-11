using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Windows.Foundation;
using Windows.UI;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasMiscTests
{
    [Fact]
    public void CachedGeometry_Draw_StrokesPath()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(1, 1, 4, 4));
        using var cached = new CanvasCachedGeometry(geometry, 2);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 8, 8, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        cached.Draw(ds, Color.FromArgb(255, 90, 80, 70), 1);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void CachedGeometry_Fill_WithBrush_FillsPath()
    {
        using var brush = new CanvasSolidColorBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 10, 20, 30));
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(1, 1, 4, 4));
        using var cached = new CanvasCachedGeometry(geometry);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 8, 8, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        cached.Fill(ds, brush);

        byte[] pixels = target.GetPixelBytes();
        int offset = ((1 * 8) + 1) * 4;
        pixels[offset + 0].Should().Be(30);
        pixels[offset + 1].Should().Be(20);
        pixels[offset + 2].Should().Be(10);
        pixels[offset + 3].Should().Be(255);
    }

    [Fact]
    public void CommandList_ReportsSize()
    {
        using var commandList = new CanvasCommandList(CanvasDevice.GetSharedDevice(), 8, 16, 96);

        commandList.Size.Width.Should().Be(8);
        commandList.Size.Height.Should().Be(16);
        commandList.Dpi.Should().Be(96);
    }

    [Fact]
    public void Bitmap_ReportsDpiAndSize()
    {
        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [Color.FromArgb(255, 255, 0, 0)],
            1, 1, 144);

        bitmap.Dpi.Should().Be(144);
        bitmap.Size.Width.Should().Be(1);
        bitmap.Size.Height.Should().Be(1);
    }

    [Fact]
    public void Bitmap_GetBounds_ReturnsBounds()
    {
        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [Color.FromArgb(255, 255, 0, 0)],
            1, 1);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 1, 1, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        Rect bounds = bitmap.GetBounds(ds);

        bounds.Width.Should().Be(1);
        bounds.Height.Should().Be(1);
    }

    [Fact]
    public async Task Bitmap_LoadAsync_WithBytes_LoadsBitmap()
    {
        using CanvasBitmap original = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [Color.FromArgb(255, 12, 34, 56)],
            1, 1);
        await using var stream = new MemoryStream();
        await original.SaveAsync(stream, CanvasBitmapFileFormat.Png);
        byte[] bytes = stream.ToArray();

        using CanvasBitmap loaded = await CanvasBitmap.LoadAsync(CanvasDevice.GetSharedDevice(), bytes);

        loaded.SizeInPixels.Width.Should().Be(1);
        loaded.SizeInPixels.Height.Should().Be(1);
    }

    [Fact]
    public void ImageSource_ReportsWidthHeightDpi()
    {
        using var source = new CanvasImageSource(CanvasDevice.GetSharedDevice(), 10, 20, 144);

        source.Width.Should().Be(10);
        source.Height.Should().Be(20);
        source.Dpi.Should().Be(144);
    }

    [Fact]
    public void ImageSource_Invalidate_SetsIsDirty()
    {
        using var source = new CanvasImageSource(CanvasDevice.GetSharedDevice(), 4, 4, 96);
        using (CanvasDrawingSession ds = source.CreateDrawingSession(Color.FromArgb(0, 0, 0, 0)))
            ds.FillRectangle(0, 0, 4, 4, Color.FromArgb(255, 255, 0, 0));
        using CanvasBitmap bitmap = source.CreateBitmap();
        source.IsDirty.Should().BeFalse();

        source.Invalidate();

        source.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void ImageSource_ImageSource_IsNull()
    {
        using var source = new CanvasImageSource(CanvasDevice.GetSharedDevice(), 4, 4, 96);

        source.ImageSource.Should().BeNull();
    }

    [Fact]
    public void VirtualImageSource_CreateDrawingSession_WithRegion_AddsRegion()
    {
        using var source = new CanvasVirtualImageSource(CanvasDevice.GetSharedDevice(), 100, 100, 96, 32);
        var region = new Rect(10, 10, 20, 20);

        using CanvasDrawingSession ds = source.CreateDrawingSession(region);

        source.InvalidRegions.Should().ContainSingle().Which.Should().Be(region);
    }

    [Fact]
    public void SpriteBatch_Flush_WithExternalSession_DrawsSprites()
    {
        using CanvasBitmap sprite = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [Color.FromArgb(255, 100, 50, 25)],
            1, 1);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 4, 1, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        using var batch = new CanvasSpriteBatch(CanvasDevice.GetSharedDevice());

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        batch.Draw(sprite, new Rect(0, 0, 1, 1));
        batch.Draw(sprite, new Rect(1, 0, 1, 1));
        batch.Flush(ds);

        target.GetPixelBytes().Should().Contain(value => value != 0);
        batch.Count.Should().Be(0);
    }

    [Fact]
    public void RenderTarget_SaveAsync_ToStream_DoesNotThrow()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 2, 2, 96);
        using (CanvasDrawingSession ds = target.CreateDrawingSession())
            ds.Clear(Color.FromArgb(255, 255, 0, 0));

        using var stream = new MemoryStream();
        Func<Task> act = async () => await target.SaveAsync(stream, CanvasBitmapFileFormat.Png);

        act.Should().NotThrowAsync();
        stream.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void RenderTarget_Dpi_ReturnsValue()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 4, 4, 144);

        target.Dpi.Should().Be(144);
    }

    [Fact]
    public void Bitmap_LoadAsync_WithString_ThrowsOnInvalidPath()
    {
        Func<Task> act = async () => await CanvasBitmap.LoadAsync(CanvasDevice.GetSharedDevice(), "nonexistent.png");

        act.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public void ImageSource_CreateDrawingSession_WithoutClear_DoesNotThrow()
    {
        using var source = new CanvasImageSource(CanvasDevice.GetSharedDevice(), 4, 4, 96);

        using CanvasDrawingSession ds = source.CreateDrawingSession(Color.FromArgb(0, 0, 0, 0));

        ds.Should().NotBeNull();
    }

    [Fact]
    public void VirtualImageSource_TileSize_ReturnsValue()
    {
        using var source = new CanvasVirtualImageSource(CanvasDevice.GetSharedDevice(), 100, 100, 96, 64);

        source.TileSize.Should().Be(64);
    }

    [Fact]
    public void CachedGeometry_Draw_WithStrokeStyle_DoesNotThrow()
    {
        var style = new CanvasStrokeStyle { StartCap = CanvasCapStyle.Round };
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(1, 1, 4, 4));
        using var cached = new CanvasCachedGeometry(geometry, 2, style);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 8, 8, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        cached.Draw(ds, Color.FromArgb(255, 90, 80, 70), 1, style);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }
}
