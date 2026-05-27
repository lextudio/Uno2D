using SkiaSharp;
using System;
using System.Numerics;
using System.Text;
using Windows.Foundation;

namespace Microsoft.Graphics.Canvas.Geometry
{
    public enum CanvasFigureFill
    {
        Alternate,
        Winding
    }

    public enum CanvasFigureLoop
    {
        Open,
        Closed
    }

    public enum CanvasSweepDirection
    {
        Clockwise,
        CounterClockwise
    }

    public enum CanvasArcSize
    {
        Small,
        Large
    }

    public enum CanvasFilledRegionDetermination
    {
        Alternate,
        Winding
    }

    public enum CanvasFigureSegmentOptions
    {
        None,
        ForceRoundJoin
    }

    public enum CanvasDashStyle
    {
        Solid,
        Dash
    }

    public enum CanvasCapStyle
    {
        Flat,
        Round,
        Square
    }

    public enum CanvasLineJoin
    {
        Miter,
        Bevel,
        Round
    }

    public interface ICanvasPathReceiver
    {
        void BeginFigure(Vector2 startPoint, CanvasFigureFill figureFill);
        void AddLine(Vector2 endPoint);
        void AddCubicBezier(Vector2 cp1, Vector2 cp2, Vector2 endPoint);
        void AddQuadraticBezier(Vector2 cp, Vector2 endPoint);
        void AddArc(Vector2 endPoint, float radiusX, float radiusY, float rotationAngle, CanvasSweepDirection sweepDirection, CanvasArcSize arcSize);
        void EndFigure(CanvasFigureLoop figureLoop);
        void SetFilledRegionDetermination(CanvasFilledRegionDetermination value);
        void SetSegmentOptions(CanvasFigureSegmentOptions options);
    }

    public sealed class CanvasGeometry : IDisposable
    {
        private readonly SKPath _path;

        private CanvasGeometry(SKPath path)
        {
            _path = path;
        }

        public static CanvasGeometry CreateText(Microsoft.Graphics.Canvas.Text.CanvasTextLayout layout)
        {
            SKPath path = layout.CreatePath();
            return new CanvasGeometry(path);
        }

        public Rect ComputeBounds()
        {
            SKRect bounds = _path.Bounds;
            return new Rect(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
        }

        public void SendPathTo(ICanvasPathReceiver receiver)
        {
            using var iter = _path.CreateRawIterator();
            SKPoint[] points = new SKPoint[4];
            while (true)
            {
                SKPathVerb verb = iter.Next(points);
                switch (verb)
                {
                    case SKPathVerb.Move:
                        receiver.BeginFigure(points[0].ToVector2(), CanvasFigureFill.Alternate);
                        break;
                    case SKPathVerb.Line:
                        receiver.AddLine(points[1].ToVector2());
                        break;
                    case SKPathVerb.Quad:
                        receiver.AddQuadraticBezier(points[1].ToVector2(), points[2].ToVector2());
                        break;
                    case SKPathVerb.Conic:
                        receiver.AddQuadraticBezier(points[1].ToVector2(), points[2].ToVector2());
                        break;
                    case SKPathVerb.Cubic:
                        receiver.AddCubicBezier(points[1].ToVector2(), points[2].ToVector2(), points[3].ToVector2());
                        break;
                    case SKPathVerb.Close:
                        receiver.EndFigure(CanvasFigureLoop.Closed);
                        break;
                    case SKPathVerb.Done:
                        return;
                }
            }
        }

        public void Dispose()
        {
            _path.Dispose();
        }
    }

    internal static class SkiaExtensions
    {
        public static Vector2 ToVector2(this SKPoint point) => new(point.X, point.Y);
    }
}
