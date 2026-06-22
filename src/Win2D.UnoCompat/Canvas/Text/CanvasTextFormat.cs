using System;
using System.Collections.Concurrent;
using SkiaSharp;
using Windows.Foundation;

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

        // Cumulative advance (in DIPs) before each UTF-16 code unit. Length == _text.Length + 1;
        // _cumulativeX[i] is the X of the leading edge of character i, _cumulativeX[Length] is the
        // total advance. Built lazily from the resolved typeface's real glyph advances so caret /
        // selection / hit-testing align with what DrawText actually renders (no monospace guess).
        private float[]? _cumulativeX;
        private float _ascent;
        private float _descent;
        private float _leading;

        public CanvasTextLayout(CanvasDevice device, string text, CanvasTextFormat format, float maxWidth, float maxHeight)
        {
            _text = text ?? string.Empty;
            _textFormat = format;
            _maxWidth = maxWidth;
            _maxHeight = maxHeight;
        }

        public string Text => _text;
        public CanvasTextFormat Format => _textFormat;
        public float MaxWidth => _maxWidth;
        public float MaxHeight => _maxHeight;

        private void EnsureMetrics()
        {
            if (_cumulativeX is not null)
                return;

            using SKFont font = new SKFont(_textFormat.ResolveTypeface(), _textFormat.FontSize);
            SKFontMetrics fontMetrics = font.Metrics;
            _ascent = -fontMetrics.Ascent;        // Skia ascent is negative (above baseline)
            _descent = fontMetrics.Descent;       // positive (below baseline)
            _leading = fontMetrics.Leading;

            int len = _text.Length;
            var cumulative = new float[len + 1];
            if (len == 0)
            {
                _cumulativeX = cumulative;
                return;
            }

            // One glyph per Unicode code point; the high surrogate of a pair carries the advance,
            // the trailing low surrogate gets zero so caret indices map back to UTF-16 offsets.
            ushort[] glyphs = font.GetGlyphs(_text);
            float[] widths = font.GetGlyphWidths(glyphs);

            int glyphIndex = 0;
            float x = 0f;
            for (int i = 0; i < len; i++)
            {
                cumulative[i] = x;
                if (char.IsLowSurrogate(_text[i]))
                {
                    // Trailing unit of a surrogate pair — advance was already applied at the lead unit.
                    continue;
                }

                float advance = glyphIndex < widths.Length ? widths[glyphIndex] : 0f;
                glyphIndex++;
                x += advance;
            }
            cumulative[len] = x;
            _cumulativeX = cumulative;
        }

        private float TotalWidth
        {
            get { EnsureMetrics(); return _cumulativeX![_cumulativeX.Length - 1]; }
        }

        private float LineHeight
        {
            get { EnsureMetrics(); return _ascent + _descent + _leading; }
        }

        /// <summary>The bounds of the laid-out text. Single line (the editor formats one line at a time).</summary>
        public Rect LayoutBounds
        {
            get { EnsureMetrics(); return new Rect(0, 0, TotalWidth, LineHeight); }
        }

        /// <summary>
        /// Layout bounds including trailing whitespace. This shim's advance accumulation already
        /// counts whitespace glyph advances, so it equals <see cref="LayoutBounds"/>. (Real Win2D's
        /// plain LayoutBounds excludes trailing whitespace, which would collapse spaces between
        /// runs — callers measuring run advances must use this property for parity.)
        /// </summary>
        public Rect LayoutBoundsIncludingTrailingWhitespace
        {
            get { EnsureMetrics(); return new Rect(0, 0, TotalWidth, LineHeight); }
        }

        /// <summary>The bounds of the inked pixels. Approximated by the layout bounds for this shim.</summary>
        public Rect DrawBounds => LayoutBounds;

        public CanvasLineMetrics[] LineMetrics
        {
            get
            {
                EnsureMetrics();
                return new[]
                {
                    new CanvasLineMetrics
                    {
                        CharacterCount = _text.Length,
                        TrailingWhitespaceCount = CountTrailingWhitespace(),
                        TerminalNewlineCount = 0,
                        Height = LineHeight,
                        Baseline = _ascent,
                    }
                };
            }
        }

        /// <summary>
        /// Returns the position of the caret for a character index. Mirrors Win2D:
        /// <paramref name="trailingSideOfCharacter"/> selects the trailing (right) edge.
        /// </summary>
        public System.Numerics.Vector2 GetCaretPosition(int characterIndex, bool trailingSideOfCharacter)
        {
            EnsureMetrics();
            int idx = characterIndex + (trailingSideOfCharacter ? 1 : 0);
            idx = Math.Clamp(idx, 0, _cumulativeX!.Length - 1);
            return new System.Numerics.Vector2(_cumulativeX[idx], 0f);
        }

        /// <summary>Hit-tests a point, returning the character region under it.</summary>
        public CanvasTextLayoutRegion HitTest(float x, float y)
        {
            EnsureMetrics();
            int len = _text.Length;
            var region = new CanvasTextLayoutRegion { CharacterIndex = 0, CharacterCount = 0, LayoutBounds = new Rect(0, 0, 0, LineHeight) };
            if (len == 0)
                return region;

            // Find the character whose [leading, trailing) advance span contains x.
            for (int i = 0; i < len; i++)
            {
                // Skip the trailing unit of a surrogate pair — it shares the lead unit's span.
                int next = i + 1;
                while (next < len && char.IsLowSurrogate(_text[next]))
                    next++;

                float left = _cumulativeX![i];
                float right = _cumulativeX[next];
                if (x < right || next >= len)
                {
                    region.CharacterIndex = i;
                    region.CharacterCount = next - i;
                    region.LayoutBounds = new Rect(left, 0, Math.Max(0, right - left), LineHeight);
                    return region;
                }
                i = next - 1;
            }

            // Past the end — caret after the last character.
            region.CharacterIndex = Math.Max(0, len - 1);
            region.CharacterCount = 0;
            region.LayoutBounds = new Rect(_cumulativeX![len], 0, 0, LineHeight);
            return region;
        }

        /// <summary>Returns the bounding regions of a character range.</summary>
        public CanvasTextLayoutRegion[] GetCharacterRegions(int characterIndex, int characterCount)
        {
            EnsureMetrics();
            int len = _text.Length;
            int start = Math.Clamp(characterIndex, 0, len);
            int end = Math.Clamp(characterIndex + characterCount, start, len);
            if (end <= start)
                return Array.Empty<CanvasTextLayoutRegion>();

            float left = _cumulativeX![start];
            float right = _cumulativeX[end];
            return new[]
            {
                new CanvasTextLayoutRegion
                {
                    CharacterIndex = start,
                    CharacterCount = end - start,
                    LayoutBounds = new Rect(left, 0, Math.Max(0, right - left), LineHeight),
                }
            };
        }

        private int CountTrailingWhitespace()
        {
            int count = 0;
            for (int i = _text.Length - 1; i >= 0 && char.IsWhiteSpace(_text[i]); i--)
                count++;
            return count;
        }

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
