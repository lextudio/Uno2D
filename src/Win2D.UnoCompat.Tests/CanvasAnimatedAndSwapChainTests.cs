using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.UI;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasAnimatedAndSwapChainTests
{
    [Fact]
    public void CanvasAnimatedControl_TickRaisesCreateResourcesAndUpdate()
    {
        var control = new CanvasAnimatedControlCore { FramesPerSecond = 30 };
        int createResourcesCount = 0;
        long updateCount = 0;
        control.CreateResources += (_, args) =>
        {
            args.Device.Should().BeSameAs(CanvasDevice.GetSharedDevice());
            createResourcesCount++;
        };
        control.Update += (_, args) => updateCount = args.UpdateCount;

        control.Tick();
        control.Tick();

        createResourcesCount.Should().Be(1);
        updateCount.Should().Be(2);
        control.TargetElapsedTime.TotalMilliseconds.Should().BeApproximately(33.333, 0.5);
    }

    [Fact]
    public void CanvasAnimatedControl_DoesNotTickWhenPaused()
    {
        var control = new CanvasAnimatedControlCore { IsPaused = true };
        int updates = 0;
        control.Update += (_, _) => updates++;

        control.Tick();

        updates.Should().Be(0);
        control.UpdateCount.Should().Be(0);
    }

    [Fact]
    public void CanvasSwapChainPanel_PresentsBackBuffer()
    {
        using var panel = new CanvasSwapChain();
        using (CanvasDrawingSession ds = panel.CreateDrawingSession(2, 2))
            ds.FillRectangle(1, 1, 1, 1, Color.FromArgb(255, 1, 2, 3));

        panel.Present();
        using CanvasBitmap bitmap = panel.CreateBitmap();
        byte[] pixel = new byte[4];
        bitmap.CopyPixels(pixel, 1, 1, 1, 1);

        pixel.Should().Equal(3, 2, 1, 255);
        panel.FrontBuffer.Should().NotBeNull();
    }
}
