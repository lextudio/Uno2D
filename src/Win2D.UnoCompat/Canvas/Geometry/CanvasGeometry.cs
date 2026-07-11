using SkiaSharp;
using System;
using System.Numerics;
using Windows.Foundation;
using Microsoft.Graphics.Canvas.Text;

namespace Microsoft.Graphics.Canvas.Geometry
{
    // ── Enums ──────────────────────────────────────────────────────────

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
        ForceRoundJoin,
        ForceUnstroked,
    }

    public enum CanvasDashStyle
    {
        Solid,
        Dash,
        Dot,
        DashDot,
        DashDotDot,
    }

    public enum CanvasCapStyle
    {
        Flat,
        Round,
        Square,
        Triangle,
    }

    public enum CanvasLineJoin
    {
        Miter,
        Bevel,
        Round,
        MiterOrBevel,
    }

    public enum CanvasStrokeTransformBehavior
    {
        Default,
        Fixed,
        Hairline,
    }

    public enum CanvasGeometrySimplification
    {
        Default,
        Lines,
    }

    public enum CanvasGeometryRelation
    {
        Disjoint,
        Contained,
        Contains,
        Overlap,
    }

    // ── Interfaces ────────────────────────────────────────────────────

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

    // ── CanvasGeometry ────────────────────────────────────────────────

    public sealed class CanvasGeometry : IDisposable
    {
        private readonly SKPath _path;

        private CanvasGeometry(SKPath path)
        {
            _path = path;
        }

        // ── Factory methods ────────────────────────────────────────────

        public static CanvasGeometry CreateRectangle(Rect rect)
        {
            var path = new SKPath();
            path.AddRect(ToSkRect(rect));
            return new CanvasGeometry(path);
        }

        public static CanvasGeometry CreateRoundedRectangle(Rect rect, float radiusX, float radiusY)
        {
            var path = new SKPath();
            path.AddRoundRect(ToSkRect(rect), radiusX, radiusY);
            return new CanvasGeometry(path);
        }

        public static CanvasGeometry CreateEllipse(Rect rect)
        {
            var path = new SKPath();
            path.AddOval(ToSkRect(rect));
            return new CanvasGeometry(path);
        }

        public static CanvasGeometry CreateCircle(float cx, float cy, float radius)
        {
            var path = new SKPath();
            path.AddCircle(cx, cy, radius);
            return new CanvasGeometry(path);
        }

        public static CanvasGeometry CreatePolygon(Vector2[] points)
        {
            var path = new SKPath();
            if (points is { Length: > 0 })
            {
                path.MoveTo(points[0].X, points[0].Y);
                for (int i = 1; i < points.Length; i++)
                    path.LineTo(points[i].X, points[i].Y);
                path.Close();
            }
            return new CanvasGeometry(path);
        }

        public static CanvasGeometry CreatePath(CanvasPathBuilder pathBuilder)
        {
            return new CanvasGeometry(pathBuilder.GetPath());
        }

        // ── CreateText (existing) ──────────────────────────────────────

        public static CanvasGeometry CreateText(Microsoft.Graphics.Canvas.Text.CanvasTextLayout layout)
        {
            SKPath path = layout.CreatePath();
            return new CanvasGeometry(path);
        }

        // ── Instance operations ────────────────────────────────────────

        public CanvasGeometry Combine(CanvasGeometry geometry, Matrix3x2 transform, CanvasGeometryCombine combine)
        {
            var other = new SKPath();
            geometry._path.Transform(ToSKMatrix(transform), other);
            var result = _path.Op(other, ToSkPathOp(combine));
            return new CanvasGeometry(result ?? new SKPath());
        }

        public CanvasGeometry Stroke(float strokeWidth, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth,
                IsAntialias = true,
            };
            if (strokeStyle is { })
            {
                paint.StrokeCap = strokeStyle.GetSkStrokeCap(strokeStyle.StartCap);
                paint.StrokeJoin = strokeStyle.GetSkStrokeJoin(strokeStyle.LineJoin);
            }
            var result = new SKPath();
            paint.GetFillPath(_path, result);
            return new CanvasGeometry(result);
        }

        public CanvasGeometry Outline(float strokeWidth, CanvasStrokeStyle? strokeStyle = null)
        {
            // In Win2D, Outline is identical to Stroke for simple cases;
            // for complex paths it may differ. Use same approach as Stroke.
            return Stroke(strokeWidth, strokeStyle);
        }

        public CanvasGeometry Simplify(CanvasFilledRegionDetermination fillDetermination)
        {
            var result = _path.Op(_path, SKPathOp.Union) ?? new SKPath();
            result.FillType = fillDetermination == CanvasFilledRegionDetermination.Winding
                ? SKPathFillType.Winding
                : SKPathFillType.EvenOdd;
            return new CanvasGeometry(result);
        }

        public float ComputeArea(Matrix3x2? transform = null)
        {
            var path = _path;
            if (transform.HasValue)
            {
                path = new SKPath();
                _path.Transform(ToSKMatrix(transform.Value), path);
            }

            // Shoelace formula on path points
            var points = path.Points;
            if (points.Length < 3)
                return 0f;

            float area = 0f;
            int n = points.Length;
            for (int i = 0; i < n; i++)
            {
                int j = (i + 1) % n;
                area += points[i].X * points[j].Y;
                area -= points[j].X * points[i].Y;
            }

            if (path != _path) path.Dispose();

            return Math.Abs(area) / 2f;
        }

        public Vector2 ComputePointOnPath(float distance)
        {
            SKPoint point = _path.GetPoint((int)distance);
            return new Vector2(point.X, point.Y);
        }

        public bool FillContainsPoint(Vector2 point)
        {
            return _path.Contains(point.X, point.Y);
        }

        public bool CompareWith(CanvasGeometry otherGeometry)
        {
            // Compare by running intersection and checking if empty
            var result = _path.Op(otherGeometry._path, SKPathOp.Intersect);
            return result is not null && !result.IsEmpty;
        }

        public CanvasGeometry Transform(Matrix3x2 matrix)
        {
            var result = new SKPath();
            _path.Transform(ToSKMatrix(matrix), result);
            return new CanvasGeometry(result);
        }

        public Rect ComputeBounds()
        {
            SKRect bounds = _path.Bounds;
            return new Rect(bounds.Left, bounds.Top, bounds.Width, bounds.Height);
        }

        public Rect ComputeStrokeBounds(float strokeWidth, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth,
                IsAntialias = true,
            };
            if (strokeStyle is { })
            {
                paint.StrokeCap = strokeStyle.GetSkStrokeCap(strokeStyle.StartCap);
                paint.StrokeJoin = strokeStyle.GetSkStrokeJoin(strokeStyle.LineJoin);
                var dash = strokeStyle.GetDashEffect();
                if (dash is not null)
                    paint.PathEffect = dash;
            }
            paint.GetFillPath(_path, new SKPath());
            SKRect strokeBounds = _path.Bounds;
            strokeBounds.Inflate(strokeWidth, strokeWidth);
            return new Rect(strokeBounds.Left, strokeBounds.Top, strokeBounds.Width, strokeBounds.Height);
        }

        public float ComputePathLength()
        {
            float length = 0f;
            using var meas = new SKPathMeasure(_path);
            length = meas.Length;
            return length;
        }

        public bool StrokeContainsPoint(Vector2 point, float strokeWidth, CanvasStrokeStyle? strokeStyle = null)
        {
            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = strokeWidth,
                IsAntialias = true,
            };
            if (strokeStyle is { })
            {
                paint.StrokeCap = strokeStyle.GetSkStrokeCap(strokeStyle.StartCap);
                paint.StrokeJoin = strokeStyle.GetSkStrokeJoin(strokeStyle.LineJoin);
            }
            var result = new SKPath();
            paint.GetFillPath(_path, result);
            return result.Contains(point.X, point.Y);
        }

        public Vector2[] Tessellate()
        {
            using var meas = new SKPathMeasure(_path);
            float length = meas.Length;
            if (length <= 0f)
                return Array.Empty<Vector2>();

            int count = Math.Max(1, (int)(length / 2f));
            var points = new Vector2[count];
            var skPoint = new SKPoint();
            for (int i = 0; i < count; i++)
            {
                float dist = (float)i / count * length;
                meas.GetPosition(dist, out skPoint);
                points[i] = new Vector2(skPoint.X, skPoint.Y);
            }
            return points;
        }

        public static float ComputeFlatteningTolerance(float dpi, float logicalToDeviceScale)
        {
            return 0.25f / (dpi / 96f * logicalToDeviceScale);
        }

        public static float DefaultFlatteningTolerance { get; set; } = 0.25f;

        public static CanvasGeometry CreateGroup(CanvasGeometry geometry, Matrix3x2? transform = null)
        {
            ArgumentNullException.ThrowIfNull(geometry);
            if (transform.HasValue)
                return geometry.Transform(transform.Value);
            return new CanvasGeometry(new SKPath(geometry._path));
        }

        public static CanvasGeometry CreateGlyphRun(Vector2 point, ushort[] glyphIndices, CanvasFontFace fontFace, float fontSize)
        {
            ArgumentNullException.ThrowIfNull(fontFace);
            var path = new SKPath();
            using var font = new SKFont(fontFace.Typeface, fontSize);
            font.GetGlyphPaths(glyphIndices, (contourPath, matrix) =>
            {
                path.AddPath(contourPath, ref matrix);
            });
            path.Offset(point.X, point.Y);
            return new CanvasGeometry(path);
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

        // Internal — used by CanvasDrawingSession.DrawGeometry/FillGeometry
        internal SKPath GetPath() => _path;

        public void Dispose()
        {
            _path.Dispose();
        }

        // ── Private helpers ────────────────────────────────────────────

        private static SKRect ToSkRect(Rect rect)
            => new((float)rect.X, (float)rect.Y, (float)(rect.X + rect.Width), (float)(rect.Y + rect.Height));

        private static SKPathOp ToSkPathOp(CanvasGeometryCombine combine) => combine switch
        {
            CanvasGeometryCombine.Intersect => SKPathOp.Intersect,
            CanvasGeometryCombine.Xor => SKPathOp.Xor,
            CanvasGeometryCombine.Exclude => SKPathOp.Difference,
            _ => SKPathOp.Union,
        };

        private static SKMatrix ToSKMatrix(Matrix3x2 m)
        {
            return new SKMatrix
            {
                ScaleX = m.M11,
                SkewX = m.M21,
                TransX = m.M31,
                SkewY = m.M12,
                ScaleY = m.M22,
                TransY = m.M32,
                Persp0 = 0,
                Persp1 = 0,
                Persp2 = 1,
            };
        }
    }

    internal static class SkiaExtensions
    {
        public static Vector2 ToVector2(this SKPoint point) => new(point.X, point.Y);
    }
}
