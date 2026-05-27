using FluentAssertions;
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
}
