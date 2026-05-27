using System;
using System.Collections.Concurrent;
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
        private static readonly ConcurrentDictionary<string, SKTypeface> TypefaceCache = new(StringComparer.OrdinalIgnoreCase);

        public string? FontFamily { get; set; }
        public float FontSize { get; set; }
        public CanvasHorizontalAlignment HorizontalAlignment { get; set; }
        public CanvasVerticalAlignment VerticalAlignment { get; set; }
        public CanvasWordWrapping WordWrapping { get; set; }

        public SKTypeface ResolveTypeface()
        {
            if (string.IsNullOrWhiteSpace(FontFamily))
                return SKTypeface.Default;

            if (TypefaceCache.TryGetValue(FontFamily, out SKTypeface? cached))
                return cached;

            SKTypeface resolved = SKTypeface.Default;

            try
            {
                string family = FontFamily;
                string? explicitFamilyName = null;

                    // WinUI/Uno commonly uses "uri#FamilyName".
                    int hashIndex = family.LastIndexOf('#');
                    if (hashIndex >= 0)
                    {
                        explicitFamilyName = family[(hashIndex + 1)..];
                        family = family[..hashIndex];
                    }

                if (family.Contains("ms-appx:///", StringComparison.OrdinalIgnoreCase) || family.Contains("file://", StringComparison.OrdinalIgnoreCase))
                {
                    string path = family
                        .Replace("ms-appx:///", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("file://", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .TrimStart('/', '\\');

                    // Resolve app package-relative asset path for ms-appx URIs.
                    if (!System.IO.Path.IsPathRooted(path))
                        path = System.IO.Path.Combine(AppContext.BaseDirectory, path);

                    if (System.IO.File.Exists(path))
                    {
                        SKTypeface? fromFile = SKTypeface.FromFile(path);
                        if (fromFile is not null)
                            resolved = fromFile;
                    }
                }

                if (ReferenceEquals(resolved, SKTypeface.Default) && !string.IsNullOrWhiteSpace(explicitFamilyName))
                {
                    SKTypeface? named = SKTypeface.FromFamilyName(explicitFamilyName);
                    if (named is not null)
                        resolved = named;
                }

                if (ReferenceEquals(resolved, SKTypeface.Default))
                    resolved = SKTypeface.FromFamilyName(family) ?? SKTypeface.Default;
            }
            catch
            {
            }

            return TypefaceCache.GetOrAdd(FontFamily, resolved);
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
