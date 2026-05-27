using System;
using SkiaSharp;

namespace Microsoft.Graphics.Canvas.Text
{
    public enum CanvasHorizontalAlignment
    {
        Left,
        Center,
        Right
    }

    public enum CanvasVerticalAlignment
    {
        Top,
        Center,
        Bottom
    }

    public enum CanvasWordWrapping
    {
        NoWrap,
        Wrap
    }

    public sealed class CanvasTextFormat : IDisposable
    {
        public string? FontFamily { get; set; }
        public float FontSize { get; set; }
        public CanvasHorizontalAlignment HorizontalAlignment { get; set; }
        public CanvasVerticalAlignment VerticalAlignment { get; set; }
        public CanvasWordWrapping WordWrapping { get; set; }

        public SKTypeface ResolveTypeface()
        {
            if (!string.IsNullOrWhiteSpace(FontFamily))
            {
                try
                {
                    if (FontFamily.Contains("ms-appx:///", StringComparison.OrdinalIgnoreCase) || FontFamily.Contains("file://", StringComparison.OrdinalIgnoreCase))
                    {
                        string path = FontFamily.Replace("ms-appx:///", string.Empty).Replace("file://", string.Empty).TrimStart('/', '\\');
                        if (System.IO.File.Exists(path))
                            return SKTypeface.FromFile(path);
                    }

                    return SKTypeface.FromFamilyName(FontFamily) ?? SKTypeface.Default;
                }
                catch
                {
                }
            }

            return SKTypeface.Default;
        }

        public void Dispose()
        {
        }
    }

    public sealed class CanvasTextLayout : IDisposable
    {
        private readonly string _text;
        private readonly CanvasTextFormat _textFormat;
        private readonly float _maxWidth;
        private readonly float _maxHeight;

        public CanvasTextLayout(CanvasDevice device, string text, CanvasTextFormat format, float maxWidth, float maxHeight)
        {
            _text = text;
            _textFormat = format;
            _maxWidth = maxWidth;
            _maxHeight = maxHeight;
        }

        public string Text => _text;
        public CanvasTextFormat Format => _textFormat;
        public float MaxWidth => _maxWidth;
        public float MaxHeight => _maxHeight;

        public SKPath CreatePath()
        {
            using SKFont font = new SKFont(_textFormat.ResolveTypeface(), _textFormat.FontSize);
            return font.GetTextPath(_text, new SKPoint(0, 0));
        }

        public void Dispose()
        {
        }
    }
}
