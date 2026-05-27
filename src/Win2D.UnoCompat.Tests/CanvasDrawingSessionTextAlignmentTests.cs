using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using SkiaSharp;
using Windows.UI;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasDrawingSessionTextAlignmentTests
{
    [Fact]
    public void DrawText_PointCenteredAlignment_DrawsAroundRequestedPoint()
    {
        using SKBitmap bitmap = new(300, 200);
        using SKCanvas canvas = new(bitmap);
        using CanvasDrawingSession ds = new(canvas);

        ds.Clear(Color.FromArgb(255, 0, 0, 0));
        CanvasTextFormat format = new()
        {
            FontFamily = "file://Assets/OpenSans-Regular.ttf",
            FontSize = 60,
            HorizontalAlignment = CanvasHorizontalAlignment.Center,
            VerticalAlignment = CanvasVerticalAlignment.Center
        };

        const int cx = 150;
        const int cy = 100;
        ds.DrawText("A", cx, cy, Color.FromArgb(255, 255, 255, 255), format);

        FindInkBounds(bitmap, out int minX, out int maxX, out int minY, out int maxY);

        minX.Should().BeLessThan(cx);
        maxX.Should().BeGreaterThan(cx);
        minY.Should().BeLessThan(cy);
        maxY.Should().BeGreaterThan(cy);

        float centerX = (minX + maxX) / 2f;
        float centerY = (minY + maxY) / 2f;
        centerX.Should().BeApproximately(cx, 8f);
        centerY.Should().BeApproximately(cy, 8f);
    }

    private static void FindInkBounds(SKBitmap bitmap, out int minX, out int maxX, out int minY, out int maxY)
    {
        minX = bitmap.Width;
        minY = bitmap.Height;
        maxX = -1;
        maxY = -1;

        for (int y = 0; y < bitmap.Height; y++)
        for (int x = 0; x < bitmap.Width; x++)
        {
            SKColor c = bitmap.GetPixel(x, y);
            if (c.Red == 0 && c.Green == 0 && c.Blue == 0)
                continue;

            if (x < minX) minX = x;
            if (x > maxX) maxX = x;
            if (y < minY) minY = y;
            if (y > maxY) maxY = y;
        }
    }
}
