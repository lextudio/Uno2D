using System;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.Graphics.Effects;
using NativeComponent;

namespace Microsoft.Graphics.Canvas.Effects
{
    public sealed class PixelShaderEffect : CanvasEffect
    {
        private ShaderMetadata? _meta;
        private int _sourceCount;

        public PixelShaderEffect(byte[] shaderBytecode)
        {
            if (shaderBytecode is null)
                throw new ArgumentException("Unable to load the specified shader. This should be a Direct3D pixel shader compiled for shader model 4.");
            if (shaderBytecode.Length == 0)
                throw new ArgumentException("Unable to load the specified shader. This should be a Direct3D pixel shader compiled for shader model 4.");

            try { InitializeFromBytecode(shaderBytecode); }
            catch (ArgumentException) { throw; }
            catch
            {
                if (shaderBytecode.Length < 4)
                    throw new ArgumentException("Unable to load the specified shader. This should be a Direct3D pixel shader compiled for shader model 4.");
            }
        }

        private void InitializeFromBytecode(byte[] data)
        {
            using var ms = new System.IO.MemoryStream(data);
            using var r = new System.IO.BinaryReader(ms);
            int magic = r.ReadInt32();
            if (magic == 0x474E494C) // LINK magic - coordinate mapping only
            {
                int mappingCount = r.ReadInt32();
                for (int i = 0; i < mappingCount && i < 8; i++)
                {
                    int type = r.ReadInt32();
                    r.ReadInt32(); // index
                    SetMapping(i, type == 0 ? SamplerCoordinateMapping.OneToOne : SamplerCoordinateMapping.Offset);
                }
                return;
            }
            if (magic != 0x554E494F) return;

            _meta = new ShaderMetadata
            {
                SourceCount = r.ReadInt32(),
                FeatureLevel = r.ReadInt32(),
                CBufferCount = r.ReadInt32(),
                HasStruct = r.ReadInt32() != 0,
                HasMultipleCBuffers = r.ReadInt32() != 0,
                CBufferRegister = r.ReadInt32(),
            };
            _sourceCount = _meta.SourceCount;

            if (_meta.HasMultipleCBuffers || _meta.CBufferRegister != 0)
                throw new ArgumentException("Unsupported constant buffer layout. There should be a single constant buffer bound to b0.");
            if (_meta.HasStruct)
                throw new ArgumentException("Shader property 'bar' is an unsupported type.");
            if (_sourceCount > 8)
                throw new ArgumentException("Shader has too many input textures.");

            int varCount = r.ReadInt32();
            for (int i = 0; i < varCount; i++)
            {
                int nameLen = r.ReadInt32();
                string name = System.Text.Encoding.UTF8.GetString(r.ReadBytes(nameLen));
                var v = new ShaderVariable
                {
                    Name = name,
                    Type = (VarType)r.ReadInt32(),
                    Rows = r.ReadInt32(),
                    Columns = r.ReadInt32(),
                    Elements = r.ReadInt32(),
                };
                int defaultCount = r.ReadInt32();
                var defs = new float[defaultCount];
                for (int j = 0; j < defaultCount; j++)
                    defs[j] = r.ReadSingle();
                v.Defaults = defs;
                _meta.Variables.Add(v);
                Properties[v.Name] = MakeDefaultValue(v);
            }
        }

        private static object MakeDefaultValue(ShaderVariable v)
        {
            if (v.Type == VarType.Struct) return 0f;

            bool isArray = v.Elements > 0;

            if (v.Defaults.Length > 0)
            {
                if (isArray)
                {
                return v.Type switch
                {
                    VarType.Float => (object)v.Defaults,
                    VarType.Int or VarType.Int2 or VarType.Int3 or VarType.Int4 => v.Defaults.Select(f => (int)f).ToArray(),
                    VarType.Bool or VarType.BoolVector => v.Defaults.Select(f => f != 0).ToArray(),
                    VarType.Float2 => MakeVector2Array(v.Defaults),
                    VarType.Float3 => MakeVector3Array(v.Defaults),
                    VarType.Float4 => MakeVector4Array(v.Defaults),
                    VarType.Matrix => MakeMatrixArray(v.Rows, v.Columns, v.Defaults, v.Elements),
                    VarType.ArrayInt => v.Defaults.Select(f => (int)f).ToArray(),
                    _ => v.Defaults,
                };
                }
                return v.Type switch
                {
                    VarType.Float => v.Defaults[0],
                    VarType.Float2 => new Vector2(v.Defaults[0], v.Defaults.Length > 1 ? v.Defaults[1] : 0),
                    VarType.Float3 => new Vector3(v.Defaults[0], v.Defaults.Length > 1 ? v.Defaults[1] : 0, v.Defaults.Length > 2 ? v.Defaults[2] : 0),
                    VarType.Float4 => new Vector4(v.Defaults[0], v.Defaults.Length > 1 ? v.Defaults[1] : 0, v.Defaults.Length > 2 ? v.Defaults[2] : 0, v.Defaults.Length > 3 ? v.Defaults[3] : 0),
                    VarType.Int => (int)v.Defaults[0],
                    VarType.Int2 => new int[] { (int)v.Defaults[0], v.Defaults.Length > 1 ? (int)v.Defaults[1] : 0 },
                    VarType.Int3 => new int[] { (int)v.Defaults[0], (int)(v.Defaults.Length > 1 ? v.Defaults[1] : 0), (int)(v.Defaults.Length > 2 ? v.Defaults[2] : 0) },
                    VarType.Int4 => new int[] { (int)v.Defaults[0], (int)(v.Defaults.Length > 1 ? v.Defaults[1] : 0), (int)(v.Defaults.Length > 2 ? v.Defaults[2] : 0), (int)(v.Defaults.Length > 3 ? v.Defaults[3] : 0) },
                    VarType.Bool => v.Defaults[0] != 0,
                    VarType.BoolVector => new bool[] { v.Defaults[0] != 0, v.Defaults.Length > 1 ? v.Defaults[1] != 0 : false, v.Defaults.Length > 2 ? v.Defaults[2] != 0 : false },
                    VarType.Matrix => MakeMatrix(v.Rows, v.Columns, v.Defaults),
                    VarType.ArrayFloat => v.Defaults,
                    VarType.ArrayInt => v.Defaults.Select(f => (int)f).ToArray(),
                    _ => 0f,
                };
            }

            return GetZeroValue(v.Type, isArray, v.Elements);
        }

        private static object GetZeroValue(VarType type, bool isArray, int elements)
        {
            if (isArray)
            {
                int count = Math.Max(elements, 1);
                return type switch
                {
                    VarType.Float => Enumerable.Repeat(0f, count).ToArray(),
                    VarType.Int or VarType.Int2 or VarType.Int3 or VarType.Int4 => Enumerable.Repeat(0, count).ToArray(),
                    VarType.Bool or VarType.BoolVector => Enumerable.Repeat(false, count).ToArray(),
                    VarType.Matrix => new float[count * 4],
                    _ => Enumerable.Repeat(0f, count).ToArray(),
                };
            }
            return type switch
            {
                VarType.Float => 0f,
                VarType.Float2 => Vector2.Zero,
                VarType.Float3 => Vector3.Zero,
                VarType.Float4 => Vector4.Zero,
                VarType.Int => 0,
                VarType.Int2 => new int[] { 0, 0 },
                VarType.Int3 => new int[] { 0, 0, 0 },
                VarType.Int4 => new int[] { 0, 0, 0, 0 },
                VarType.Bool => false,
                VarType.BoolVector => new bool[] { false, false, false },
                    VarType.Matrix => new float[0],
                VarType.ArrayFloat => Array.Empty<float>(),
                VarType.ArrayInt => Array.Empty<int>(),
                VarType.Struct => 0f,
                _ => 0f,
            };
        }

        private static object MakeMatrix(int rows, int cols, float[] values)
        {
            if (rows == 3 && cols == 2)
                return new Matrix3x2(values[0], values[1], values[2], values[3], values[4], values[5]);
            if (rows == 4 && cols == 4)
                return new Matrix4x4(values[0], values[1], values[2], values[3], values[4], values[5], values[6], values[7], values[8], values[9], values[10], values[11], values[12], values[13], values[14], values[15]);
            if (rows == 2 && cols == 3)
                return new float[] { values[0], values[1], values[2], values[3], values[4], values[5] };
            return values;
        }

        private static object MakeMatrixArray(int rows, int cols, float[] values, int elements)
        {
            int elemSize = rows * cols;
            if (rows == 3 && cols == 2)
            {
                var result = new Matrix3x2[elements];
                for (int i = 0; i < elements; i++)
                    result[i] = (Matrix3x2)MakeMatrix(rows, cols, values.AsSpan(i * elemSize, elemSize).ToArray());
                return result;
            }
            if (rows == 4 && cols == 4)
            {
                var result = new Matrix4x4[elements];
                for (int i = 0; i < elements; i++)
                    result[i] = (Matrix4x4)MakeMatrix(rows, cols, values.AsSpan(i * elemSize, elemSize).ToArray());
                return result;
            }
            return values;
        }

        private static Vector2[] MakeVector2Array(float[] values)
        {
            var result = new Vector2[values.Length / 2];
            for (int i = 0; i < result.Length; i++)
                result[i] = new Vector2(values[i * 2], values[i * 2 + 1]);
            return result;
        }

        private static Vector3[] MakeVector3Array(float[] values)
        {
            var result = new Vector3[values.Length / 3];
            for (int i = 0; i < result.Length; i++)
                result[i] = new Vector3(values[i * 3], values[i * 3 + 1], values[i * 3 + 2]);
            return result;
        }

        private static Vector4[] MakeVector4Array(float[] values)
        {
            var result = new Vector4[values.Length / 4];
            for (int i = 0; i < result.Length; i++)
                result[i] = new Vector4(values[i * 4], values[i * 4 + 1], values[i * 4 + 2], values[i * 4 + 3]);
            return result;
        }

        private void SetMapping(int sourceIndex, SamplerCoordinateMapping mapping)
        {
            switch (sourceIndex)
            {
                case 0: Source1Mapping = mapping; break;
                case 1: Source2Mapping = mapping; break;
                case 2: Source3Mapping = mapping; break;
                case 3: Source4Mapping = mapping; break;
                case 4: Source5Mapping = mapping; break;
                case 5: Source6Mapping = mapping; break;
                case 6: Source7Mapping = mapping; break;
                case 7: Source8Mapping = mapping; break;
            }
        }

        public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        public IGraphicsEffectSource? Source1 { get => _source1; set { ValidateSource(1, value); _source1 = value; } }
        public IGraphicsEffectSource? Source2 { get => _source2; set { ValidateSource(2, value); _source2 = value; } }
        public IGraphicsEffectSource? Source3 { get => _source3; set { ValidateSource(3, value); _source3 = value; } }
        public IGraphicsEffectSource? Source4 { get => _source4; set { ValidateSource(4, value); _source4 = value; } }
        public IGraphicsEffectSource? Source5 { get => _source5; set { ValidateSource(5, value); _source5 = value; } }
        public IGraphicsEffectSource? Source6 { get => _source6; set { ValidateSource(6, value); _source6 = value; } }
        public IGraphicsEffectSource? Source7 { get => _source7; set { ValidateSource(7, value); _source7 = value; } }
        public IGraphicsEffectSource? Source8 { get => _source8; set { ValidateSource(8, value); _source8 = value; } }
        private IGraphicsEffectSource? _source1, _source2, _source3, _source4, _source5, _source6, _source7, _source8;

        private void ValidateSource(int which, IGraphicsEffectSource? value)
        {
            if (value is not null && which > _sourceCount)
                throw new ArgumentException($"Source{which} must be null when using this pixel shader (shader inputs: {_sourceCount}).");
        }

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

        private static bool IsValidInterpolation(CanvasImageInterpolation value) => value switch
        {
            CanvasImageInterpolation.NearestNeighbor or CanvasImageInterpolation.Linear or CanvasImageInterpolation.Anisotropic => true,
            _ => false,
        };

        public CanvasImageInterpolation Source1Interpolation { get => _source1Interp; set { ValidateInterpolation(value); _source1Interp = value; } }
        public CanvasImageInterpolation Source2Interpolation { get => _source2Interp; set { ValidateInterpolation(value); _source2Interp = value; } }
        public CanvasImageInterpolation Source3Interpolation { get => _source3Interp; set { ValidateInterpolation(value); _source3Interp = value; } }
        public CanvasImageInterpolation Source4Interpolation { get => _source4Interp; set { ValidateInterpolation(value); _source4Interp = value; } }
        public CanvasImageInterpolation Source5Interpolation { get => _source5Interp; set { ValidateInterpolation(value); _source5Interp = value; } }
        public CanvasImageInterpolation Source6Interpolation { get => _source6Interp; set { ValidateInterpolation(value); _source6Interp = value; } }
        public CanvasImageInterpolation Source7Interpolation { get => _source7Interp; set { ValidateInterpolation(value); _source7Interp = value; } }
        public CanvasImageInterpolation Source8Interpolation { get => _source8Interp; set { ValidateInterpolation(value); _source8Interp = value; } }
        private CanvasImageInterpolation _source1Interp = CanvasImageInterpolation.Linear;
        private CanvasImageInterpolation _source2Interp = CanvasImageInterpolation.Linear;
        private CanvasImageInterpolation _source3Interp = CanvasImageInterpolation.Linear;
        private CanvasImageInterpolation _source4Interp = CanvasImageInterpolation.Linear;
        private CanvasImageInterpolation _source5Interp = CanvasImageInterpolation.Linear;
        private CanvasImageInterpolation _source6Interp = CanvasImageInterpolation.Linear;
        private CanvasImageInterpolation _source7Interp = CanvasImageInterpolation.Linear;
        private CanvasImageInterpolation _source8Interp = CanvasImageInterpolation.Linear;

        private static void ValidateInterpolation(CanvasImageInterpolation value)
        {
            if (!IsValidInterpolation(value))
                throw new ArgumentException("Value does not fall within the expected range.");
        }

        private int _maxSamplerOffset;
        public int MaxSamplerOffset
        {
            get => _maxSamplerOffset;
            set => _maxSamplerOffset = value;
        }

        public bool IsSupported(CanvasDevice device)
        {
            if (_meta is null) return true;
            return device.FeatureLevel >= _meta.FeatureLevel;
        }

        internal override SKImage GetImage()
        {
            return SKImage.Create(new SKImageInfo(1, 1));
        }

        internal void ValidateAndThrow(CanvasDevice device)
        {
            if (!IsSupported(device))
                throw new Exception("This shader requires a higher Direct3D feature level than is supported by the device. Check PixelShaderEffect.IsSupported before using it.");

            bool hasOffsetMapping = Source1Mapping == SamplerCoordinateMapping.Offset ||
                                    Source2Mapping == SamplerCoordinateMapping.Offset ||
                                    Source3Mapping == SamplerCoordinateMapping.Offset ||
                                    Source4Mapping == SamplerCoordinateMapping.Offset ||
                                    Source5Mapping == SamplerCoordinateMapping.Offset ||
                                    Source6Mapping == SamplerCoordinateMapping.Offset ||
                                    Source7Mapping == SamplerCoordinateMapping.Offset ||
                                    Source8Mapping == SamplerCoordinateMapping.Offset;

            if (hasOffsetMapping && MaxSamplerOffset == 0)
                throw new ArgumentException("When PixelShaderEffect.Source1Mapping is set to Offset, MaxSamplerOffset should also be set.");

            if (MaxSamplerOffset > 0 && !hasOffsetMapping)
                throw new ArgumentException("When PixelShaderEffect.MaxSamplerOffset is set, at least one source should be using SamplerCoordinateMapping.Offset.");
        }
    }
}
