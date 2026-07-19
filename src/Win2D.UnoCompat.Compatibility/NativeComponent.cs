using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics.Effects;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;

namespace NativeComponent
{
    public enum EffectPropertyMapping
    {
        Unknown = 0,
        Direct = 1,
        VectorX = 2,
        VectorY = 3,
        VectorZ = 4,
        VectorW = 5,
        RectToVector4 = 6,
        RadiansToDegrees = 7,
        ColorMatrixAlphaMode = 8,
        ColorToVector3 = 9,
        ColorToVector4 = 10,
    }

    internal sealed class MockDirect3DDevice : IDirect3DDevice
    {
        public MockDirect3DDevice() { }
        public MockDirect3DDevice(bool featureLevel93) { MaxSupportedFeatureLevel = featureLevel93 ? 0 : 1; }

        public int MaxSupportedFeatureLevel { get; } = 1;
        public void Trim() { }
        public void Dispose() { }
    }

    internal sealed class MockDirect3DSurface : IDirect3DSurface
    {
        private readonly int _width;
        private readonly int _height;
        private readonly DirectXPixelFormat _format;

        public MockDirect3DSurface(int width, int height, DirectXPixelFormat format)
        {
            _width = width;
            _height = height;
            _format = format;
        }

        public Direct3DSurfaceDescription Description => new Direct3DSurfaceDescription
        {
            Width = _width,
            Height = _height,
            Format = _format,
            MultisampleDescription = new Direct3DMultisampleDescription
            {
                Count = 1,
                Quality = 0,
            },
        };
        public void Dispose() { }
    }

    internal sealed class FixedSizeList<T> : IList<T>
    {
        private readonly List<T> _items;

        public FixedSizeList(int size)
        {
            _items = new List<T>(size);
            for (int i = 0; i < size; i++)
                _items.Add(default!);
        }

        public T this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        public int Count => _items.Count;
        public bool IsReadOnly => false;

        public void Add(T item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public void Insert(int index, T item) => throw new NotImplementedException();
        public bool Remove(T item) => throw new NotImplementedException();
        public void RemoveAt(int index) => throw new NotImplementedException();

        public bool Contains(T item) => _items.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => _items.GetEnumerator();
        public int IndexOf(T item) => _items.IndexOf(item);
    }

    public static class DeviceCreator
    {
        public static IDirect3DDevice CreateDevice() => new MockDirect3DDevice();
        public static IDirect3DDevice CreateDevice(bool useFeatureLevel93)
        {
            CanvasDevice.NextDeviceFeatureLevel = useFeatureLevel93 ? 0 : 1;
            return new MockDirect3DDevice(useFeatureLevel93);
        }
    }

    public static class EffectAccessor
    {
        public static void GetNamedPropertyMapping(
            object effect,
            string propertyName,
            out int mappingIndex,
            out EffectPropertyMapping mapping)
        {
            mappingIndex = 0;
            mapping = EffectPropertyMapping.Unknown;
            if (effect is not null)
            {
                if (propertyName == "AlphaMode")
                {
                    if (effect is ColorManagementEffect)
                        mappingIndex = 4;
                    else if (effect is ColorMatrixEffect)
                        mappingIndex = 1;
                    mapping = EffectPropertyMapping.ColorMatrixAlphaMode;
                    return;
                }

                if (propertyName == "MultiplyAmount")
                {
                    mappingIndex = 0;
                    mapping = EffectPropertyMapping.VectorX;
                    return;
                }
                if (propertyName == "Source1Amount")
                {
                    mappingIndex = 0;
                    mapping = EffectPropertyMapping.VectorY;
                    return;
                }
                if (propertyName == "Source2Amount")
                {
                    mappingIndex = 0;
                    mapping = EffectPropertyMapping.VectorZ;
                    return;
                }
                if (propertyName == "Offset")
                {
                    mappingIndex = 0;
                    mapping = EffectPropertyMapping.VectorW;
                    return;
                }

                var prop = effect.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
                if (prop is not null)
                {
                    mapping = EffectPropertyMapping.Direct;
                }
            }
        }

        public static int GetPropertyCount(object effect) => 0;

        public static object? GetProperty(object effect, int index)
        {
            if (effect is null) return null;

            if (effect is ArithmeticCompositeEffect ace)
            {
                if (index == 0)
                    return new float[] { ace.MultiplyAmount, ace.Source1Amount, ace.Source2Amount, ace.Offset };
            }

            if (effect is ColorManagementEffect cme)
            {
                if (index == 4)
                    return (uint)(cme.AlphaMode == CanvasAlphaMode.Premultiplied ? 1 : 2);
            }

            if (effect is ColorMatrixEffect cmx)
            {
                if (index == 1)
                    return (uint)(cmx.AlphaMode == CanvasAlphaMode.Premultiplied ? 1 : 2);
            }

            var props = effect.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (index >= 0 && index < props.Length)
                return props[index].GetValue(effect);
            return null;
        }

        public static void RealizeEffect(object device, object effect)
        {
        }

        public static IGraphicsEffect? CreateThenWrapNewEffectOfSameType(object effect) => null;

        public static IGraphicsEffect? CreateThenWrapNewEffectOfSameType(object device, object effect) => null;

        public static int GetSourceCount(object effect) => 0;

        public static object? GetSource(object effect, int index) => null;
    }

    public static class ShaderCompiler
    {
        public static byte[] CompileShader(string hlsl, string profile)
        {
            return ShaderEncoder.EncodeShader(hlsl, profile);
        }
        public static byte[] CompileShaderAndEmbedLinkingFunction(string hlsl) => ShaderEncoder.EncodeCoordinateMapping(8);
    }

    public static class ReflectionHelper
    {
        public static bool IsFactoryAgile(string _) => true;
    }

    public static class SurfaceCreator
    {
        public static IDirect3DSurface CreateSurface(object graphicsDevice, int width, int height) => new MockDirect3DSurface(width, height, DirectXPixelFormat.B8G8R8A8UIntNormalized);
        public static IDirect3DSurface CreateSurface(object graphicsDevice, int width, int height, DirectXPixelFormat format) => new MockDirect3DSurface(width, height, format);
    }

    public static class VectorCreator
    {
        public static IList<int> CreateVectorOfInts(bool flag, int initialSize)
        {
            if (flag)
                return new FixedSizeList<int>(initialSize);
            return new List<int>();
        }
    }
}
