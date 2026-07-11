using System.Numerics;
using Windows.UI;

namespace Microsoft.Graphics.Canvas.Ink
{
    public sealed class CanvasInkStroke
    {
        public CanvasInkStroke(IReadOnlyList<Vector2> points, Color color, float width)
        {
            Points = points;
            Color = color;
            Width = width;
        }

        public IReadOnlyList<Vector2> Points { get; }
        public Color Color { get; }
        public float Width { get; }
    }

    public sealed class CanvasInkStrokeBuilder
    {
        private readonly List<Vector2> _points = new();
        public Color Color { get; set; } = Color.FromArgb(255, 0, 0, 0);
        public float Width { get; set; } = 2f;
        public void AddPoint(Vector2 point) => _points.Add(point);
        public CanvasInkStroke Build() => new(_points.ToArray(), Color, Width);
    }

    public sealed class CanvasInk
    {
        private readonly List<CanvasInkStroke> _strokes = new();
        public IReadOnlyList<CanvasInkStroke> Strokes => _strokes;
        public void AddStroke(CanvasInkStroke stroke) => _strokes.Add(stroke);

        public void Draw(CanvasDrawingSession drawingSession)
        {
            foreach (CanvasInkStroke stroke in _strokes)
            {
                for (int i = 1; i < stroke.Points.Count; i++)
                {
                    Vector2 a = stroke.Points[i - 1];
                    Vector2 b = stroke.Points[i];
                    drawingSession.DrawLine(a.X, a.Y, b.X, b.Y, stroke.Color, stroke.Width);
                }
            }
        }
    }
}
