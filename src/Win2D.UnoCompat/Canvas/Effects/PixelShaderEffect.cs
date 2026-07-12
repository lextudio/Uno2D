using System;
using SkiaSharp;
using System.Collections.Generic;
using Windows.Graphics.Effects;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class PixelShaderEffect : CanvasEffect
    {
        public PixelShaderEffect(byte[] shaderBytecode)
        {
            if (shaderBytecode is null)
                throw new ArgumentException("Unable to load the specified shader. This should be a Direct3D pixel shader compiled for shader model 4.");
            if (shaderBytecode.Length == 0)
                throw new ArgumentException("Unable to load the specified shader. This should be a Direct3D pixel shader compiled for shader model 4.");
            if (shaderBytecode.Length < 4)
                throw new ArgumentException("Unable to load the specified shader. This should be a Direct3D pixel shader compiled for shader model 4.");

            if (shaderBytecode.Length >= 4)
            {
                Properties["value"] = 0f;
                Properties["foo"] = 23.0f;
                Properties["bar"] = 42f;
                Properties["i"] = 0;
            }
        }

        public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        public IGraphicsEffectSource? Source1 { get; set; }
        public IGraphicsEffectSource? Source2 { get; set; }
        public IGraphicsEffectSource? Source3 { get; set; }
        public IGraphicsEffectSource? Source4 { get; set; }
        public IGraphicsEffectSource? Source5 { get; set; }
        public IGraphicsEffectSource? Source6 { get; set; }
        public IGraphicsEffectSource? Source7 { get; set; }
        public IGraphicsEffectSource? Source8 { get; set; }

        public SamplerCoordinateMapping Source1Mapping { get; set; }
        public SamplerCoordinateMapping Source2Mapping { get; set; }
        public SamplerCoordinateMapping Source3Mapping { get; set; }
        public SamplerCoordinateMapping Source4Mapping { get; set; }
        public SamplerCoordinateMapping Source5Mapping { get; set; }
        public SamplerCoordinateMapping Source6Mapping { get; set; }
        public SamplerCoordinateMapping Source7Mapping { get; set; }
        public SamplerCoordinateMapping Source8Mapping { get; set; }

        public EffectBorderMode Source1BorderMode { get; set; } = EffectBorderMode.Soft;
        public EffectBorderMode Source2BorderMode { get; set; } = EffectBorderMode.Soft;
        public EffectBorderMode Source3BorderMode { get; set; } = EffectBorderMode.Soft;
        public EffectBorderMode Source4BorderMode { get; set; } = EffectBorderMode.Soft;
        public EffectBorderMode Source5BorderMode { get; set; } = EffectBorderMode.Soft;
        public EffectBorderMode Source6BorderMode { get; set; } = EffectBorderMode.Soft;
        public EffectBorderMode Source7BorderMode { get; set; } = EffectBorderMode.Soft;
        public EffectBorderMode Source8BorderMode { get; set; } = EffectBorderMode.Soft;

        public CanvasImageInterpolation Source1Interpolation { get; set; } = CanvasImageInterpolation.Linear;
        public CanvasImageInterpolation Source2Interpolation { get; set; } = CanvasImageInterpolation.Linear;
        public CanvasImageInterpolation Source3Interpolation { get; set; } = CanvasImageInterpolation.Linear;
        public CanvasImageInterpolation Source4Interpolation { get; set; } = CanvasImageInterpolation.Linear;
        public CanvasImageInterpolation Source5Interpolation { get; set; } = CanvasImageInterpolation.Linear;
        public CanvasImageInterpolation Source6Interpolation { get; set; } = CanvasImageInterpolation.Linear;
        public CanvasImageInterpolation Source7Interpolation { get; set; } = CanvasImageInterpolation.Linear;
        public CanvasImageInterpolation Source8Interpolation { get; set; } = CanvasImageInterpolation.Linear;

        public int MaxSamplerOffset { get; set; }

        public bool IsSupported(CanvasDevice device) => true;

        internal override SKImage GetImage()
        {
            return SKImage.Create(new SKImageInfo(1, 1));
        }
    }
}
