using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasTextLayoutAdditionalTests
{
    [Fact]
    public void Text_ReturnsOriginalString()
    {
        using var layout = CreateLayout("hello");
        layout.Text.Should().Be("hello");
    }

    [Fact]
    public void Format_ReturnsOriginalFormat()
    {
        var format = new CanvasTextFormat { FontFamily = "Consolas", FontSize = 16 };
        using var layout = new CanvasTextLayout(CanvasDevice.GetSharedDevice(), "test", format, 100, 50);
        layout.Format.Should().BeSameAs(format);
    }

    [Fact]
    public void MaxWidth_ReturnsOriginalValue()
    {
        using var layout = CreateLayout("test", maxWidth: 100);
        layout.MaxWidth.Should().Be(100);
    }

    [Fact]
    public void MaxHeight_ReturnsOriginalValue()
    {
        using var layout = CreateLayout("test", maxHeight: 50);
        layout.MaxHeight.Should().Be(50);
    }

    [Fact]
    public void LayoutBoundsIncludingTrailingWhitespace_ReturnsBounds()
    {
        using var layout = CreateLayout("hello ");
        var bounds = layout.LayoutBoundsIncludingTrailingWhitespace;
        bounds.Width.Should().BeGreaterThan(0);
        bounds.Height.Should().BeGreaterThan(0);
    }

    [Fact]
    public void DrawBounds_ReturnsLayoutBounds()
    {
        using var layout = CreateLayout("hello");
        layout.DrawBounds.Should().Be(layout.LayoutBounds);
    }

    [Fact]
    public void GetCharacterRegions_ReturnsCorrectRegion()
    {
        using var layout = CreateLayout("hello");
        var regions = layout.GetCharacterRegions(1, 3);
        regions.Should().ContainSingle();
        regions[0].CharacterIndex.Should().Be(1);
        regions[0].CharacterCount.Should().Be(3);
        regions[0].LayoutBounds.Width.Should().BeGreaterThan(0);
    }

    [Fact]
    public void GetCharacterRegions_WithZeroCount_ReturnsEmpty()
    {
        using var layout = CreateLayout("hello");
        var regions = layout.GetCharacterRegions(2, 0);
        regions.Should().BeEmpty();
    }

    [Fact]
    public void GetCharacterRegions_WithOutOfRange_Clamps()
    {
        using var layout = CreateLayout("hi");
        var regions = layout.GetCharacterRegions(0, 10);
        regions.Should().ContainSingle();
        regions[0].CharacterCount.Should().Be(2);
    }

    [Fact]
    public void CreatePath_ReturnsNonEmptyPath()
    {
        using var layout = CreateLayout("A");
        using var path = layout.CreatePath();
        path.Should().NotBeNull();
        path.Bounds.Width.Should().BeGreaterThan(0);
        path.Bounds.Height.Should().BeGreaterThan(0);
    }

    private static CanvasTextLayout CreateLayout(string text, float fontSize = 16f, float maxWidth = float.PositiveInfinity, float maxHeight = float.PositiveInfinity)
    {
        var format = new CanvasTextFormat { FontFamily = "Consolas", FontSize = fontSize };
        return new CanvasTextLayout(CanvasDevice.GetSharedDevice(), text, format, maxWidth, maxHeight);
    }
}
