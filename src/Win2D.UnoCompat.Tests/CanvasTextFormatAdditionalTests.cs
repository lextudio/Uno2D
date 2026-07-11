using FluentAssertions;
using Microsoft.Graphics.Canvas.Text;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasTextFormatAdditionalTests
{
    [Fact]
    public void FontWeight_DefaultIsNormal()
    {
        var format = new CanvasTextFormat();
        format.FontWeight.Weight.Should().Be(400);
    }

    [Fact]
    public void FontWeight_Bold_ResolvesTypeface()
    {
        var format = new CanvasTextFormat
        {
            FontFamily = "Consolas",
            FontSize = 16,
            FontWeight = CanvasFontWeight.Bold,
        };

        var typeface = format.ResolveTypeface();
        typeface.Should().NotBeNull();
    }

    [Fact]
    public void FontStretch_Condensed_ResolvesTypeface()
    {
        var format = new CanvasTextFormat
        {
            FontFamily = "Consolas",
            FontSize = 16,
            FontStretch = CanvasFontStretch.Condensed,
        };

        var typeface = format.ResolveTypeface();
        typeface.Should().NotBeNull();
    }

    [Fact]
    public void FontStyle_Italic_ResolvesTypeface()
    {
        var format = new CanvasTextFormat
        {
            FontFamily = "Consolas",
            FontSize = 16,
            FontStyle = CanvasFontStyle.Italic,
        };

        var typeface = format.ResolveTypeface();
        typeface.Should().NotBeNull();
    }

    [Fact]
    public void FontStyle_Oblique_ResolvesTypeface()
    {
        var format = new CanvasTextFormat
        {
            FontFamily = "Consolas",
            FontSize = 16,
            FontStyle = CanvasFontStyle.Oblique,
        };

        var typeface = format.ResolveTypeface();
        typeface.Should().NotBeNull();
    }

    [Fact]
    public void Locale_DoesNotAffectTypefaceResolution()
    {
        var format = new CanvasTextFormat
        {
            FontFamily = "Consolas",
            FontSize = 16,
            Locale = "en-US",
        };

        var typeface = format.ResolveTypeface();
        typeface.Should().NotBeNull();
    }

    [Fact]
    public void Direction_DefaultIsLeftToRight()
    {
        var format = new CanvasTextFormat();
        format.Direction.Should().Be(CanvasTextDirection.LeftToRight);
    }

    [Fact]
    public void Direction_RightToLeft_DoesNotThrow()
    {
        var format = new CanvasTextFormat
        {
            FontFamily = "Consolas",
            FontSize = 16,
            Direction = CanvasTextDirection.RightToLeft,
        };

        var typeface = format.ResolveTypeface();
        typeface.Should().NotBeNull();
    }

    [Fact]
    public void ParagraphAlignment_DefaultIsNear()
    {
        var format = new CanvasTextFormat();
        format.ParagraphAlignment.Should().Be(CanvasParagraphAlignment.Near);
    }

    [Fact]
    public void WordWrapping_DefaultIsNoWrap()
    {
        var format = new CanvasTextFormat();
        format.WordWrapping.Should().Be(CanvasWordWrapping.NoWrap);
    }

    [Fact]
    public void OpticalAlignment_DefaultIsFalse()
    {
        var format = new CanvasTextFormat();
        format.OpticalAlignment.Should().BeFalse();
    }

    [Fact]
    public void TrimmingDelimiterCount_DefaultIsOne()
    {
        var format = new CanvasTextFormat();
        format.TrimmingDelimiterCount.Should().Be(1);
    }

    [Fact]
    public void FontStretch_AllValues_ResolveTypeface()
    {
        var stretches = new[]
        {
            CanvasFontStretch.UltraCondensed,
            CanvasFontStretch.ExtraCondensed,
            CanvasFontStretch.Condensed,
            CanvasFontStretch.SemiCondensed,
            CanvasFontStretch.Normal,
            CanvasFontStretch.SemiExpanded,
            CanvasFontStretch.Expanded,
            CanvasFontStretch.ExtraExpanded,
            CanvasFontStretch.UltraExpanded,
        };

        foreach (var stretch in stretches)
        {
            var format = new CanvasTextFormat
            {
                FontFamily = "Consolas",
                FontSize = 16,
                FontStretch = stretch,
            };

            var typeface = format.ResolveTypeface();
            typeface.Should().NotBeNull($"FontStretch {stretch} should resolve");
        }
    }

    [Fact]
    public void FontWeight_AllPresets_ResolveTypeface()
    {
        var weights = new[]
        {
            CanvasFontWeight.Thin,
            CanvasFontWeight.ExtraLight,
            CanvasFontWeight.Light,
            CanvasFontWeight.Normal,
            CanvasFontWeight.Medium,
            CanvasFontWeight.SemiBold,
            CanvasFontWeight.Bold,
            CanvasFontWeight.ExtraBold,
            CanvasFontWeight.Black,
        };

        foreach (var weight in weights)
        {
            var format = new CanvasTextFormat
            {
                FontFamily = "Consolas",
                FontSize = 16,
                FontWeight = weight,
            };

            var typeface = format.ResolveTypeface();
            typeface.Should().NotBeNull($"FontWeight {weight.Weight} should resolve");
        }
    }

    [Fact]
    public void OpticalAlignment_DoesNotThrow()
    {
        var format = new CanvasTextFormat
        {
            FontFamily = "Consolas",
            FontSize = 16,
            OpticalAlignment = true,
        };

        var typeface = format.ResolveTypeface();
        typeface.Should().NotBeNull();
    }



    [Fact]
    public void TrimmingDelimiterCount_Multiple_TrimsCorrectly()
    {
        var format = new CanvasTextFormat
        {
            FontFamily = "Consolas",
            FontSize = 16,
            TrimmingGranularity = CanvasTrimmingGranularity.Character,
            TrimmingDelimiter = "..",
            TrimmingDelimiterCount = 2,
        };

        string trimmed = format.ApplyTrimming("abcdefghij", 50);

        trimmed.Should().EndWith("....");
    }
}
