using System.Numerics;
using System.Text;
using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Ink;
using Microsoft.Graphics.Canvas.Printing;
using Microsoft.Graphics.Canvas.Svg;
using Microsoft.Graphics.Canvas.Typography;
using Windows.Foundation;
using Windows.UI;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasRemainingFeaturesTests
{
    [Fact]
    public void CanvasPrintDocument_RendersPdf()
    {
        var document = new CanvasPrintDocument();
        document.PrintPage += (_, args) =>
        {
            args.PageNumber.Should().Be(1);
            args.DrawingSession.FillRectangle(0, 0, 10, 10, Color.FromArgb(255, 255, 0, 0));
        };
        using var stream = new MemoryStream();

        document.RenderToPdf(stream, 1, 100, 100);

        Encoding.ASCII.GetString(stream.ToArray(), 0, 4).Should().Be("%PDF");
    }

    [Fact]
    public async Task CanvasSvgDocument_LoadsAndDrawsBasicShapes()
    {
        string svg = """<svg width="4" height="4"><rect x="1" y="1" width="2" height="2" fill="#0A141E"/></svg>""";
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(svg));
        CanvasSvgDocument document = await CanvasSvgDocument.LoadAsync(stream);
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 4, 4, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        document.Draw(ds, new Rect(0, 0, 4, 4));

        document.GetSize().Should().Be(new Size(4, 4));
        PixelAt(target.GetPixelBytes(), 4, 1, 1).Should().Be((10, 20, 30, 255));
    }

    [Fact]
    public void CanvasInk_DrawsStroke()
    {
        var builder = new CanvasInkStrokeBuilder { Color = Color.FromArgb(255, 1, 2, 3), Width = 1 };
        builder.AddPoint(new Vector2(0, 0));
        builder.AddPoint(new Vector2(3, 0));
        var ink = new CanvasInk();
        ink.AddStroke(builder.Build());
        using var target = new CanvasRenderTarget(CanvasDevice.GetSharedDevice(), 4, 2, 96);
        using CanvasDrawingSession ds = target.CreateDrawingSession();

        ds.Clear(Color.FromArgb(0, 0, 0, 0));
        ink.Draw(ds);

        target.GetPixelBytes().Should().Contain(value => value != 0);
        ink.Strokes.Should().ContainSingle();
    }

    [Fact]
    public void CanvasTypography_StoresFeatureValues()
    {
        var typography = new CanvasTypography();

        typography.AddFeature(CanvasTypographyFeatureName.Kerning, 1);
        typography.AddFeature(CanvasTypographyFeatureName.StandardLigatures, 0);

        typography.TryGetFeature(CanvasTypographyFeatureName.Kerning, out int kerning).Should().BeTrue();
        kerning.Should().Be(1);
        typography.Features[CanvasTypographyFeatureName.StandardLigatures].Should().Be(0);
    }

    private static (byte R, byte G, byte B, byte A) PixelAt(byte[] pixels, int width, int x, int y)
    {
        int offset = ((y * width) + x) * 4;
        return (pixels[offset + 2], pixels[offset + 1], pixels[offset], pixels[offset + 3]);
    }
}
