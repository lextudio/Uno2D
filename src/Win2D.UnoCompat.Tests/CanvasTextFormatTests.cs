using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasTextFormatTests
{
    [Fact]
    public void ResolveTypeface_ForMsAppxFont_ReturnsNonDefaultTypeface()
    {
        CanvasTextFormat format = new()
        {
            FontFamily = "file://Assets/OpenSans-Regular.ttf",
            FontSize = 16
        };

        var typeface = format.ResolveTypeface();

        typeface.Should().NotBeNull();
        typeface.FamilyName.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void ResolveTypeface_UsesCache_ForSameFontFamily()
    {
        CanvasTextFormat format = new()
        {
            FontFamily = "file://Assets/OpenSans-Regular.ttf",
            FontSize = 16
        };

        var first = format.ResolveTypeface();
        var second = format.ResolveTypeface();

        ReferenceEquals(first, second).Should().BeTrue();
    }

    [Fact]
    public void LayoutMetrics_IncludeLetterAndWordSpacing()
    {
        var plain = new CanvasTextLayout(
            CanvasDevice.GetSharedDevice(),
            "a b",
            new CanvasTextFormat { FontFamily = "Consolas", FontSize = 16 },
            float.PositiveInfinity,
            float.PositiveInfinity);
        var spaced = new CanvasTextLayout(
            CanvasDevice.GetSharedDevice(),
            "a b",
            new CanvasTextFormat { FontFamily = "Consolas", FontSize = 16, LetterSpacing = 2, WordSpacing = 5 },
            float.PositiveInfinity,
            float.PositiveInfinity);

        spaced.LayoutBounds.Width.Should().BeApproximately(plain.LayoutBounds.Width + 9, 0.1);
        spaced.GetCaretPosition(3, false).X.Should().BeApproximately((float)spaced.LayoutBounds.Width, 0.1f);
    }

    [Fact]
    public void UniformLineSpacing_OverridesMeasuredLineHeight()
    {
        using var layout = new CanvasTextLayout(
            CanvasDevice.GetSharedDevice(),
            "line",
            new CanvasTextFormat
            {
                FontFamily = "Consolas",
                FontSize = 16,
                LineSpacing = 42,
                LineSpacingMethod = CanvasLineSpacingMethod.Uniform,
            },
            float.PositiveInfinity,
            float.PositiveInfinity);

        layout.LayoutBounds.Height.Should().Be(42);
        layout.LineMetrics[0].Height.Should().Be(42);
    }

    [Fact]
    public void ApplyTrimming_TrimsByCharacterToFitWidth()
    {
        var format = new CanvasTextFormat
        {
            FontFamily = "Consolas",
            FontSize = 16,
            TrimmingGranularity = CanvasTrimmingGranularity.Character,
            TrimmingDelimiter = "...",
        };

        string trimmed = format.ApplyTrimming("abcdefghij", 50);

        trimmed.Should().EndWith("...");
        trimmed.Length.Should().BeLessThan("abcdefghij".Length);
    }

    [Fact]
    public void ApplyTrimming_TrimsByWordBoundary()
    {
        var format = new CanvasTextFormat
        {
            FontFamily = "Consolas",
            FontSize = 16,
            TrimmingGranularity = CanvasTrimmingGranularity.Word,
            TrimmingDelimiter = "...",
        };

        string trimmed = format.ApplyTrimming("alpha beta gamma", 95);

        trimmed.Should().Be("alpha beta...");
    }
}
