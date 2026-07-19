using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NativeComponent
{
    public class ShaderMetadata
    {
        public int SourceCount { get; set; }
        public int FeatureLevel { get; set; }
        public int CBufferCount { get; set; }
        public bool HasStruct { get; set; }
        public bool HasMultipleCBuffers { get; set; }
        public int CBufferRegister { get; set; }
        public List<ShaderVariable> Variables { get; set; } = new();
    }

    public class ShaderVariable
    {
        public string Name { get; set; }
        public VarType Type { get; set; }
        public int Rows { get; set; }
        public int Columns { get; set; }
        public int Elements { get; set; }
        public float[] Defaults { get; set; } = Array.Empty<float>();
    }

    public enum VarType
    {
        Float, Float2, Float3, Float4,
        Int, Int2, Int3, Int4,
        Bool, Matrix, ArrayFloat, ArrayInt,
        BoolVector, Struct
    }

    public class CoordinateMappingMetadata
    {
        public int SourceCount { get; set; }
        public List<(int Type, int Index)> Mappings { get; set; } = new();
    }

    internal static class ShaderDecoder
    {
        public static ShaderMetadata Decode(byte[] data)
        {
            if (data == null || data.Length < 4) return null;
            try
            {
                using var ms = new MemoryStream(data);
                var r = new BinaryReader(ms);
                if (r.ReadInt32() != 0x554E494F) return null;

                return new ShaderMetadata
                {
                    SourceCount = r.ReadInt32(),
                    FeatureLevel = r.ReadInt32(),
                    CBufferCount = r.ReadInt32(),
                    HasStruct = r.ReadInt32() != 0,
                    HasMultipleCBuffers = r.ReadInt32() != 0,
                    CBufferRegister = r.ReadInt32(),
                    Variables = ReadVariables(r),
                };
            }
            catch { return null; }
        }

        static List<ShaderVariable> ReadVariables(BinaryReader r)
        {
            int count = r.ReadInt32();
            var vars = new List<ShaderVariable>();
            for (int i = 0; i < count; i++)
            {
                int nameLen = r.ReadInt32();
                string name = Encoding.UTF8.GetString(r.ReadBytes(nameLen));
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
                vars.Add(v);
            }
            return vars;
        }

        public static CoordinateMappingMetadata DecodeMapping(byte[] data)
        {
            if (data == null || data.Length < 8) return null;
            try
            {
                using var ms = new MemoryStream(data);
                var r = new BinaryReader(ms);
                int magic = r.ReadInt32();
                if (magic != 0x474E494C && magic != 0x554E494F) return null;

                int count = r.ReadInt32();
                var meta = new CoordinateMappingMetadata { SourceCount = count };
                for (int i = 0; i < count; i++)
                    meta.Mappings.Add((r.ReadInt32(), r.ReadInt32()));
                return meta;
            }
            catch { return null; }
        }
    }
}
