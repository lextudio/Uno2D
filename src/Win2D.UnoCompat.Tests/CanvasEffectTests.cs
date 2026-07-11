using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Windows.Foundation;
using Windows.UI;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasEffectTests
{
    [Fact]
    public void ColorMatrixEffect_CanSwapRedAndBlue()
    {
        using CanvasBitmap source = OnePixel(Color.FromArgb(255, 10, 20, 30));
        var effect = new ColorMatrixEffect
        {
            Source = source,
            ColorMatrix =
            [
                0, 0, 1, 0, 0,
                0, 1, 0, 0, 0,
                1, 0, 0, 0, 0,
                0, 0, 0, 1, 0,
            ],
        };

        (byte R, byte G, byte B, byte A) pixel = DrawEffectPixel(effect, 1, 1, 0, 0);

        pixel.Should().Be((30, 20, 10, 255));
    }

    [Fact]
    public void OpacityEffect_ReducesAlpha()
    {
        using CanvasBitmap source = OnePixel(Color.FromArgb(255, 100, 100, 100));
        var effect = new OpacityEffect { Source = source, Opacity = 0.5f };

        (byte R, byte G, byte B, byte A) pixel = DrawEffectPixel(effect, 1, 1, 0, 0);

        pixel.A.Should().BeInRange((byte)126, (byte)128);
    }

    [Fact]
    public void SaturationEffect_ZeroSaturationMakesGray()
    {
        using CanvasBitmap source = OnePixel(Color.FromArgb(255, 255, 0, 0));
        var effect = new SaturationEffect { Source = source, Saturation = 0 };

        (byte R, byte G, byte B, byte A) pixel = DrawEffectPixel(effect, 1, 1, 0, 0);

        pixel.R.Should().Be(pixel.G);
        pixel.G.Should().Be(pixel.B);
    }

    [Fact]
    public void BlendEffect_AddCombinesForegroundAndBackground()
    {
        using CanvasBitmap background = OnePixel(Color.FromArgb(255, 100, 0, 0));
        using CanvasBitmap foreground = OnePixel(Color.FromArgb(255, 0, 50, 0));
        var effect = new BlendEffect
        {
            Background = background,
            Foreground = foreground,
            Mode = CanvasBlend.Add,
        };

        (byte R, byte G, byte B, byte A) pixel = DrawEffectPixel(effect, 1, 1, 0, 0);

        pixel.R.Should().BeGreaterThan(90);
        pixel.G.Should().BeGreaterThan(40);
    }

    [Fact]
    public void CompositeEffect_ComposesMultipleSources()
    {
        using CanvasBitmap first = OnePixel(Color.FromArgb(255, 100, 0, 0));
        using CanvasBitmap second = OnePixel(Color.FromArgb(255, 0, 50, 0));
        var effect = new CompositeEffect { Mode = CanvasComposite.Add };
        effect.Sources.Add(first);
        effect.Sources.Add(second);

        (byte R, byte G, byte B, byte A) pixel = DrawEffectPixel(effect, 1, 1, 0, 0);

        pixel.R.Should().BeGreaterThan(90);
        pixel.G.Should().BeGreaterThan(40);
    }

    [Fact]
    public void GaussianBlurEffect_SpreadsSinglePixel()
    {
        using CanvasBitmap source = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            CreateSingleWhitePixelImage(5, 5, 2, 2),
            5,
            5);
        var effect = new GaussianBlurEffect { Source = source, BlurAmount = 1.5f };

        (byte R, byte G, byte B, byte A) neighbor = DrawEffectPixel(effect, 5, 5, 2, 1);

        neighbor.A.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ShadowEffect_DrawsOffsetShadowOnly()
    {
        using CanvasBitmap source = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(),
            CreateSingleWhitePixelImage(5, 5, 1, 1),
            5,
            5);
        var effect = new ShadowEffect
        {
            Source = source,
            BlurAmount = 0,
            Offset = new System.Numerics.Vector2(2, 2),
            Color = Color.FromArgb(255, 0, 0, 0),
        };

        (byte R, byte G, byte B, byte A) pixel = DrawEffectPixel(effect, 5, 5, 3, 3);

        pixel.A.Should().Be(255);
    }

    private static CanvasBitmap OnePixel(Color color)
        => CanvasBitmap.CreateFromColors(CanvasDevice.GetSharedDevice(), [color], 1, 1);

    private static (byte R, byte G, byte B, byte A) DrawEffectPixel(CanvasEffect effect, int width, int height, int x, int y)
    {
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), width, height, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawImage(effect, new Rect(0, 0, width, height));
        return PixelAt(target.GetPixelBytes(), width, x, y);
    }

    private static Color[] CreateSingleWhitePixelImage(int width, int height, int x, int y)
    {
        var colors = Enumerable.Repeat(Color.FromArgb(0, 0, 0, 0), width * height).ToArray();
        colors[(y * width) + x] = Color.FromArgb(255, 255, 255, 255);
        return colors;
    }

    private static (byte R, byte G, byte B, byte A) PixelAt(byte[] pixels, int width, int x, int y)
    {
        int offset = ((y * width) + x) * 4;
        return (pixels[offset + 2], pixels[offset + 1], pixels[offset], pixels[offset + 3]);
    }
}
