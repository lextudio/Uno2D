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

    [Fact]
    public void DrawEllipse_Rect_WithBrush_DrawsPixels()
    {
        using var brush = new CanvasSolidColorBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 0, 255, 0));
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawEllipse(new Rect(1, 1, 6, 6), brush);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawCachedGeometry_WithOffset_DrawsPixels()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(0, 0, 3, 3));
        using var cached = new CanvasCachedGeometry(geometry);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawCachedGeometry(cached, 2f, 2f, Color.FromArgb(255, 255, 0, 0));

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void CreateLayer_WithGeometryClip_RestoresState()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(1, 1, 5, 5));
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        using (ds.CreateLayer(1f, geometry))
        {
            ds.FillRectangle(0, 0, 10, 10, Color.FromArgb(255, 255, 0, 0));
        }
        ds.FillRectangle(6, 6, 2, 2, Color.FromArgb(255, 0, 255, 0));

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawInk_DrawsPixels()
    {
        var stroke = new Microsoft.Graphics.Canvas.Ink.CanvasInkStroke(
            new[] { new System.Numerics.Vector2(1, 1), new System.Numerics.Vector2(8, 8) },
            Color.FromArgb(255, 0, 0, 255),
            2f);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawInk(new[] { stroke });

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawImage_Effect_AtPoint_DrawsPixels()
    {
        using CanvasBitmap source = CanvasBitmap.CreateFromColors(CanvasDevice.GetSharedDevice(), [Color.FromArgb(255, 10, 20, 30)], 1, 1);
        var effect = new Microsoft.Graphics.Canvas.Effects.ColorMatrixEffect
        {
            Source = source,
            ColorMatrix = [1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0, 0, 1, 0],
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 4, 4, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawImage(effect, 1f, 1f);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawImage_WithDestinationQuad_DrawsPixels()
    {
        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(CanvasDevice.GetSharedDevice(), [Color.FromArgb(255, 255, 0, 0)], 1, 1);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 10, 10, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        var quad = new[]
        {
            new System.Numerics.Vector2(1, 1),
            new System.Numerics.Vector2(9, 1),
            new System.Numerics.Vector2(9, 9),
            new System.Numerics.Vector2(1, 9),
        };

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawImage(bitmap, quad, new Rect(0, 0, 1, 1));

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawText_Rect_NoFormat_DrawsPixels()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 50, 30, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawText("Hi", new Rect(0, 0, 50, 30), Color.FromArgb(255, 255, 255, 255));

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawText_Point_WithBrush_NoFormat_DrawsPixels()
    {
        using var brush = new CanvasSolidColorBrush(CanvasDevice.GetSharedDevice(), Color.FromArgb(255, 255, 255, 255));
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 50, 30, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawText("Hi", 5, 20, brush);

        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void DrawImage_WithDestinationQuad_Trapezoid_HonorsAllFourCorners()
    {
        // An 8x8 source bitmap with a distinct solid color filling each quadrant, so bilinear
        // sampling near (but not at) the quadrant centers is unaffected by neighboring colors.
        Color topLeftColor = Color.FromArgb(255, 255, 0, 0);
        Color bottomRightColor = Color.FromArgb(255, 255, 255, 0);
        var colors = new Color[64];
        for (int y = 0; y < 8; y++)
        {
            for (int x = 0; x < 8; x++)
            {
                bool right = x >= 4;
                bool bottom = y >= 4;
                colors[(y * 8) + x] = (right, bottom) switch
                {
                    (false, false) => topLeftColor,
                    (true, false) => Color.FromArgb(255, 0, 255, 0),
                    (false, true) => Color.FromArgb(255, 0, 0, 255),
                    (true, true) => bottomRightColor,
                };
            }
        }
        using CanvasBitmap bitmap = CanvasBitmap.CreateFromColors(CanvasDevice.GetSharedDevice(), colors, 8, 8);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 20, 20, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        // A trapezoid (non-parallelogram): top edge narrower than bottom edge.
        // An affine fit using only 3 corners (ignoring the bottom-right point) would place
        // the bottom-right destination corner incorrectly; a true projective transform honors it.
        var quad = new[]
        {
            new System.Numerics.Vector2(5, 2),   // top-left
            new System.Numerics.Vector2(15, 2),  // top-right
            new System.Numerics.Vector2(19, 18), // bottom-right
            new System.Numerics.Vector2(1, 18),  // bottom-left
        };

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawImage(bitmap, quad, new Rect(0, 0, 8, 8));

        Color[] pixels = target.GetPixelColors();
        Color nearBottomRight = pixels[(15 * 20) + 16];
        Color nearTopLeft = pixels[(4 * 20) + 7];

        nearBottomRight.Should().Be(bottomRightColor);
        nearTopLeft.Should().Be(topLeftColor);
    }

    [Fact]
    public void CanvasRenderTarget_GetPixelColors_Region_ReturnsMatchingPixel()
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 4, 4, 96);
        using (CanvasDrawingSession ds = target.CreateDrawingSession())
        {
            ds.Clear(Color.FromArgb(0, 0, 0, 0));
            ds.FillRectangle(2, 2, 1, 1, Color.FromArgb(255, 10, 20, 30));
        }

        Color[] colors = target.GetPixelColors(2, 2, 1, 1);

        colors.Should().ContainSingle().Which.Should().Be(Color.FromArgb(255, 10, 20, 30));
    }
}
