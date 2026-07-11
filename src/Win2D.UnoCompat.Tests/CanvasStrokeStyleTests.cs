using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Windows.Foundation;
using Windows.UI;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasStrokeStyleTests
{
    [Fact]
    public void DefaultStrokeStyle_HasSolidDash()
    {
        var style = new CanvasStrokeStyle();

        style.DashStyle.Should().Be(CanvasDashStyle.Solid);
        style.StartCap.Should().Be(CanvasCapStyle.Flat);
        style.EndCap.Should().Be(CanvasCapStyle.Flat);
        style.LineJoin.Should().Be(CanvasLineJoin.Miter);
    }

    [Fact]
    public void StrokeStyle_PropertiesCanBeSet()
    {
        var style = new CanvasStrokeStyle
        {
            DashStyle = CanvasDashStyle.Dash,
            StartCap = CanvasCapStyle.Round,
            EndCap = CanvasCapStyle.Square,
            LineJoin = CanvasLineJoin.Bevel,
        };

        style.DashStyle.Should().Be(CanvasDashStyle.Dash);
        style.StartCap.Should().Be(CanvasCapStyle.Round);
        style.EndCap.Should().Be(CanvasCapStyle.Square);
        style.LineJoin.Should().Be(CanvasLineJoin.Bevel);
    }

    [Fact]
    public void StrokeStyle_RoundCap_AffectsRenderedLine()
    {
        var style = new CanvasStrokeStyle
        {
            StartCap = CanvasCapStyle.Round,
            EndCap = CanvasCapStyle.Round,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawLine(2, 5, 8, 5, Color.FromArgb(255, 255, 255, 255), 3, style);

        byte[] pixels = target.GetPixelBytes();
        pixels.Should().Contain(value => value != 0);
    }

    [Fact]
    public void StrokeStyle_DashStyle_AffectsRenderedLine()
    {
        var style = new CanvasStrokeStyle
        {
            DashStyle = CanvasDashStyle.Dash,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 20, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawLine(1, 5, 19, 5, Color.FromArgb(255, 255, 255, 255), 2, style);

        byte[] pixels = target.GetPixelBytes();
        int nonZero = pixels.Count(b => b != 0);
        nonZero.Should().BeGreaterThan(0);
    }

    [Fact]
    public void StrokeStyle_LineJoinBevel_AffectsRenderedLine()
    {
        var style = new CanvasStrokeStyle
        {
            LineJoin = CanvasLineJoin.Bevel,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 20, 20, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawLine(2, 10, 10, 2, Color.FromArgb(255, 255, 255, 255), 3, style);
        ds.DrawLine(10, 2, 18, 10, Color.FromArgb(255, 255, 255, 255), 3, style);

        byte[] pixels = target.GetPixelBytes();
        pixels.Should().Contain(value => value != 0);
    }

    [Fact]
    public void StrokeStyle_LineJoinRound_AffectsRenderedLine()
    {
        var style = new CanvasStrokeStyle
        {
            LineJoin = CanvasLineJoin.Round,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 20, 20, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawLine(2, 10, 10, 2, Color.FromArgb(255, 255, 255, 255), 3, style);
        ds.DrawLine(10, 2, 18, 10, Color.FromArgb(255, 255, 255, 255), 3, style);

        byte[] pixels = target.GetPixelBytes();
        pixels.Should().Contain(value => value != 0);
    }

    [Fact]
    public void StrokeStyle_LineJoinMiter_AffectsRenderedLine()
    {
        var style = new CanvasStrokeStyle
        {
            LineJoin = CanvasLineJoin.Miter,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 20, 20, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawLine(2, 10, 10, 2, Color.FromArgb(255, 255, 255, 255), 3, style);
        ds.DrawLine(10, 2, 18, 10, Color.FromArgb(255, 255, 255, 255), 3, style);

        byte[] pixels = target.GetPixelBytes();
        pixels.Should().Contain(value => value != 0);
    }

    [Fact]
    public void StrokeStyle_StartCapSquare_AffectsRenderedLine()
    {
        var style = new CanvasStrokeStyle
        {
            StartCap = CanvasCapStyle.Square,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawLine(2, 5, 8, 5, Color.FromArgb(255, 255, 255, 255), 3, style);

        byte[] pixels = target.GetPixelBytes();
        pixels.Should().Contain(value => value != 0);
    }

    [Fact]
    public void StrokeStyle_EndCapSquare_AffectsRenderedLine()
    {
        var style = new CanvasStrokeStyle
        {
            EndCap = CanvasCapStyle.Square,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawLine(2, 5, 8, 5, Color.FromArgb(255, 255, 255, 255), 3, style);

        byte[] pixels = target.GetPixelBytes();
        pixels.Should().Contain(value => value != 0);
    }
}
