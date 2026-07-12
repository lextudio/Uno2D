using System.Numerics;
using SkiaSharp;
using Windows.Graphics.Effects;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class Transform3DEffect : CanvasEffect, IGraphicsEffect
    {
        public Matrix4x4 TransformMatrix { get; set; } = Matrix4x4.Identity;
        private CanvasImageInterpolation _interpolationMode = CanvasImageInterpolation.Linear;
        public CanvasImageInterpolation InterpolationMode
        {
            get => _interpolationMode;
            set
            {
                if (value == CanvasImageInterpolation.HighQualityCubic)
                    throw new ArgumentException("Transform3DEffect does not support HighQualityCubic interpolation mode.", nameof(value));
                _interpolationMode = value;
            }
        }
        public EffectBorderMode BorderMode { get; set; } = EffectBorderMode.Soft;
        public string Name { get; set; } = string.Empty;

        internal override SKImage GetImage()
        {
            return RequireSourceImage(Source);
        }
    }
}
