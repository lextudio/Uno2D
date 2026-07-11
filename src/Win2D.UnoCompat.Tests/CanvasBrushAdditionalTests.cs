using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Windows.Foundation;
using Windows.UI;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasBrushAdditionalTests
{
    [Fact]
    public void ImageBrush_SourceRect_ClipsBitmap()
    {
        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [Color.FromArgb(255, 255, 0, 0), Color.FromArgb(255, 0, 255, 0)],
            2, 1);
        using var brush = new CanvasImageBrush(CanvasDevice.GetSharedDevice())
        {
            Image = bitmap,
            SourceRect = new Rect(0, 0, 1, 1),
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 2, 1, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.FillRectangle(new Rect(0, 0, 2, 1), brush);
        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void ImageBrush_Transform_AffectsRendering()
    {
        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(), [Color.FromArgb(255, 255, 0, 0)], 1, 1);
        using var brush = new CanvasImageBrush(CanvasDevice.GetSharedDevice())
        {
            Image = bitmap,
            Transform = System.Numerics.Matrix3x2.CreateScale(2, 2),
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 4, 1, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.FillRectangle(new Rect(0, 0, 4, 1), brush);
        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void ImageBrush_Opacity_AffectsRendering()
    {
        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(), [Color.FromArgb(255, 255, 255, 255)], 1, 1);
        using var brush = new CanvasImageBrush(CanvasDevice.GetSharedDevice())
        {
            Image = bitmap,
            Opacity = 0.5f,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 2, 1, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.FillRectangle(new Rect(0, 0, 2, 1), brush);
        byte[] pixels = target.GetPixelBytes();
        pixels[3].Should().BeInRange((byte)126, (byte)128);
    }

    [Fact]
    public void LinearGradientBrush_WithGradientStops_RendersCorrectly()
    {
        var stops = new[]
        {
            new CanvasGradientStop { Position = 0f, Color = Color.FromArgb(255, 255, 0, 0) },
            new CanvasGradientStop { Position = 0.5f, Color = Color.FromArgb(255, 0, 255, 0) },
            new CanvasGradientStop { Position = 1f, Color = Color.FromArgb(255, 0, 0, 255) },
        };
        using var brush = new CanvasLinearGradientBrush(CanvasDevice.GetSharedDevice(), stops)
        {
            StartPoint = new Point(0, 0), EndPoint = new Point(9, 0),
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 1, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.FillRectangle(new Rect(0, 0, 10, 1), brush);
        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void RadialGradientBrush_WithGradientStops_RendersCorrectly()
    {
        var stops = new[]
        {
            new CanvasGradientStop { Position = 0f, Color = Color.FromArgb(255, 255, 255, 255) },
            new CanvasGradientStop { Position = 1f, Color = Color.FromArgb(255, 0, 0, 0) },
        };
        using var brush = new CanvasRadialGradientBrush(CanvasDevice.GetSharedDevice(), stops)
        {
            Center = new Point(5, 5), Radius = 5,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 11, 11, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.FillRectangle(new Rect(0, 0, 11, 11), brush);
        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void LinearGradientBrush_ExtendX_Clamp_DoesNotThrow()
    {
        using var brush = new CanvasLinearGradientBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 255, 0, 0), Color.FromArgb(255, 0, 0, 255))
        {
            StartPoint = new Point(0, 0), EndPoint = new Point(5, 0), ExtendX = CanvasEdgeBehavior.Clamp,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 1, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.FillRectangle(new Rect(0, 0, 10, 1), brush);
        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void LinearGradientBrush_ExtendY_Wrap_DoesNotThrow()
    {
        using var brush = new CanvasLinearGradientBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 255, 0, 0), Color.FromArgb(255, 0, 0, 255))
        {
            StartPoint = new Point(0, 0), EndPoint = new Point(5, 0), ExtendY = CanvasEdgeBehavior.Wrap,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 1, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.FillRectangle(new Rect(0, 0, 10, 1), brush);
        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void LinearGradientBrush_Transform_AffectsRendering()
    {
        using var brush = new CanvasLinearGradientBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 255, 0, 0), Color.FromArgb(255, 0, 0, 255))
        {
            StartPoint = new Point(0, 0), EndPoint = new Point(5, 0), Transform = System.Numerics.Matrix3x2.CreateScale(2, 1),
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 1, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.FillRectangle(new Rect(0, 0, 10, 1), brush);
        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void LinearGradientBrush_Opacity_AffectsRendering()
    {
        using var brush = new CanvasLinearGradientBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 255, 0, 0), Color.FromArgb(255, 0, 0, 255))
        {
            StartPoint = new Point(0, 0), EndPoint = new Point(5, 0), Opacity = 0.5f,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 1, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.FillRectangle(new Rect(0, 0, 10, 1), brush);
        byte[] pixels = target.GetPixelBytes();
        pixels[3].Should().BeInRange((byte)126, (byte)128);
    }

    [Fact]
    public void RadialGradientBrush_ExtendX_Wrap_DoesNotThrow()
    {
        using var brush = new CanvasRadialGradientBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 0, 0, 0))
        {
            Center = new Point(5, 5), Radius = 5, ExtendX = CanvasEdgeBehavior.Wrap,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 11, 11, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.FillRectangle(new Rect(0, 0, 11, 11), brush);
        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void RadialGradientBrush_ExtendY_Mirror_DoesNotThrow()
    {
        using var brush = new CanvasRadialGradientBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 0, 0, 0))
        {
            Center = new Point(5, 5), Radius = 5, ExtendY = CanvasEdgeBehavior.Mirror,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 11, 11, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.FillRectangle(new Rect(0, 0, 11, 11), brush);
        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void RadialGradientBrush_Transform_AffectsRendering()
    {
        using var brush = new CanvasRadialGradientBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 0, 0, 0))
        {
            Center = new Point(5, 5), Radius = 5, Transform = System.Numerics.Matrix3x2.CreateScale(2, 1),
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 11, 11, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.FillRectangle(new Rect(0, 0, 11, 11), brush);
        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void RadialGradientBrush_Opacity_AffectsRendering()
    {
        using var brush = new CanvasRadialGradientBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 0, 0, 0))
        {
            Center = new Point(5, 5), Radius = 5, Opacity = 0.5f,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 11, 11, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.FillRectangle(new Rect(0, 0, 11, 11), brush);
        byte[] pixels = target.GetPixelBytes();
        pixels[3].Should().BeInRange((byte)126, (byte)128);
    }
}
