using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas.Typography;

namespace Microsoft.Graphics.Canvas.Text
{
    public sealed class CanvasTextLayout : IDisposable
    {
        private readonly string _text;
        private readonly CanvasTextFormat _textFormat;
        private readonly float _maxWidth;
        private readonly float _maxHeight;

        private float[]? _cumulativeX;
        private CanvasClusterMetrics[]? _clusterMetrics;
        private float _ascent;
        private float _descent;
        private float _leading;
        private readonly List<RangeValue<float>> _fontSizeOverrides = new();
        private readonly List<RangeValue<string>> _localeOverrides = new();
        private readonly List<RangeValue<bool>> _strikethroughOverrides = new();
        private readonly List<RangeValue<bool>> _underlineOverrides = new();
        private readonly List<RangeValue<object?>> _drawingEffectOverrides = new();
        private readonly List<RangeValue<string>> _fontFamilyOverrides = new();
        private readonly List<RangeValue<CanvasFontWeight>> _fontWeightOverrides = new();
        private readonly List<RangeValue<CanvasFontStretch>> _fontStretchOverrides = new();
        private readonly List<RangeValue<CanvasFontStyle>> _fontStyleOverrides = new();
        private readonly List<RangeValue<bool>> _pairKerningOverrides = new();
        private readonly List<RangeValue<float>> _leadingSpacingOverrides = new();
        private readonly List<RangeValue<float>> _trailingSpacingOverrides = new();
        private readonly List<RangeValue<CanvasTypography>> _typographyOverrides = new();

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
        public Size RequestedSize => new(_maxWidth, _maxHeight);
        public Size MinimumSize => new(TotalWidth, LineHeight);
        public object? DrawingEffect { get; set; }

        public int LineCount => 1;

        public int MaximumBidiReorderingDepth => 0;

        public CanvasHorizontalAlignment HorizontalAlignment => _textFormat.HorizontalAlignment;

        public CanvasVerticalAlignment VerticalAlignment => _textFormat.VerticalAlignment;

        public CanvasTextDirection Direction => _textFormat.Direction;

        public CanvasWordWrapping WordWrapping => _textFormat.WordWrapping;

        public string DefaultFontFamily => _textFormat.FontFamily ?? string.Empty;

        public float DefaultFontSize => _textFormat.FontSize;

        public CanvasFontStretch DefaultFontStretch => _textFormat.FontStretch;

        public CanvasFontStyle DefaultFontStyle => _textFormat.FontStyle;

        public CanvasFontWeight DefaultFontWeight => _textFormat.FontWeight;

        public string DefaultLocaleName => _textFormat.Locale ?? string.Empty;

        public float IncrementalTabStop
        {
            get => _textFormat.IncrementalTabStop;
            set => _textFormat.IncrementalTabStop = value;
        }

        public bool LastLineWrapping
        {
            get => _textFormat.LastLineWrapping;
            set => _textFormat.LastLineWrapping = value;
        }

        public float LineSpacing
        {
            get => _textFormat.LineSpacing;
            set => _textFormat.LineSpacing = value;
        }

        public float LineSpacingBaseline
        {
            get => _textFormat.LineSpacingBaseline;
            set => _textFormat.LineSpacingBaseline = value;
        }

        public CanvasLineSpacingMode LineSpacingMode
        {
            get => _textFormat.LineSpacingMode;
            set => _textFormat.LineSpacingMode = value;
        }

        public bool OpticalAlignment
        {
            get => _textFormat.OpticalAlignment;
            set => _textFormat.OpticalAlignment = value;
        }

        public CanvasDrawTextOptions Options
        {
            get => _textFormat.Options;
            set => _textFormat.Options = value;
        }

        public CanvasTrimmingGranularity TrimmingGranularity
        {
            get => _textFormat.TrimmingGranularity;
            set => _textFormat.TrimmingGranularity = value;
        }

        public string TrimmingDelimiter
        {
            get => _textFormat.TrimmingDelimiter ?? string.Empty;
            set => _textFormat.TrimmingDelimiter = value;
        }

        public int TrimmingDelimiterCount
        {
            get => _textFormat.TrimmingDelimiterCount;
            set => _textFormat.TrimmingDelimiterCount = value;
        }

        public CanvasTrimmingSign TrimmingSign
        {
            get => _textFormat.TrimmingSign;
            set => _textFormat.TrimmingSign = value;
        }

        public string CustomTrimmingSign
        {
            get => _textFormat.CustomTrimmingSign ?? string.Empty;
            set => _textFormat.CustomTrimmingSign = value;
        }

        public CanvasVerticalGlyphOrientation VerticalGlyphOrientation
        {
            get => _textFormat.VerticalGlyphOrientation;
            set => _textFormat.VerticalGlyphOrientation = value;
        }

        public CanvasDevice Device => CanvasDevice.GetSharedDevice();

        private void EnsureMetrics()
        {
            if (_cumulativeX is not null)
                return;

            using SKFont font = new SKFont(_textFormat.ResolveTypeface(), _textFormat.FontSize);
            SKFontMetrics fontMetrics = font.Metrics;
            _ascent = -fontMetrics.Ascent;
            _descent = fontMetrics.Descent;
            _leading = fontMetrics.Leading;

            int len = _text.Length;
            var cumulative = new float[len + 1];
            if (len == 0)
            {
                _cumulativeX = cumulative;
                return;
            }

            ushort[] glyphs = font.GetGlyphs(_text);
            float[] widths = font.GetGlyphWidths(glyphs);

            int glyphIndex = 0;
            float x = 0f;
            for (int i = 0; i < len; i++)
            {
                cumulative[i] = x;
                if (char.IsLowSurrogate(_text[i]))
                    continue;

                float advance = glyphIndex < widths.Length ? widths[glyphIndex] : 0f;
                glyphIndex++;
                if (i < len - 1)
                    advance += _textFormat.LetterSpacing;
                if (char.IsWhiteSpace(_text[i]))
                    advance += _textFormat.WordSpacing;
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
            get
            {
                EnsureMetrics();
                if (_textFormat.LineSpacingMethod == CanvasLineSpacingMethod.Uniform && _textFormat.LineSpacing > 0)
                    return _textFormat.LineSpacing;

                return _ascent + _descent + _leading;
            }
        }

        public Rect LayoutBounds
        {
            get { EnsureMetrics(); return new Rect(0, 0, TotalWidth, LineHeight); }
        }

        public Rect LayoutBoundsIncludingTrailingWhitespace
        {
            get { EnsureMetrics(); return new Rect(0, 0, TotalWidth, LineHeight); }
        }

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

        public CanvasClusterMetrics[] ClusterMetrics
        {
            get
            {
                EnsureClusterMetrics();
                return _clusterMetrics!.ToArray();
            }
        }

        public float GetFontSize(int characterIndex)
        {
            ValidateCharacterIndex(characterIndex);
            return GetRangeValue(_fontSizeOverrides, characterIndex, _textFormat.FontSize);
        }

        public void SetFontSize(int characterIndex, int characterCount, float fontSize)
        {
            if (fontSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(fontSize));

            SetRangeValue(_fontSizeOverrides, characterIndex, characterCount, fontSize);
            InvalidateMetrics();
        }

        public string GetLocale(int characterIndex)
        {
            ValidateCharacterIndex(characterIndex);
            return GetRangeValue(_localeOverrides, characterIndex, _textFormat.Locale ?? string.Empty);
        }

        public void SetLocale(int characterIndex, int characterCount, string locale)
        {
            ArgumentNullException.ThrowIfNull(locale);
            SetRangeValue(_localeOverrides, characterIndex, characterCount, locale);
        }

        public bool GetStrikethrough(int characterIndex)
        {
            ValidateCharacterIndex(characterIndex);
            return GetRangeValue(_strikethroughOverrides, characterIndex, false);
        }

        public void SetStrikethrough(int characterIndex, int characterCount, bool strikethrough)
        {
            SetRangeValue(_strikethroughOverrides, characterIndex, characterCount, strikethrough);
        }

        public bool GetUnderline(int characterIndex)
        {
            ValidateCharacterIndex(characterIndex);
            return GetRangeValue(_underlineOverrides, characterIndex, false);
        }

        public void SetUnderline(int characterIndex, int characterCount, bool underline)
        {
            SetRangeValue(_underlineOverrides, characterIndex, characterCount, underline);
        }

        public object? GetDrawingEffect(int characterIndex)
        {
            ValidateCharacterIndex(characterIndex);
            return GetRangeValue(_drawingEffectOverrides, characterIndex, DrawingEffect);
        }

        public void SetDrawingEffect(int characterIndex, int characterCount, object? drawingEffect)
        {
            SetRangeValue(_drawingEffectOverrides, characterIndex, characterCount, drawingEffect);
        }

        public void SetBrush(int characterIndex, int characterCount, object? brush)
        {
            SetRangeValue(_drawingEffectOverrides, characterIndex, characterCount, brush);
        }

        public object? GetBrush(int characterIndex)
        {
            return GetDrawingEffect(characterIndex);
        }

        public void SetCustomBrush(int characterIndex, int characterCount, object? brush)
        {
            SetRangeValue(_drawingEffectOverrides, characterIndex, characterCount, brush);
        }

        public object? GetCustomBrush(int characterIndex)
        {
            return GetDrawingEffect(characterIndex);
        }

        public string GetFontFamily(int characterIndex)
        {
            ValidateCharacterIndex(characterIndex);
            return GetRangeValue(_fontFamilyOverrides, characterIndex, DefaultFontFamily);
        }

        public void SetFontFamily(int characterIndex, int characterCount, string fontFamily)
        {
            ArgumentNullException.ThrowIfNull(fontFamily);
            SetRangeValue(_fontFamilyOverrides, characterIndex, characterCount, fontFamily);
        }

        public CanvasFontWeight GetFontWeight(int characterIndex)
        {
            ValidateCharacterIndex(characterIndex);
            return GetRangeValue(_fontWeightOverrides, characterIndex, DefaultFontWeight);
        }

        public void SetFontWeight(int characterIndex, int characterCount, CanvasFontWeight fontWeight)
        {
            SetRangeValue(_fontWeightOverrides, characterIndex, characterCount, fontWeight);
        }

        public CanvasFontStretch GetFontStretch(int characterIndex)
        {
            ValidateCharacterIndex(characterIndex);
            return GetRangeValue(_fontStretchOverrides, characterIndex, DefaultFontStretch);
        }

        public void SetFontStretch(int characterIndex, int characterCount, CanvasFontStretch fontStretch)
        {
            SetRangeValue(_fontStretchOverrides, characterIndex, characterCount, fontStretch);
        }

        public CanvasFontStyle GetFontStyle(int characterIndex)
        {
            ValidateCharacterIndex(characterIndex);
            return GetRangeValue(_fontStyleOverrides, characterIndex, DefaultFontStyle);
        }

        public void SetFontStyle(int characterIndex, int characterCount, CanvasFontStyle fontStyle)
        {
            SetRangeValue(_fontStyleOverrides, characterIndex, characterCount, fontStyle);
        }

        public bool GetPairKerning(int characterIndex)
        {
            ValidateCharacterIndex(characterIndex);
            return GetRangeValue(_pairKerningOverrides, characterIndex, true);
        }

        public void SetPairKerning(int characterIndex, int characterCount, bool pairKerning)
        {
            SetRangeValue(_pairKerningOverrides, characterIndex, characterCount, pairKerning);
        }

        public float GetLeadingCharacterSpacing(int characterIndex)
        {
            ValidateCharacterIndex(characterIndex);
            return GetRangeValue(_leadingSpacingOverrides, characterIndex, 0f);
        }

        public float GetTrailingCharacterSpacing(int characterIndex)
        {
            ValidateCharacterIndex(characterIndex);
            return GetRangeValue(_trailingSpacingOverrides, characterIndex, 0f);
        }

        public void SetCharacterSpacing(int characterIndex, int characterCount, float leadingSpacing, float trailingSpacing, float minimumAdvance)
        {
            ValidateRange(characterIndex, characterCount);
            SetRangeValue(_leadingSpacingOverrides, characterIndex, characterCount, leadingSpacing);
            SetRangeValue(_trailingSpacingOverrides, characterIndex, characterCount, trailingSpacing);
        }

        public int[] GetFormatChangeIndices()
        {
            var indices = new HashSet<int> { 0 };
            foreach (var range in _fontSizeOverrides) { indices.Add(range.Start); indices.Add(range.End); }
            foreach (var range in _localeOverrides) { indices.Add(range.Start); indices.Add(range.End); }
            foreach (var range in _fontFamilyOverrides) { indices.Add(range.Start); indices.Add(range.End); }
            foreach (var range in _fontWeightOverrides) { indices.Add(range.Start); indices.Add(range.End); }
            foreach (var range in _fontStretchOverrides) { indices.Add(range.Start); indices.Add(range.End); }
            foreach (var range in _fontStyleOverrides) { indices.Add(range.Start); indices.Add(range.End); }
            foreach (var range in _drawingEffectOverrides) { indices.Add(range.Start); indices.Add(range.End); }
            var sorted = indices.OrderBy(i => i).ToArray();
            return sorted;
        }

        public System.Numerics.Matrix3x2 GetGlyphOrientationTransform(int characterIndex)
        {
            return System.Numerics.Matrix3x2.Identity;
        }

        public float GetMinimumCharacterAdvance(int characterIndex, int characterCount)
        {
            EnsureMetrics();
            if (_text.Length == 0) return 0f;
            int end = Math.Min(characterIndex + characterCount, _text.Length);
            return (_cumulativeX![end] - _cumulativeX[characterIndex]) / Math.Max(1, characterCount);
        }

        public float GetMinimumLineLength()
        {
            return TotalWidth;
        }

        public void SetInlineObject(int characterIndex, int characterCount, object? inlineObject)
        {
        }

        public object? GetInlineObject(int characterIndex)
        {
            return null;
        }

        public CanvasTypography GetTypography(int characterIndex)
        {
            ValidateCharacterIndex(characterIndex);
            return GetRangeValue(_typographyOverrides, characterIndex, new CanvasTypography());
        }

        public void SetTypography(int characterIndex, int characterCount, CanvasTypography typography)
        {
            ArgumentNullException.ThrowIfNull(typography);
            SetRangeValue(_typographyOverrides, characterIndex, characterCount, typography);
        }

        public void DrawToTextRenderer(CanvasDrawingSession drawingSession, float x, float y)
        {
            ArgumentNullException.ThrowIfNull(drawingSession);
            drawingSession.DrawTextLayout(this, x, y, Windows.UI.Color.FromArgb(255, 0, 0, 0));
        }

        public System.Numerics.Vector2 GetCaretPosition(int characterIndex, bool trailingSideOfCharacter)
        {
            EnsureMetrics();
            int idx = characterIndex + (trailingSideOfCharacter ? 1 : 0);
            idx = Math.Clamp(idx, 0, _cumulativeX!.Length - 1);
            return new System.Numerics.Vector2(_cumulativeX[idx], 0f);
        }

        public CanvasTextLayoutRegion HitTest(float x, float y)
        {
            EnsureMetrics();
            int len = _text.Length;
            var region = new CanvasTextLayoutRegion { CharacterIndex = 0, CharacterCount = 0, LayoutBounds = new Rect(0, 0, 0, LineHeight) };
            if (len == 0)
                return region;

            for (int i = 0; i < len; i++)
            {
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

            region.CharacterIndex = Math.Max(0, len - 1);
            region.CharacterCount = 0;
            region.LayoutBounds = new Rect(_cumulativeX![len], 0, 0, LineHeight);
            return region;
        }

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

        public void DrawToBitmap(byte[] pixels, int x, int y, int width, int height)
        {
            ArgumentNullException.ThrowIfNull(pixels);
            if (width <= 0)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0)
                throw new ArgumentOutOfRangeException(nameof(height));

            int requiredBytes = checked(width * height * 4);
            if (pixels.Length < requiredBytes)
                throw new ArgumentException("The destination buffer is too small.", nameof(pixels));

            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            using SKSurface surface = SKSurface.Create(info)
                ?? throw new InvalidOperationException("Unable to create Skia surface.");
            surface.Canvas.Clear(SKColors.Transparent);

            using SKFont font = new(_textFormat.ResolveTypeface(), _textFormat.FontSize);
            using var paint = new SKPaint
            {
                Color = SKColors.Black,
                IsAntialias = true,
            };

            EnsureMetrics();
            surface.Canvas.DrawText(_text, x, y + _ascent, font, paint);
            surface.Canvas.Flush();

            using SKPixmap pixmap = surface.PeekPixels()
                ?? throw new InvalidOperationException("Unable to access rendered pixels.");
            Marshal.Copy(pixmap.GetPixels(), pixels, 0, requiredBytes);
        }

        private void EnsureClusterMetrics()
        {
            if (_clusterMetrics is not null)
                return;

            EnsureMetrics();
            int len = _text.Length;
            if (len == 0)
            {
                _clusterMetrics = Array.Empty<CanvasClusterMetrics>();
                return;
            }

            var clusters = new List<CanvasClusterMetrics>();
            for (int i = 0; i < len; i++)
            {
                int next = i + 1;
                while (next < len && char.IsLowSurrogate(_text[next]))
                    next++;

                string clusterText = _text.Substring(i, next - i);
                float left = _cumulativeX![i];
                float right = _cumulativeX[next];
                bool isWhitespace = clusterText.All(char.IsWhiteSpace);
                bool isNewline = clusterText.Contains('\n') || clusterText.Contains('\r');
                clusters.Add(new CanvasClusterMetrics
                {
                    CharacterIndex = i,
                    CharacterCount = next - i,
                    GlyphCount = 1,
                    Width = Math.Max(0, right - left),
                    CanWrapLineAfter = isWhitespace || isNewline,
                    IsWhitespace = isWhitespace,
                    IsNewline = isNewline,
                    IsSoftHyphen = clusterText.Contains('\u00ad'),
                    IsRightToLeft = _textFormat.Direction == CanvasTextDirection.RightToLeft,
                });
                i = next - 1;
            }

            _clusterMetrics = clusters.ToArray();
        }

        private void InvalidateMetrics()
        {
            _cumulativeX = null;
            _clusterMetrics = null;
        }

        private void ValidateCharacterIndex(int characterIndex)
        {
            if (characterIndex < 0 || characterIndex > _text.Length)
                throw new ArgumentOutOfRangeException(nameof(characterIndex));
        }

        private void ValidateRange(int characterIndex, int characterCount)
        {
            if (characterIndex < 0 || characterIndex > _text.Length)
                throw new ArgumentOutOfRangeException(nameof(characterIndex));
            if (characterCount < 0 || characterIndex + characterCount > _text.Length)
                throw new ArgumentOutOfRangeException(nameof(characterCount));
        }

        private static T GetRangeValue<T>(IReadOnlyList<RangeValue<T>> ranges, int characterIndex, T fallback)
        {
            for (int i = ranges.Count - 1; i >= 0; i--)
            {
                RangeValue<T> range = ranges[i];
                if (characterIndex >= range.Start && characterIndex < range.End)
                    return range.Value;
            }

            return fallback;
        }

        private void SetRangeValue<T>(List<RangeValue<T>> ranges, int characterIndex, int characterCount, T value)
        {
            ValidateRange(characterIndex, characterCount);
            if (characterCount == 0)
                return;

            ranges.Add(new RangeValue<T>(characterIndex, characterIndex + characterCount, value));
        }

        public void Dispose()
        {
        }

        private readonly record struct RangeValue<T>(int Start, int End, T Value);
    }
}
