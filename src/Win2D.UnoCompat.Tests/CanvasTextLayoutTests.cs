using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasTextLayoutTests
{
    private static CanvasTextLayout CreateLayout(string text, float fontSize = 16f)
    {
        var format = new CanvasTextFormat { FontFamily = "Consolas", FontSize = fontSize };
        return new CanvasTextLayout(CanvasDevice.GetSharedDevice(), text, format, float.PositiveInfinity, float.PositiveInfinity);
    }

    [Fact]
    public void GetCaretPosition_IsMonotonicAndStartsAtZero()
    {
        using var layout = CreateLayout("Hello, world");

        layout.GetCaretPosition(0, false).X.Should().Be(0f);

        float prev = -1f;
        for (int i = 0; i <= "Hello, world".Length; i++)
        {
            float x = layout.GetCaretPosition(i, false).X;
            x.Should().BeGreaterThanOrEqualTo(prev, "caret X must not move backward as the index grows");
            prev = x;
        }
    }

    [Fact]
    public void TrailingCaret_EqualsLeadingCaretOfNextCharacter()
    {
        using var layout = CreateLayout("abc");

        layout.GetCaretPosition(0, true).X.Should().Be(layout.GetCaretPosition(1, false).X);
        layout.GetCaretPosition(1, true).X.Should().Be(layout.GetCaretPosition(2, false).X);
    }

    [Fact]
    public void LayoutBounds_WidthEqualsCaretPositionAfterLastCharacter()
    {
        using var layout = CreateLayout("function");

        float endCaret = layout.GetCaretPosition("function".Length, false).X;
        ((float)layout.LayoutBounds.Width).Should().BeApproximately(endCaret, 0.01f);
        layout.LayoutBounds.Height.Should().BeGreaterThan(0);
    }

    [Fact]
    public void HitTest_RoundTripsWithCaretPosition()
    {
        using var layout = CreateLayout("indexer");

        for (int i = 0; i < "indexer".Length; i++)
        {
            float left = layout.GetCaretPosition(i, false).X;
            float right = layout.GetCaretPosition(i, true).X;
            float mid = (left + right) / 2f;

            var region = layout.HitTest(mid, 1f);
            region.CharacterIndex.Should().Be(i);
        }
    }

    [Fact]
    public void HitTest_BeyondEnd_ReturnsLastCharacter()
    {
        using var layout = CreateLayout("xyz");

        var region = layout.HitTest(100000f, 1f);
        region.CharacterIndex.Should().Be(2);
    }

    [Fact]
    public void EmptyText_HasZeroWidthAndPositiveHeight()
    {
        using var layout = CreateLayout(string.Empty);

        ((float)layout.LayoutBounds.Width).Should().Be(0f);
        layout.LayoutBounds.Height.Should().BeGreaterThan(0);
        layout.GetCaretPosition(0, false).X.Should().Be(0f);
    }

    [Fact]
    public void LineMetrics_ReportsCharacterCountAndBaseline()
    {
        using var layout = CreateLayout("trailing  ");

        var metrics = layout.LineMetrics;
        metrics.Should().HaveCount(1);
        metrics[0].CharacterCount.Should().Be("trailing  ".Length);
        metrics[0].TrailingWhitespaceCount.Should().Be(2);
        metrics[0].Baseline.Should().BeGreaterThan(0);
    }
}
