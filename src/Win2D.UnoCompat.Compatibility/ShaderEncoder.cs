using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace NativeComponent
{
    internal static class ShaderEncoder
    {
        public static byte[] EncodeShader(string hlsl, string profile)
        {
            if (profile.StartsWith("vs_"))
                return Array.Empty<byte>();

            bool is93 = profile.Contains("9_3");
            int sourceCount = 0;
            foreach (Match m in Regex.Matches(hlsl, @"\btexture2D\s+([^;]+)"))
                sourceCount += m.Groups[1].Value.Count(c => c == ',') + 1;
            bool hasStruct = Regex.IsMatch(hlsl, @"\bstruct\s+\w+\s*\{");
            int cbufferCount = Regex.Matches(hlsl, @"\bcbuffer\b").Count;
            bool hasMultipleCBuffers = cbufferCount > 1;
            int cbufferReg = 0;
            {
                var m = Regex.Match(hlsl, @"register\s*\(\s*b(\d+)\s*\)");
                if (m.Success) cbufferReg = int.Parse(m.Groups[1].Value);
            }
            var variables = ParseVariables(hlsl);

            using var ms = new System.IO.MemoryStream();
            var w = new System.IO.BinaryWriter(ms);
            w.Write(0x554E494F);
            w.Write(sourceCount);
            w.Write(is93 ? 0 : 1);
            w.Write(cbufferCount);
            w.Write(hasStruct ? 1 : 0);
            w.Write(hasMultipleCBuffers ? 1 : 0);
            w.Write(cbufferReg);
            w.Write(variables.Count);

            foreach (var v in variables)
            {
                byte[] nameBytes = Encoding.UTF8.GetBytes(v.Name);
                w.Write(nameBytes.Length);
                w.Write(nameBytes);
                w.Write((int)v.Type);
                w.Write(v.Rows);
                w.Write(v.Columns);
                w.Write(v.Elements);
                w.Write(v.Defaults.Length);
                foreach (float f in v.Defaults)
                    w.Write(f);
            }
            return ms.ToArray();
        }

        public static byte[] EncodeCoordinateMapping(int sourceCount)
        {
            using var ms = new System.IO.MemoryStream();
            var w = new System.IO.BinaryWriter(ms);
            w.Write(0x474E494C); // LINK magic
            w.Write(sourceCount);
            for (int i = 0; i < sourceCount; i++)
            {
                w.Write(i == 0 ? 0 : 1);
                w.Write(i);
            }
            return ms.ToArray();
        }

        struct VarInfo
        {
            public string Name;
            public VarType Type;
            public int Rows, Columns, Elements;
            public float[] Defaults;
        }

        static List<VarInfo> ParseVariables(string hlsl)
        {
            var vars = new List<VarInfo>();

            // Remove multi-line comments and single-line comments
            hlsl = Regex.Replace(hlsl, @"/\*.*?\*/", "", RegexOptions.Singleline);
            hlsl = Regex.Replace(hlsl, @"//[^\n]*", "");

            // Parse struct declarations
            var structNames = new HashSet<string>();
            foreach (Match sm in Regex.Matches(hlsl, @"struct\s+(\w+)\s*\{", RegexOptions.Multiline))
                structNames.Add(sm.Groups[1].Value);

            // Parse variables inside cbuffer blocks
            foreach (Match cbMatch in Regex.Matches(hlsl, @"cbuffer\s+\w+(?:\s*:\s*register\s*\(\s*\w+\s*\))?\s*\{([^}]+)\}"))
            {
                string body = cbMatch.Groups[1].Value;
                foreach (Match vm in Regex.Matches(body, @"\b(float|int|float2|float3|float4|int2|int3|int4|bool|bool2|bool3|bool4)\s+(\w+)"))
                {
                    var vi = MakeVar(vm.Groups[2].Value, vm.Groups[1].Value);
                    if (vi.HasValue) vars.Add(vi.Value);
                }
            }

            // Parse struct-typed variable declarations: Foo bar;
            foreach (Match sm in Regex.Matches(hlsl, @"^\s*(\w+)\s+(\w+)\s*;", RegexOptions.Multiline))
            {
                string typeName = sm.Groups[1].Value;
                string varName = sm.Groups[2].Value;
                if (structNames.Contains(typeName))
                {
                    if (!vars.Exists(v => v.Name == varName))
                        vars.Add(new VarInfo { Name = varName, Type = VarType.Struct, Defaults = Array.Empty<float>() });
                }
            }

            // Parse global variable declarations (not inside blocks)
            var globalDecls = new List<string>();
            foreach (Match gm in Regex.Matches(hlsl, @"\b(?:(?:row_major|column_major)\s+)?(?:float|int|bool|double)\w*\s+\w+(?:\s*\[\s*\d+\s*\])?(?:\s*=\s*[^;]*)?\s*;"))
            {
                string decl = gm.Value.Trim();
                if (!decl.StartsWith("cbuffer") && !decl.StartsWith("texture2D") && !decl.StartsWith("sampler") && !decl.StartsWith("SamplerState"))
                    globalDecls.Add(decl);
            }

            foreach (string decl in globalDecls)
            {
                var vi = ParseVariableDeclaration(decl);
                if (vi.HasValue && !vars.Exists(v => v.Name == vi.Value.Name))
                    vars.Add(vi.Value);
            }

            return vars;
        }

        static VarInfo? MakeVar(string name, string typeStr)
        {
            var vt = typeStr switch
            {
                "float" => VarType.Float, "float2" => VarType.Float2, "float3" => VarType.Float3, "float4" => VarType.Float4,
                "int" => VarType.Int, "int2" => VarType.Int2, "int3" => VarType.Int3, "int4" => VarType.Int4,
                "bool" => VarType.Bool, "bool2" => VarType.BoolVector, "bool3" => VarType.BoolVector, "bool4" => VarType.BoolVector,
                _ => VarType.Float,
            };
            return new VarInfo { Name = name, Type = vt, Rows = 1, Columns = 1, Defaults = Array.Empty<float>() };
        }

        static VarInfo? ParseVariableDeclaration(string decl)
        {
            // Match: [row_major|column_major] [type] [name][[N]] = { values };
            var m = Regex.Match(decl, @"^(row_major|column_major)?\s*(\w+(?:\d*x\d+)?)\s+(\w+)(?:\s*\[\s*(\d+)\s*\])?(?:\s*=\s*(.+))?\s*;");
            if (!m.Success) return null;

            string? rowMajorStr = m.Groups[1].Success ? m.Groups[1].Value : null;
            bool rowMajor = rowMajorStr == "row_major";
            string typeStr = m.Groups[2].Value;
            string name = m.Groups[3].Value;
            string arraySizeStr = m.Groups[4].Success ? m.Groups[4].Value : null;
            string initStr = m.Groups[5].Success ? m.Groups[5].Value.Trim() : null;

            if (typeStr == "true" || typeStr == "false") return null; // not a type, probably value
            if (typeStr == "main") return null;

            int elements = arraySizeStr != null ? int.Parse(arraySizeStr) : 0;

            // Parse type
            VarType vt;
            int rows = 1, cols = 1;
            if (typeStr.StartsWith("float"))
            {
                if (typeStr == "float" || typeStr == "float1") vt = VarType.Float;
                else if (typeStr == "float2") { vt = VarType.Float2; cols = 2; }
                else if (typeStr == "float3") { vt = VarType.Float3; cols = 3; }
                else if (typeStr == "float4") { vt = VarType.Float4; cols = 4; }
                else
                {
                    var mm = Regex.Match(typeStr, @"float(\d+)x(\d+)");
                    if (mm.Success)
                    {
                        int r = int.Parse(mm.Groups[1].Value);
                        int c = int.Parse(mm.Groups[2].Value);
                        if (r == 1 && c == 1)
                            vt = VarType.Float;
                        else
                        {
                            vt = VarType.Matrix;
                            rows = r; cols = c;
                        }
                    }
                    else if (typeStr == "float1x1")
                        vt = VarType.Float;
                    else
                        vt = VarType.Matrix;
                }
            }
            else if (typeStr.StartsWith("int"))
            {
                if (typeStr == "int" || typeStr == "int1") vt = VarType.Int;
                else if (typeStr == "int2") { vt = VarType.Int2; cols = 2; }
                else if (typeStr == "int3") { vt = VarType.Int3; cols = 3; }
                else if (typeStr == "int4") { vt = VarType.Int4; cols = 4; }
                else
                {
                    var mm = Regex.Match(typeStr, @"int(\d+)x(\d+)");
                    if (mm.Success)
                    {
                        int r = int.Parse(mm.Groups[1].Value);
                        int c = int.Parse(mm.Groups[2].Value);
                        if (r == 1 && c == 1)
                            vt = VarType.Int;
                        else
                        {
                            vt = VarType.ArrayInt;
                            if (elements == 0) elements = 1;
                        }
                    }
                    else
                        vt = VarType.Matrix;
                }
            }
            else if (typeStr.StartsWith("bool"))
            {
                if (typeStr == "bool" || typeStr == "bool1") vt = VarType.Bool;
                else
                {
                    var bmm = Regex.Match(typeStr, @"bool(\d+)x(\d+)");
                    if (bmm.Success)
                    {
                        vt = VarType.BoolVector;
                        if (elements == 0) elements = 1;
                    }
                    else
                    {
                        vt = VarType.BoolVector;
                        cols = typeStr switch { "bool2" => 2, "bool3" => 3, "bool4" => 4, _ => 1 };
                    }
                }
            }
            else return null;

            // Parse default values from initializer
            float[] defaults = ParseDefaultValues(initStr, typeStr, rows, cols, elements, rowMajor);

            return new VarInfo { Name = name, Type = vt, Rows = rows, Columns = cols, Elements = elements, Defaults = defaults };
        }

        static float[] ParseDefaultValues(string? initStr, string typeStr, int rows, int cols, int elements, bool rowMajor)
        {
            if (initStr == null) return Array.Empty<float>();

            // Remove enclosing braces if present
            string inner = initStr.Trim();
            if (inner.StartsWith("{"))
                inner = inner.Substring(1, inner.Length - 2).Trim();

            // Split by commas, parse each value
            var parts = new List<string>();
            int depth = 0;
            int start = 0;
            for (int i = 0; i <= inner.Length; i++)
            {
                if (i == inner.Length || (inner[i] == ',' && depth == 0))
                {
                    string part = inner.Substring(start, i - start).Trim().TrimStart('{').TrimEnd('}').Trim();
                    if (part.Length > 0)
                        parts.Add(part);
                    start = i + 1;
                }
                else if (inner[i] == '{') depth++;
                else if (inner[i] == '}') depth--;
            }

            if (parts.Count == 0) return Array.Empty<float>();

            var result = new List<float>();
            foreach (string part in parts)
            {
                if (part == "true") result.Add(1f);
                else if (part == "false") result.Add(0f);
                else if (float.TryParse(part, NumberStyles.Float, CultureInfo.InvariantCulture, out float fv))
                    result.Add(fv);
            }
            return result.ToArray();
        }
    }
}
