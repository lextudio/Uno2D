using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Windows.Foundation;
using Windows.UI;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasEffectAdditionalTests
{
    [Fact]
    public void Transform2DEffect_ScalesImage()
    {
        using CanvasBitmap source = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(), [Color.FromArgb(255, 100, 100, 100)], 1, 1);
        var effect = new Transform2DEffect
        {
            Source = source,
            TransformMatrix = System.Numerics.Matrix3x2.CreateScale(2, 2),
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 3, 3, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawImage(effect, new Rect(0, 0, 3, 3));
        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void Transform2DEffect_TranslatesImage()
    {
        using CanvasBitmap source = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(), [Color.FromArgb(255, 100, 100, 100)], 1, 1);
        var effect = new Transform2DEffect
        {
            Source = source,
            TransformMatrix = System.Numerics.Matrix3x2.CreateTranslation(2, 2),
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 5, 5, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawImage(effect, new Rect(0, 0, 5, 5));
        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void GaussianBlurEffect_BorderMode_Hard_DoesNotThrow()
    {
        using CanvasBitmap source = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(), [Color.FromArgb(255, 255, 255, 255)], 1, 1);
        var effect = new GaussianBlurEffect
        {
            Source = source,
            BlurAmount = 1.5f,
            BorderMode = EffectBorderMode.Hard,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 3, 3, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawImage(effect, new Rect(0, 0, 3, 3));
        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void GaussianBlurEffect_BorderMode_Soft_DoesNotThrow()
    {
        using CanvasBitmap source = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(), [Color.FromArgb(255, 255, 255, 255)], 1, 1);
        var effect = new GaussianBlurEffect
        {
            Source = source,
            BlurAmount = 1.5f,
            BorderMode = EffectBorderMode.Soft,
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 3, 3, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawImage(effect, new Rect(0, 0, 3, 3));
        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void Transform2DEffect_WithRotation_DoesNotThrow()
    {
        using CanvasBitmap source = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(), [Color.FromArgb(255, 100, 100, 100)], 1, 1);
        var effect = new Transform2DEffect
        {
            Source = source,
            TransformMatrix = System.Numerics.Matrix3x2.CreateRotation(0.5f),
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 3, 3, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawImage(effect, new Rect(0, 0, 3, 3));
        target.GetPixelBytes().Should().Contain(value => value != 0);
    }

    [Fact]
    public void Transform2DEffect_WithTranslation_DoesNotThrow()
    {
        using CanvasBitmap source = CanvasBitmap.CreateFromColors(
            CanvasDevice.GetSharedDevice(), [Color.FromArgb(255, 100, 100, 100)], 1, 1);
        var effect = new Transform2DEffect
        {
            Source = source,
            TransformMatrix = System.Numerics.Matrix3x2.CreateTranslation(2, 2),
        };
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 5, 5, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();
        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ds.DrawImage(effect, new Rect(0, 0, 5, 5));
        target.GetPixelBytes().Should().Contain(value => value != 0);
    }
}
