using SkiaSharp;
using System.Numerics;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class Transform2DEffect : CanvasEffect
    {
        public Matrix3x2 TransformMatrix { get; set; } = Matrix3x2.Identity;

        public EffectBorderMode BorderMode { get; set; } = EffectBorderMode.Soft;

        public CanvasImageInterpolation InterpolationMode { get; set; } = CanvasImageInterpolation.Linear;

        public float Sharpness { get; set; } = 1f;

        public new ICanvasImage? Source { get; set; }

        internal override SKImage GetImage()
        {
            using SKImage input = RequireSourceImage(Source);
            SKRect bounds = new(0, 0, input.Width, input.Height);
            SKMatrix matrix = ToSkMatrix(TransformMatrix);
            bounds = matrix.MapRect(bounds);
            int width = Math.Max(1, (int)MathF.Ceiling(bounds.Width + MathF.Abs(bounds.Left)));
            int height = Math.Max(1, (int)MathF.Ceiling(bounds.Height + MathF.Abs(bounds.Top)));
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using SKSurface surface = SKSurface.Create(info)
                ?? throw new InvalidOperationException("Unable to create transform effect surface.");
            surface.Canvas.Clear(SKColors.Transparent);
            surface.Canvas.Concat(matrix);
            surface.Canvas.DrawImage(input, 0, 0);
            surface.Canvas.Flush();
            return surface.Snapshot();
        }
    }
}
