using FluentAssertions;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasControlScaleTests
{
    [Fact]
    public void TryComputeDipScale_ReturnsExpectedScale_ForHiDpiSurface()
    {
        bool ok = CanvasControl.TryComputeDipScale(
            pixelWidth: 1728,
            pixelHeight: 978,
            actualWidthDip: 864,
            actualHeightDip: 489,
            out float sx,
            out float sy);

        ok.Should().BeTrue();
        sx.Should().BeApproximately(2f, 0.001f);
        sy.Should().BeApproximately(2f, 0.001f);
    }

    [Fact]
    public void TryComputeDipScale_ReturnsFalse_ForInvalidInputs()
    {
        bool ok = CanvasControl.TryComputeDipScale(0, 100, 100, 100, out _, out _);
        ok.Should().BeFalse();
    }
}
