using SkiaSharp;
using System.Numerics;
using Windows.UI;

namespace Microsoft.Graphics.Canvas.Brushes
{
    public enum CanvasEdgeBehavior
    {
        Clamp,
        Wrap,
        Mirror,
    }

    public struct CanvasGradientStop
    {
        public float Position { get; set; }
        public Color Color { get; set; }
    }

    internal static class CanvasBrushHelpers
    {
        public static SKColor ToSkColor(Color color, float opacity = 1f)
        {
            byte alpha = (byte)Math.Clamp(color.A * Math.Clamp(opacity, 0f, 1f), 0f, 255f);
            return new SKColor(color.R, color.G, color.B, alpha);
        }

        public static SKShaderTileMode ToSkTileMode(CanvasEdgeBehavior behavior) => behavior switch
        {
            CanvasEdgeBehavior.Wrap => SKShaderTileMode.Repeat,
            CanvasEdgeBehavior.Mirror => SKShaderTileMode.Mirror,
            _ => SKShaderTileMode.Clamp,
        };

        public static SKMatrix ToSkMatrix(Matrix3x2 matrix)
        {
            return new SKMatrix
            {
                ScaleX = matrix.M11,
                SkewX = matrix.M21,
                TransX = matrix.M31,
                SkewY = matrix.M12,
                ScaleY = matrix.M22,
                TransY = matrix.M32,
                Persp0 = 0,
                Persp1 = 0,
                Persp2 = 1,
            };
        }

        public static SKPaint CreatePaint(SKPaintStyle style, bool isAntialias, SKBlendMode blendMode)
        {
            return new SKPaint
            {
                Style = style,
                IsAntialias = isAntialias,
                BlendMode = blendMode,
            };
        }
    }
}
