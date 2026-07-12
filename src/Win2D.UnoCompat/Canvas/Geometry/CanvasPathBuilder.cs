using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Microsoft.Graphics.Canvas.Geometry
{
    internal enum PathBuilderOpType
    {
        SetFilledRegionDetermination,
        SetSegmentOptions,
        AddArc,
    }

    internal readonly record struct PathBuilderOp(
        PathBuilderOpType Type,
        CanvasFilledRegionDetermination FilledRegionDetermination,
        CanvasFigureSegmentOptions SegmentOptions,
        Vector2 ArcEndPoint,
        float ArcRadiusX,
        float ArcRadiusY,
        float ArcRotationAngle,
        CanvasSweepDirection ArcSweepDirection,
        CanvasArcSize ArcSize);

    public sealed class CanvasPathBuilder
    {
        private readonly SKPath _path;
        private readonly List<PathBuilderOp> _ops = new();
        private CanvasFilledRegionDetermination _fillDetermination = CanvasFilledRegionDetermination.Alternate;
        private CanvasFigureSegmentOptions _segmentOptions = CanvasFigureSegmentOptions.None;

        public CanvasPathBuilder(CanvasDevice device)
        {
            _path = new SKPath();
        }

        public void BeginFigure(float x, float y)
        {
            _path.MoveTo(x, y);
        }

        public void BeginFigure(Vector2 point)
        {
            _path.MoveTo(point.X, point.Y);
        }

        public void AddLine(float x, float y)
        {
            _path.LineTo(x, y);
        }

        public void AddLine(Vector2 point)
        {
            _path.LineTo(point.X, point.Y);
        }

        public void AddCubicBezier(Vector2 controlPoint1, Vector2 controlPoint2, Vector2 endPoint)
        {
            _path.CubicTo(controlPoint1.X, controlPoint1.Y, controlPoint2.X, controlPoint2.Y, endPoint.X, endPoint.Y);
        }

        public void AddBezier(Vector2 controlPoint1, Vector2 controlPoint2, Vector2 endPoint)
        {
            _path.CubicTo(controlPoint1.X, controlPoint1.Y, controlPoint2.X, controlPoint2.Y, endPoint.X, endPoint.Y);
        }

        public void AddBezier(float x1, float y1, float x2, float y2, float x3, float y3)
        {
            _path.CubicTo(x1, y1, x2, y2, x3, y3);
        }

        public void AddQuadraticBezier(Vector2 controlPoint, Vector2 endPoint)
        {
            _path.QuadTo(controlPoint.X, controlPoint.Y, endPoint.X, endPoint.Y);
        }

        public void AddQuadraticBezier(float x1, float y1, float x2, float y2)
        {
            _path.QuadTo(x1, y1, x2, y2);
        }

        public void AddArc(Vector2 endPoint, float radiusX, float radiusY, float rotationAngle, CanvasSweepDirection sweepDirection, CanvasArcSize arcSize)
        {
            _ops.Add(new PathBuilderOp(PathBuilderOpType.AddArc, default, default,
                endPoint, radiusX, radiusY, rotationAngle, sweepDirection, arcSize));
            var dir = sweepDirection == CanvasSweepDirection.Clockwise
                ? SKPathDirection.Clockwise
                : SKPathDirection.CounterClockwise;
            var size = arcSize == CanvasArcSize.Large
                ? SKPathArcSize.Large
                : SKPathArcSize.Small;
            _path.ArcTo(radiusX, radiusY, rotationAngle, size, dir, endPoint.X, endPoint.Y);
        }

        public void AddGeometry(CanvasGeometry geometry)
        {
            _path.AddPath(geometry.GetPath());
        }

        public void EndFigure(CanvasFigureLoop figureLoop)
        {
            if (figureLoop == CanvasFigureLoop.Closed)
                _path.Close();
        }

        public void SetFilledRegionDetermination(CanvasFilledRegionDetermination value)
        {
            _ops.Add(new PathBuilderOp(PathBuilderOpType.SetFilledRegionDetermination, value, default, default, 0, 0, 0, default, default));
            _fillDetermination = value;
            _path.FillType = value == CanvasFilledRegionDetermination.Winding
                ? SKPathFillType.Winding
                : SKPathFillType.EvenOdd;
        }

        public void SetSegmentOptions(CanvasFigureSegmentOptions options)
        {
            _ops.Add(new PathBuilderOp(PathBuilderOpType.SetSegmentOptions, default, options, default, 0, 0, 0, default, default));
            _segmentOptions = options;
        }

        // Internal — called by CanvasGeometry.CreatePath
        internal SKPath GetPath()
        {
            _path.FillType = _fillDetermination == CanvasFilledRegionDetermination.Winding
                ? SKPathFillType.Winding
                : SKPathFillType.EvenOdd;
            return _path;
        }

        internal IReadOnlyList<PathBuilderOp> GetOps() => _ops;
    }
}
