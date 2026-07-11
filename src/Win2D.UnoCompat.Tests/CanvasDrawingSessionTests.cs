using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using Windows.Foundation;
using Windows.UI;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasDrawingSessionTests
{
    [Fact]
    public void FillCircle_DrawsPixels()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.FillCircle(5, 5, 3, Color.FromArgb(255, 255, 0, 0));

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void FillCircle_WithBrush_DrawsPixels()
    {
        using var brush = new CanvasSolidColorBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 0, 255, 0));
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.FillCircle(5, 5, 3, brush);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawCircle_DrawsPixels()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawCircle(5, 5, 3, Color.FromArgb(255, 0, 255, 0), 1);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void FillEllipse_DrawsPixels()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.FillEllipse(5, 5, 4, 3, Color.FromArgb(255, 0, 0, 255));

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawEllipse_DrawsPixels()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawEllipse(5, 5, 4, 3, Color.FromArgb(255, 0, 255, 0), 1);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void FillEllipse_WithRect_DrawsPixels()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.FillEllipse(new Rect(1, 1, 8, 8), Color.FromArgb(255, 0, 0, 255));

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void FillEllipse_WithBrush_DrawsPixels()
    {
        using var brush = new CanvasSolidColorBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 0, 255, 0));
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.FillEllipse(new Rect(1, 1, 8, 8), brush);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawEllipse_WithRect_DrawsPixels()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawEllipse(new Rect(1, 1, 8, 8), Color.FromArgb(255, 0, 0, 255), 1);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawRectangle_DrawsPixels()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawRectangle(1, 1, 8, 8, Color.FromArgb(255, 255, 0, 0), 1);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void FillRectangle_WithBrushFloat_DrawsPixels()
    {
        using var brush = new CanvasSolidColorBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 0, 0, 255));
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.FillRectangle(1, 1, 8, 8, brush);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawRoundedRectangle_DrawsPixels()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawRoundedRectangle(1, 1, 8, 8, 2, 2, Color.FromArgb(255, 255, 0, 0), 1);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void FillRoundedRectangle_DrawsPixels()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.FillRoundedRectangle(1, 1, 8, 8, 2, 2, Color.FromArgb(255, 0, 0, 255));

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void FillGeometry_WithColor_DrawsPixels()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(1, 1, 8, 8));
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.FillGeometry(geometry, Color.FromArgb(255, 0, 255, 0));

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawGeometry_DrawsPixels()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(1, 1, 8, 8));
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawGeometry(geometry, Color.FromArgb(255, 255, 0, 0), 1);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawText_WithRectAndColor_DrawsPixels()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 50, 20, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        var format = new CanvasTextFormat { FontFamily = "Consolas", FontSize = 12 };

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawText("Hello", new Rect(0, 0, 50, 20), Color.FromArgb(255, 255, 255, 255), format);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawTextLayout_DrawsPixels()
    {
        var format = new CanvasTextFormat { FontFamily = "file://Assets/OpenSans-Regular.ttf", FontSize = 24 };
        using var layout = new CanvasTextLayout(CanvasDevice.GetSharedDevice(), "AAA", format, 100, 40);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 100, 40, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawTextLayout(layout, 5, 30, Color.FromArgb(255, 255, 255, 255));

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawImage_WithPosition_DrawsPixels()
    {
        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [Color.FromArgb(255, 0, 0, 255)],
            1, 1);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 3, 3, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawImage(bitmap, 1, 1);

        byte[] pixels = target.GetPixelBytes();
        int offset = ((1 * 3) + 1) * 4;
        pixels[offset + 0].Should().Be(255);
        pixels[offset + 1].Should().Be(0);
        pixels[offset + 2].Should().Be(0);
        pixels[offset + 3].Should().Be(255);
    }

    [Fact]
    public void DrawImage_WithSourceRect_DrawsPixels()
    {
        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [Color.FromArgb(255, 0, 0, 255)],
            1, 1);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 3, 3, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawImage(bitmap, new Rect(1, 1, 1, 1), new Rect(0, 0, 1, 1));

        byte[] pixels = target.GetPixelBytes();
        int offset = ((1 * 3) + 1) * 4;
        pixels[offset + 0].Should().Be(255);
        pixels[offset + 1].Should().Be(0);
        pixels[offset + 2].Should().Be(0);
        pixels[offset + 3].Should().Be(255);
    }

    [Fact]
    public void DrawImage_WithTint_DrawsPixels()
    {
        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [Color.FromArgb(255, 255, 255, 255)],
            1, 1);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 3, 3, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawImage(bitmap, new Rect(1, 1, 1, 1), new Rect(0, 0, 1, 1), Color.FromArgb(255, 255, 0, 0));

        byte[] pixels = target.GetPixelBytes();
        int offset = ((1 * 3) + 1) * 4;
        pixels[offset + 2].Should().Be((byte)255);
    }

    [Fact]
    public void DrawImage_WithEffectAndSourceRect_DrawsPixels()
    {
        using CanvasBitmap source = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            [Color.FromArgb(255, 100, 100, 100)],
            1, 1);
        var effect = new Microsoft.Graphics.Canvas.Effects.OpacityEffect { Source = source, Opacity = 0.5f };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 3, 3, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawImage(effect, new Rect(0, 0, 3, 3), new Rect(0, 0, 1, 1));

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void CreateLayer_AppliesOpacity()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        using (CanvasLayer layer = ds.CreateLayer(0.5f))
        {
            ds.FillRectangle(0, 0, 10, 10, Color.FromArgb(255, 255, 255, 255));
        }

        byte[] pixels = target.GetPixelBytes();
        pixels[3].Should().BeInRange((byte)126, (byte)128);
    }

    [Fact]
    public void Transform_SetAndGet_ReturnsSameValue()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        var expected = System.Numerics.Matrix3x2.CreateTranslation(5, 10);

        ds.Transform = expected;
        System.Numerics.Matrix3x2 actual = ds.Transform;

        actual.Translation.X.Should().Be(5);
        actual.Translation.Y.Should().Be(10);
    }

    [Fact]
    public void Antialiasing_SetAndGet_ReturnsSameValue()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Antialiasing = CanvasAntialiasing.Aliased;
        ds.Antialiasing.Should().Be(CanvasAntialiasing.Aliased);

        ds.Antialiasing = CanvasAntialiasing.Antialiased;
        ds.Antialiasing.Should().Be(CanvasAntialiasing.Antialiased);
    }

    [Fact]
    public void TextAntialiasing_SetAndGet_ReturnsSameValue()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.TextAntialiasing = CanvasTextAntialiasing.Aliased;
        ds.TextAntialiasing.Should().Be(CanvasTextAntialiasing.Aliased);

        ds.TextAntialiasing = CanvasTextAntialiasing.Grayscale;
        ds.TextAntialiasing.Should().Be(CanvasTextAntialiasing.Grayscale);
    }

    [Fact]
    public void Blend_SetAndGet_ReturnsSameValue()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Blend = CanvasBlend.Add;
        ds.Blend.Should().Be(CanvasBlend.Add);

        ds.Blend = CanvasBlend.Copy;
        ds.Blend.Should().Be(CanvasBlend.Copy);
    }

    [Fact]
    public void Units_SetAndGet_ReturnsSameValue()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Units = CanvasUnits.Dips;
        ds.Units.Should().Be(CanvasUnits.Dips);
    }

    [Fact]
    public void Flush_DoesNotThrow()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.Flush();
    }

    [Fact]
    public void DrawText_WithRectAndBrush_DrawsPixels()
    {
        using var brush = new CanvasSolidColorBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 255, 255, 255));
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 50, 20, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        var format = new CanvasTextFormat { FontFamily = "Consolas", FontSize = 12 };

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawText("Hi", new Rect(0, 0, 50, 20), brush, format);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawEllipse_WithStrokeStyle_DrawsPixels()
    {
        var style = new CanvasStrokeStyle { StartCap = CanvasCapStyle.Round, EndCap = CanvasCapStyle.Round };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawEllipse(5, 5, 4, 3, Color.FromArgb(255, 0, 255, 0), 1, style);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawEllipse_WithRectAndStrokeStyle_DrawsPixels()
    {
        var style = new CanvasStrokeStyle { LineJoin = CanvasLineJoin.Round };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawEllipse(new Rect(1, 1, 8, 8), Color.FromArgb(255, 0, 0, 255), 1, style);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawRectangle_WithStrokeStyle_DrawsPixels()
    {
        var style = new CanvasStrokeStyle { DashStyle = CanvasDashStyle.Dash };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawRectangle(1, 1, 8, 8, Color.FromArgb(255, 255, 0, 0), 1, style);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawRoundedRectangle_WithStrokeStyle_DrawsPixels()
    {
        var style = new CanvasStrokeStyle { LineJoin = CanvasLineJoin.Round };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawRoundedRectangle(1, 1, 8, 8, 2, 2, Color.FromArgb(255, 255, 0, 0), 1, style);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

}
