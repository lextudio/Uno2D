using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasCommandListTests
{
    [Fact]
    public void CommandList_ReplaysRecordedDrawing()
    {
        using var commandList = new CanvasCommandList(CanvasDevice.GetSharedDevice(), 4, 4);
        using (CanvasDrawingSession ds = commandList.CreateDrawingSession())
        {
            ds.Clear(Color.FromArgb(0, 0, 0, 0));
            ds.FillRectangle(1, 1, 2, 2, Color.FromArgb(255, 10, 20, 30));
        }

        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 4, 4, 96);
        using CanvasDrawingSession targetSession = target.CreateDrawingSession();
        targetSession.Clear(Color.FromArgb(0, 0, 0, 0));

        commandList.Draw(targetSession);

        PixelAt(target.GetPixelBytes(), 4, 1, 1).Should().Be((10, 20, 30, 255));
        PixelAt(target.GetPixelBytes(), 4, 0, 0).A.Should().Be(0);
    }

    [Fact]
    public void CommandList_ReplayIsIdempotent()
    {
        using var commandList = new CanvasCommandList(CanvasDevice.GetSharedDevice(), 2, 2);
        using (CanvasDrawingSession ds = commandList.CreateDrawingSession())
        {
            ds.Clear(Color.FromArgb(0, 0, 0, 0));
            ds.FillRectangle(0, 0, 1, 1, Color.FromArgb(255, 200, 100, 50));
        }

        using var first = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 2, 2, 96);
        using var second = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 2, 2, 96);
        using (CanvasDrawingSession ds = first.CreateDrawingSession())
            commandList.Draw(ds);
        using (CanvasDrawingSession ds = second.CreateDrawingSession())
            commandList.Draw(ds);

        first.GetPixelBytes().Should().Equal(second.GetPixelBytes());
    }

    private static (byte R, byte G, byte B, byte A) PixelAt(byte[] pixels, int width, int x, int y)
    {
        int offset = ((y * width) + x) * 4;
        return (pixels[offset + 2], pixels[offset + 1], pixels[offset], pixels[offset + 3]);
    }
}
