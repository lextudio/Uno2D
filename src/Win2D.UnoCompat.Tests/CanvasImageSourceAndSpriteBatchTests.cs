using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Windows.Foundation;
using Windows.UI;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasImageSourceAndSpriteBatchTests
{
    [Fact]
    public void CanvasImageSource_CreatesBitmapSnapshot()
    {
        using var source = new CanvasImageSource(CanvasDevice.GetSharedDevice(), 4, 4, 96);
        using (CanvasDrawingSession ds = source.CreateDrawingSession(Color.FromArgb(0, 0, 0, 0)))
            ds.FillRectangle(1, 1, 1, 1, Color.FromArgb(255, 10, 20, 30));

        using CanvasBitmap bitmap = source.CreateBitmap();
        byte[] pixel = new byte[4];
        bitmap.CopyPixels(pixel, 1, 1, 1, 1);

        pixel.Should().Equal(30, 20, 10, 255);
        source.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void CanvasVirtualImageSource_TracksInvalidRegions()
    {
        using var source = new CanvasVirtualImageSource(CanvasDevice.GetSharedDevice(), 8192, 8192, 96, 256);
        var region = new Rect(1024, 1024, 128, 128);

        source.Invalidate(region);

        source.TileSize.Should().Be(256);
        source.InvalidRegions.Should().ContainSingle().Which.Should().Be(region);
        source.GetInvalidRegions().Should().ContainSingle().Which.Should().Be(region);
        source.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void CanvasImageSource_ExposesDeviceAndSize()
    {
        CanvasDevice device = CanvasDevice.GetSharedDevice();
        using var source = new CanvasImageSource(device, 4, 8, 96);

        source.Device.Should().BeSameAs(device);
        source.Size.Should().Be(new Size(4, 8));
        source.SizeInPixels.Should().Be(new Size(4, 8));
    }

    [Fact]
    public void CanvasVirtualImageSource_CreateDrawingSessionWithClearColorAndRect_DrawsPixels()
    {
        using var source = new CanvasVirtualImageSource(CanvasDevice.GetSharedDevice(), 8, 8, 96, 4);
        var region = new Rect(0, 0, 4, 4);

        using (CanvasDrawingSession ds = source.CreateDrawingSession(Color.FromArgb(255, 0, 0, 0), region))
            ds.FillRectangle(0, 0, 4, 4, Color.FromArgb(255, 10, 20, 30));

        using CanvasBitmap bitmap = source.CreateBitmap();
        byte[] pixel = new byte[4];
        bitmap.CopyPixels(pixel, 1, 1, 1, 1);

        pixel.Should().Equal(30, 20, 10, 255);
        source.InvalidRegions.Should().Contain(region);
    }

    [Fact]
    public void CanvasSpriteBatch_FlushDrawsQueuedSprites()
    {
        using CanvasBitmap sprite = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [Color.FromArgb(255, 100, 50, 25)],
            1,
            1);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 4, 1, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        using var batch = new CanvasSpriteBatch(ds);

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        for (int i = 0; i < 4; i++)
            batch.Draw(sprite, new Rect(i, 0, 1, 1));
        batch.Flush();

        target.GetPixelBytes().Should().Contain(value => value != 0);
        batch.Count.Should().Be(0);
    }

    [Fact]
    public void CanvasSpriteBatch_EnforcesMaximumSprites()
    {
        using CanvasBitmap sprite = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [Color.FromArgb(255, 255, 255, 255)],
            1,
            1);
        using var batch = new CanvasSpriteBatch(CanvasDevice.GetSharedDevice()) { MaximumSpritesPerBatch = 1 };

        batch.Draw(sprite, new Rect(0, 0, 1, 1));
        Action secondDraw = () => batch.Draw(sprite, new Rect(1, 0, 1, 1));

        secondDraw.Should().Throw<InvalidOperationException>();
        batch.IsFailed.Should().BeTrue();
    }

    [Fact]
    public void CanvasImageSource_FromResourceCreator_UsesDeviceAndDpi()
    {
        var creator = new FakeResourceCreator(CanvasDevice.GetSharedDevice(), 144f);
        using var source = new CanvasImageSource(creator, 4, 4);

        source.Device.Should().BeSameAs(creator.Device);
        source.Dpi.Should().Be(144f);
    }

    [Fact]
    public void CanvasVirtualImageSource_FromResourceCreator_UsesDeviceAndDpi()
    {
        var creator = new FakeResourceCreator(CanvasDevice.GetSharedDevice(), 144f);
        using var source = new CanvasVirtualImageSource(creator, 512, 512, 128);

        source.Device.Should().BeSameAs(creator.Device);
        source.Dpi.Should().Be(144f);
        source.TileSize.Should().Be(128);
    }

    [Fact]
    public void CanvasCommandList_FromResourceCreator_UsesDevice()
    {
        var creator = new FakeResourceCreator(CanvasDevice.GetSharedDevice(), 96f);
        using var commandList = new CanvasCommandList(creator);

        commandList.Device.Should().BeSameAs(creator.Device);
        commandList.Size.Should().Be(new Size(1024, 1024));
    }

    private sealed class FakeResourceCreator : ICanvasResourceCreatorWithDpi
    {
        public FakeResourceCreator(CanvasDevice device, float dpi)
        {
            Device = device;
            Dpi = dpi;
        }

        public CanvasDevice Device { get; }
        public float Dpi { get; }

        public int ConvertDipsToPixels(float dips, CanvasDpiRounding dpiRounding) => (int)Math.Round(dips * Dpi / 96f);
        public float ConvertPixelsToDips(int pixels) => pixels * 96f / Dpi;
    }
}
