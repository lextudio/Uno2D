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

    [Fact]
    public void RequestedSize_AndMinimumSize_AreReported()
    {
        var format = new CanvasTextFormat { FontFamily = "Consolas", FontSize = 16 };
        using var layout = new CanvasTextLayout(CanvasDevice.GetSharedDevice(), "size", format, 123, 45);

        layout.RequestedSize.Width.Should().Be(123);
        layout.RequestedSize.Height.Should().Be(45);
        layout.MinimumSize.Width.Should().BeGreaterThan(0);
        layout.MinimumSize.Height.Should().BeGreaterThan(0);
    }

    [Fact]
    public void ClusterMetrics_ReportOneClusterPerCharacter()
    {
        using var layout = CreateLayout("a b");

        var clusters = layout.ClusterMetrics;

        clusters.Should().HaveCount(3);
        clusters[0].CharacterIndex.Should().Be(0);
        clusters[0].CharacterCount.Should().Be(1);
        clusters[0].Width.Should().BeGreaterThan(0);
        clusters[1].IsWhitespace.Should().BeTrue();
        clusters[1].CanWrapLineAfter.Should().BeTrue();
    }

    [Fact]
    public void RangeProperties_ReturnOverridesInsideRange()
    {
        using var layout = CreateLayout("abcdef", 16);
        object effect = new();

        layout.SetFontSize(1, 3, 24);
        layout.SetLocale(2, 2, "fr-FR");
        layout.SetUnderline(0, 2, true);
        layout.SetStrikethrough(4, 2, true);
        layout.SetDrawingEffect(3, 1, effect);

        layout.GetFontSize(0).Should().Be(16);
        layout.GetFontSize(2).Should().Be(24);
        layout.GetLocale(1).Should().BeEmpty();
        layout.GetLocale(2).Should().Be("fr-FR");
        layout.GetUnderline(1).Should().BeTrue();
        layout.GetUnderline(2).Should().BeFalse();
        layout.GetStrikethrough(3).Should().BeFalse();
        layout.GetStrikethrough(4).Should().BeTrue();
        layout.GetDrawingEffect(3).Should().BeSameAs(effect);
    }

    [Fact]
    public void DrawToBitmap_WritesTextPixels()
    {
        using var layout = CreateLayout("A", 24);
        byte[] pixels = new byte[64 * 64 * 4];

        layout.DrawToBitmap(pixels, 2, 2, 64, 64);

        pixels.Should().Contain(value => value != 0);
    }
}
