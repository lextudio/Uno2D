using FluentAssertions;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Windows.Foundation;
using Windows.UI;
using Xunit;

namespace Win2D.UnoCompat.Tests;

public sealed class CanvasPathBuilderTests
{
    [Fact]
    public void BuildsRectanglePath()
    {
        var builder = new CanvasPathBuilder(CanvasDevice.GetSharedDevice());
        builder.BeginFigure(0, 0);
        builder.AddLine(10, 0);
        builder.AddLine(10, 10);
        builder.AddLine(0, 10);
        builder.EndFigure(CanvasFigureLoop.Closed);

        using CanvasGeometry geometry = CanvasGeometry.CreatePath(builder);
        Rect bounds = geometry.ComputeBounds();

        bounds.Should().Be(new Rect(0, 0, 10, 10));
    }

    [Fact]
    public void BuildsPathWithBezier()
    {
        var builder = new CanvasPathBuilder(CanvasDevice.GetSharedDevice());
        builder.BeginFigure(new System.Numerics.Vector2(0, 0));
        builder.AddBezier(new System.Numerics.Vector2(10, 20), new System.Numerics.Vector2(30, 20), new System.Numerics.Vector2(40, 0));
        builder.EndFigure(CanvasFigureLoop.Open);

        using CanvasGeometry geometry = CanvasGeometry.CreatePath(builder);
        Rect bounds = geometry.ComputeBounds();

        bounds.Width.Should().BeGreaterThan(0);
        bounds.Height.Should().BeGreaterThan(0);
    }

    [Fact]
    public void BuildsPathWithQuadraticBezier()
    {
        var builder = new CanvasPathBuilder(CanvasDevice.GetSharedDevice());
        builder.BeginFigure(0, 0);
        builder.AddQuadraticBezier(10, 20, 20, 0);
        builder.EndFigure(CanvasFigureLoop.Open);

        using CanvasGeometry geometry = CanvasGeometry.CreatePath(builder);
        Rect bounds = geometry.ComputeBounds();

        bounds.Width.Should().BeGreaterThan(0);
        bounds.Height.Should().BeGreaterThan(0);
    }

    [Fact]
    public void BuildsPathWithArc()
    {
        var builder = new CanvasPathBuilder(CanvasDevice.GetSharedDevice());
        builder.BeginFigure(0, 0);
        builder.AddArc(new System.Numerics.Vector2(10, 0), 5, 5, 0, CanvasSweepDirection.Clockwise, CanvasArcSize.Small);
        builder.EndFigure(CanvasFigureLoop.Open);

        using CanvasGeometry geometry = CanvasGeometry.CreatePath(builder);
        Rect bounds = geometry.ComputeBounds();

        bounds.Width.Should().BeGreaterThan(0);
    }

    [Fact]
    public void BuildsPathWithFilledRegionDetermination()
    {
        var builder = new CanvasPathBuilder(CanvasDevice.GetSharedDevice());
        builder.SetFilledRegionDetermination(CanvasFilledRegionDetermination.Winding);
        builder.BeginFigure(0, 0);
        builder.AddLine(10, 0);
        builder.AddLine(5, 10);
        builder.EndFigure(CanvasFigureLoop.Closed);

        using CanvasGeometry geometry = CanvasGeometry.CreatePath(builder);
        float area = geometry.ComputeArea();

        area.Should().BeApproximately(50f, 0.1f);
    }

    [Fact]
    public void BuildsPathWithVectorOverloads()
    {
        var builder = new CanvasPathBuilder(CanvasDevice.GetSharedDevice());
        builder.BeginFigure(new System.Numerics.Vector2(0, 0));
        builder.AddLine(new System.Numerics.Vector2(5, 0));
        builder.AddQuadraticBezier(new System.Numerics.Vector2(5, 5), new System.Numerics.Vector2(10, 5));
        builder.EndFigure(CanvasFigureLoop.Open);

        using CanvasGeometry geometry = CanvasGeometry.CreatePath(builder);
        Rect bounds = geometry.ComputeBounds();

        bounds.Width.Should().BeGreaterThan(0);
        bounds.Height.Should().BeGreaterThan(0);
    }

    [Fact]
    public void BuildsPathWithAddGeometry()
    {
        using CanvasGeometry inner = CanvasGeometry.CreateRectangle(new Rect(0, 0, 5, 5));
        var builder = new CanvasPathBuilder(CanvasDevice.GetSharedDevice());
        builder.BeginFigure(0, 0);
        builder.AddLine(10, 0);
        builder.AddLine(10, 10);
        builder.AddLine(0, 10);
        builder.EndFigure(CanvasFigureLoop.Closed);
        builder.AddGeometry(inner);

        using CanvasGeometry geometry = CanvasGeometry.CreatePath(builder);
        float area = geometry.ComputeArea();

        area.Should().BeGreaterThan(100);
    }

    [Fact]
    public void BuildsPathWithSetSegmentOptions()
    {
        var builder = new CanvasPathBuilder(CanvasDevice.GetSharedDevice());
        builder.SetSegmentOptions(CanvasFigureSegmentOptions.ForceRoundJoin);
        builder.BeginFigure(0, 0);
        builder.AddLine(10, 0);
        builder.AddLine(10, 10);
        builder.AddLine(0, 10);
        builder.EndFigure(CanvasFigureLoop.Closed);

        using CanvasGeometry geometry = CanvasGeometry.CreatePath(builder);
        geometry.ComputeArea().Should().BeApproximately(100f, 0.1f);
    }

    [Fact]
    public void BuildsPathWithLargeArc()
    {
        var builder = new CanvasPathBuilder(CanvasDevice.GetSharedDevice());
        builder.BeginFigure(0, 0);
        builder.AddArc(new System.Numerics.Vector2(10, 0), 10, 10, 0, CanvasSweepDirection.CounterClockwise, CanvasArcSize.Large);
        builder.EndFigure(CanvasFigureLoop.Open);

        using CanvasGeometry geometry = CanvasGeometry.CreatePath(builder);
        geometry.ComputeBounds().Width.Should().BeGreaterThan(0);
    }

    [Fact]
    public void BuildsPathWithBezierFloats()
    {
        var builder = new CanvasPathBuilder(CanvasDevice.GetSharedDevice());
        builder.BeginFigure(0, 0);
        builder.AddBezier(10, 20, 30, 20, 40, 0);
        builder.EndFigure(CanvasFigureLoop.Open);

        using CanvasGeometry geometry = CanvasGeometry.CreatePath(builder);
        geometry.ComputeBounds().Width.Should().BeGreaterThan(0);
    }

    [Fact]
    public void BuildsPathWithAlternateFillDetermination()
    {
        var builder = new CanvasPathBuilder(CanvasDevice.GetSharedDevice());
        builder.SetFilledRegionDetermination(CanvasFilledRegionDetermination.Alternate);
        builder.BeginFigure(0, 0);
        builder.AddLine(10, 0);
        builder.AddLine(10, 10);
        builder.AddLine(0, 10);
        builder.EndFigure(CanvasFigureLoop.Closed);

        using CanvasGeometry geometry = CanvasGeometry.CreatePath(builder);
        geometry.ComputeArea().Should().BeApproximately(100f, 0.1f);
    }

    [Fact]
    public void BuildsOpenFigure()
    {
        var builder = new CanvasPathBuilder(CanvasDevice.GetSharedDevice());
        builder.BeginFigure(0, 0);
        builder.AddLine(10, 0);
        builder.AddLine(10, 10);
        builder.EndFigure(CanvasFigureLoop.Open);

        using CanvasGeometry geometry = CanvasGeometry.CreatePath(builder);
        geometry.ComputeBounds().Width.Should().Be(10);
    }
}
