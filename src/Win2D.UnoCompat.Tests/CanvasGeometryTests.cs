using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.Text;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasGeometryTests
{
    [Fact]
    public void CreateRectangle_ReturnsCorrectBounds()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(1, 2, 10, 20));

        Rect bounds = geometry.ComputeBounds();
        bounds.X.Should().Be(1);
        bounds.Y.Should().Be(2);
        bounds.Width.Should().Be(10);
        bounds.Height.Should().Be(20);
    }

    [Fact]
    public void CreateRoundedRectangle_ReturnsCorrectBounds()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRoundedRectangle(new Rect(0, 0, 20, 10), 3, 3);

        Rect bounds = geometry.ComputeBounds();
        bounds.Width.Should().Be(20);
        bounds.Height.Should().Be(10);
    }

    [Fact]
    public void CreateEllipse_ReturnsCorrectBounds()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateEllipse(new Rect(0, 0, 20, 10));

        Rect bounds = geometry.ComputeBounds();
        bounds.Width.Should().Be(20);
        bounds.Height.Should().Be(10);
    }

    [Fact]
    public void CreateCircle_ReturnsCorrectBounds()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateCircle(10, 10, 5);

        Rect bounds = geometry.ComputeBounds();
        bounds.Width.Should().Be(10);
        bounds.Height.Should().Be(10);
    }

    [Fact]
    public void CreatePolygon_ReturnsCorrectBounds()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreatePolygon([
            new System.Numerics.Vector2(0, 0),
            new System.Numerics.Vector2(10, 0),
            new System.Numerics.Vector2(5, 10),
        ]);

        Rect bounds = geometry.ComputeBounds();
        bounds.Width.Should().Be(10);
        bounds.Height.Should().Be(10);
    }

    [Fact]
    public void CreateText_ReturnsGeometryWithBounds()
    {
        var format = new CanvasTextFormat { FontFamily = "Consolas", FontSize = 16 };
        using var layout = new CanvasTextLayout(CanvasDevice.GetSharedDevice(), "A", format, float.PositiveInfinity, float.PositiveInfinity);
        using CanvasGeometry geometry = CanvasGeometry.CreateText(layout);

        Rect bounds = geometry.ComputeBounds();
        bounds.Width.Should().BeGreaterThan(0);
        bounds.Height.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Combine_Union_CombinesTwoRectangles()
    {
        using CanvasGeometry a = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 10));
        using CanvasGeometry b = CanvasGeometry.CreateRectangle(new Rect(5, 5, 10, 10));

        using CanvasGeometry combined = a.Combine(b, System.Numerics.Matrix3x2.Identity, CanvasGeometryCombine.Union);

        Rect bounds = combined.ComputeBounds();
        bounds.Width.Should().Be(15);
        bounds.Height.Should().Be(15);
    }

    [Fact]
    public void Combine_Intersect_ReturnsOverlap()
    {
        using CanvasGeometry a = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 10));
        using CanvasGeometry b = CanvasGeometry.CreateRectangle(new Rect(5, 5, 10, 10));

        using CanvasGeometry combined = a.Combine(b, System.Numerics.Matrix3x2.Identity, CanvasGeometryCombine.Intersect);

        Rect bounds = combined.ComputeBounds();
        bounds.Width.Should().Be(5);
        bounds.Height.Should().Be(5);
    }

    [Fact]
    public void Combine_Xor_ReturnsNonOverlappingParts()
    {
        using CanvasGeometry a = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 10));
        using CanvasGeometry b = CanvasGeometry.CreateRectangle(new Rect(5, 5, 10, 10));

        using CanvasGeometry combined = a.Combine(b, System.Numerics.Matrix3x2.Identity, CanvasGeometryCombine.Xor);

        float area = combined.ComputeArea();
        area.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Combine_Exclude_ReturnsDifference()
    {
        using CanvasGeometry a = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 10));
        using CanvasGeometry b = CanvasGeometry.CreateRectangle(new Rect(2, 2, 6, 6));

        using CanvasGeometry combined = a.Combine(b, System.Numerics.Matrix3x2.Identity, CanvasGeometryCombine.Exclude);

        float area = combined.ComputeArea();
        area.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Stroke_ReturnsStrokedGeometry()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 10));

        using CanvasGeometry stroked = geometry.Stroke(2);

        Rect bounds = stroked.ComputeBounds();
        bounds.Width.Should().BeGreaterThan(10);
        bounds.Height.Should().BeGreaterThan(10);
    }

    [Fact]
    public void Stroke_WithStrokeStyle_ReturnsStrokedGeometry()
    {
        var style = new CanvasStrokeStyle { StartCap = CanvasCapStyle.Round, EndCap = CanvasCapStyle.Round };
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 10));

        using CanvasGeometry stroked = geometry.Stroke(2, style);

        Rect bounds = stroked.ComputeBounds();
        bounds.Width.Should().BeGreaterThan(10);
    }

    [Fact]
    public void Outline_ReturnsStrokedGeometry()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 10));

        using CanvasGeometry outlined = geometry.Outline(2);

        Rect bounds = outlined.ComputeBounds();
        bounds.Width.Should().BeGreaterThan(10);
    }

    [Fact]
    public void Simplify_ReturnsSimplifiedGeometry()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 10));

        using CanvasGeometry simplified = geometry.Simplify(CanvasFilledRegionDetermination.Alternate);

        simplified.ComputeArea().Should().BeApproximately(100f, 0.1f);
    }

    [Fact]
    public void ComputeArea_ReturnsCorrectArea()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 20));

        float area = geometry.ComputeArea();

        area.Should().BeApproximately(200f, 0.1f);
    }

    [Fact]
    public void ComputeArea_WithTransform_ReturnsTransformedArea()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 10));

        float area = geometry.ComputeArea(System.Numerics.Matrix3x2.CreateScale(2, 2));

        area.Should().BeApproximately(400f, 0.1f);
    }

    [Fact]
    public void ComputePointOnPath_ReturnsPoint()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 10));

        System.Numerics.Vector2 point = geometry.ComputePointOnPath(0);

        point.X.Should().Be(0);
        point.Y.Should().Be(0);
    }

    [Fact]
    public void FillContainsPoint_ReturnsTrueForInsidePoint()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 10));

        geometry.FillContainsPoint(new System.Numerics.Vector2(5, 5)).Should().BeTrue();
        geometry.FillContainsPoint(new System.Numerics.Vector2(15, 15)).Should().BeFalse();
    }

    [Fact]
    public void CompareWith_ReturnsTrueForOverlappingGeometries()
    {
        using CanvasGeometry a = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 10));
        using CanvasGeometry b = CanvasGeometry.CreateRectangle(new Rect(5, 5, 10, 10));

        a.CompareWith(b).Should().BeTrue();
    }

    [Fact]
    public void CompareWith_ReturnsFalseForNonOverlappingGeometries()
    {
        using CanvasGeometry a = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 10));
        using CanvasGeometry b = CanvasGeometry.CreateRectangle(new Rect(20, 20, 10, 10));

        a.CompareWith(b).Should().BeFalse();
    }

    [Fact]
    public void Transform_TranslatesGeometry()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 10));

        using CanvasGeometry transformed = geometry.Transform(System.Numerics.Matrix3x2.CreateTranslation(5, 5));

        Rect bounds = transformed.ComputeBounds();
        bounds.X.Should().Be(5);
        bounds.Y.Should().Be(5);
    }

    [Fact]
    public void SendPathTo_ReplaysPathVerbs()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(0, 0, 10, 10));
        var receiver = new TestPathReceiver();

        geometry.SendPathTo(receiver);

        receiver.Verbs.Should().ContainInOrder("BeginFigure", "AddLine", "AddLine", "AddLine", "EndFigure");
    }

    [Fact]
    public void ComputeBounds_ReturnsCorrectBounds()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(2, 3, 10, 20));

        Windows.Foundation.Rect bounds = geometry.ComputeBounds();

        bounds.X.Should().Be(2);
        bounds.Y.Should().Be(3);
        bounds.Width.Should().Be(10);
        bounds.Height.Should().Be(20);
    }

    [Fact]
    public void CreatePolygon_WithEmptyPoints_ReturnsEmptyGeometry()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreatePolygon([]);

        geometry.ComputeArea().Should().Be(0);
    }

    [Fact]
    public void CreatePolygon_WithSinglePoint_ReturnsGeometry()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreatePolygon([new System.Numerics.Vector2(5, 5)]);

        geometry.ComputeBounds().Width.Should().Be(0);
    }

    [Fact]
    public void ComputeArea_WithFewerThanThreePoints_ReturnsZero()
    {
        using CanvasGeometry geometry = CanvasGeometry.CreateRectangle(new Rect(0, 0, 0, 0));

        geometry.ComputeArea().Should().Be(0);
    }

    private sealed class TestPathReceiver : ICanvasPathReceiver
    {
        public List<string> Verbs { get; } = new();

        public void BeginFigure(Vector2 startPoint, CanvasFigureFill figureFill) => Verbs.Add("BeginFigure");
        public void AddLine(Vector2 endPoint) => Verbs.Add("AddLine");
        public void AddCubicBezier(Vector2 cp1, Vector2 cp2, Vector2 endPoint) => Verbs.Add("AddCubicBezier");
        public void AddQuadraticBezier(Vector2 cp, Vector2 endPoint) => Verbs.Add("AddQuadraticBezier");
        public void AddArc(Vector2 endPoint, float radiusX, float radiusY, float rotationAngle, CanvasSweepDirection sweepDirection, CanvasArcSize arcSize) => Verbs.Add("AddArc");
        public void EndFigure(CanvasFigureLoop figureLoop) => Verbs.Add("EndFigure");
        public void SetFilledRegionDetermination(CanvasFilledRegionDetermination value) => Verbs.Add("SetFilledRegionDetermination");
        public void SetSegmentOptions(CanvasFigureSegmentOptions options) => Verbs.Add("SetSegmentOptions");
    }
}
