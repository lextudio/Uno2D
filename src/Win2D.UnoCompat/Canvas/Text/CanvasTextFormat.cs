using SkiaSharp;
using System;
using System.Collections.Concurrent;

namespace Microsoft.Graphics.Canvas.Text
{
    public sealed class CanvasTextFormat : IDisposable
    {
        private static readonly ConcurrentDictionary<string, SKTypeface> TypefaceCache = new(StringComparer.OrdinalIgnoreCase);

        // ── Core properties (existing) ─────────────────────────────────

        public string? FontFamily { get; set; }
        public float FontSize { get; set; } = 20f;
        public CanvasHorizontalAlignment HorizontalAlignment { get; set; }
        public CanvasVerticalAlignment VerticalAlignment { get; set; }
        public CanvasWordWrapping WordWrapping { get; set; }

        // ── Font properties (new) ──────────────────────────────────────

        public CanvasFontWeight FontWeight { get; set; } = CanvasFontWeight.Normal;
        public CanvasFontStretch FontStretch { get; set; } = CanvasFontStretch.Normal;
        public CanvasFontStyle FontStyle { get; set; } = CanvasFontStyle.Normal;
        public string? Locale { get; set; }

        // ── Line properties (new) ──────────────────────────────────────

        public float LineSpacing { get; set; }
        public CanvasLineSpacingMethod LineSpacingMethod { get; set; }
        public bool OpticalAlignment { get; set; }

        // ── Trimming (new) ─────────────────────────────────────────────

        public CanvasTrimmingGranularity TrimmingGranularity { get; set; }
        public string? TrimmingDelimiter { get; set; }
        public int TrimmingDelimiterCount { get; set; } = 1;

        // ── Direction / spacing (new) ──────────────────────────────────

        public CanvasTextDirection Direction { get; set; } = CanvasTextDirection.LeftToRight;
        public float WordSpacing { get; set; }
        public float LetterSpacing { get; set; }

        // ── Paragraph (new) ────────────────────────────────────────────

        public CanvasParagraphAlignment ParagraphAlignment { get; set; } = CanvasParagraphAlignment.Near;

        // ── Additional formatting ──────────────────────────────────────

        public float IncrementalTabStop { get; set; } = 48f;

        public bool LastLineWrapping { get; set; } = true;

        public float LineSpacingBaseline { get; set; }

        public CanvasLineSpacingMode LineSpacingMode { get; set; } = CanvasLineSpacingMode.Default;

        public CanvasDrawTextOptions Options { get; set; }

        public CanvasTrimmingSign TrimmingSign { get; set; } = CanvasTrimmingSign.Ellipsis;

        public string? CustomTrimmingSign { get; set; }

        public CanvasVerticalGlyphOrientation VerticalGlyphOrientation { get; set; } = CanvasVerticalGlyphOrientation.Default;

        public static string[] GetSystemFontFamilies()
        {
            return SkiaSharp.SKFontManager.Default.GetFontFamilies();
        }

        // ── Font resolution ────────────────────────────────────────────

        public SKTypeface ResolveTypeface()
        {
            string family = FontFamily ?? string.Empty;
            string cacheKey = $"{family}|{FontWeight.Weight}|{(int)FontStretch}|{(int)FontStyle}|{Locale}";

            if (TypefaceCache.TryGetValue(cacheKey, out SKTypeface? cached))
                return cached;

            SKTypeface resolved = SKTypeface.Default;

            try
            {
                string familyName = family;
                string? explicitFamilyName = null;

                int hashIndex = family.LastIndexOf('#');
                if (hashIndex >= 0)
                {
                    explicitFamilyName = family[(hashIndex + 1)..];
                    familyName = family[..hashIndex];
                }

                var fontStyle = new SKFontStyle(
                    ToSkFontStyleWeight(FontWeight.Weight),
                    FontStretch switch
                    {
                        CanvasFontStretch.UltraCondensed => SKFontStyleWidth.UltraCondensed,
                        CanvasFontStretch.ExtraCondensed => SKFontStyleWidth.ExtraCondensed,
                        CanvasFontStretch.Condensed => SKFontStyleWidth.Condensed,
                        CanvasFontStretch.SemiCondensed => SKFontStyleWidth.SemiCondensed,
                        CanvasFontStretch.Normal => SKFontStyleWidth.Normal,
                        CanvasFontStretch.SemiExpanded => SKFontStyleWidth.SemiExpanded,
                        CanvasFontStretch.Expanded => SKFontStyleWidth.Expanded,
                        CanvasFontStretch.ExtraExpanded => SKFontStyleWidth.ExtraExpanded,
                        CanvasFontStretch.UltraExpanded => SKFontStyleWidth.UltraExpanded,
                        _ => SKFontStyleWidth.Normal,
                    },
                    FontStyle switch
                    {
                        CanvasFontStyle.Italic => SKFontStyleSlant.Italic,
                        CanvasFontStyle.Oblique => SKFontStyleSlant.Oblique,
                        _ => SKFontStyleSlant.Upright,
                    });

                if (familyName.Contains("ms-appx:///", StringComparison.OrdinalIgnoreCase)
                    || familyName.Contains("file://", StringComparison.OrdinalIgnoreCase))
                {
                    string path = familyName
                        .Replace("ms-appx:///", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .Replace("file://", string.Empty, StringComparison.OrdinalIgnoreCase)
                        .TrimStart('/', '\\');

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
                    SKTypeface? named = SKTypeface.FromFamilyName(explicitFamilyName, fontStyle);
                    if (named is not null)
                        resolved = named;
                }

                if (ReferenceEquals(resolved, SKTypeface.Default))
                    resolved = SKTypeface.FromFamilyName(familyName, fontStyle) ?? SKTypeface.Default;
            }
            catch
            {
            }

            return TypefaceCache.GetOrAdd(cacheKey, resolved);
        }

        public string ApplyTrimming(string text, float maxWidth)
        {
            if (string.IsNullOrEmpty(text)
                || float.IsInfinity(maxWidth)
                || maxWidth <= 0
                || TrimmingGranularity == CanvasTrimmingGranularity.None)
            {
                return text;
            }

            string delimiter = string.IsNullOrEmpty(TrimmingDelimiter)
                ? "\u2026"
                : string.Concat(Enumerable.Repeat(TrimmingDelimiter, Math.Max(1, TrimmingDelimiterCount)));

            using SKFont font = new(ResolveTypeface(), FontSize);
            if (MeasureTextWithSpacing(font, text) <= maxWidth)
                return text;

            if (MeasureTextWithSpacing(font, delimiter) > maxWidth)
                return string.Empty;

            string candidate = text;
            while (candidate.Length > 0)
            {
                candidate = TrimmingGranularity == CanvasTrimmingGranularity.Word
                    ? TrimOneWord(candidate)
                    : candidate[..^1];

                string trimmed = candidate + delimiter;
                if (MeasureTextWithSpacing(font, trimmed) <= maxWidth)
                    return trimmed;
            }

            return delimiter;
        }

        internal float MeasureTextWithSpacing(SKFont font, string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0f;

            ushort[] glyphs = font.GetGlyphs(text);
            float[] widths = font.GetGlyphWidths(glyphs);
            float width = widths.Sum();

            if (LetterSpacing != 0 && text.Length > 1)
                width += LetterSpacing * (text.Length - 1);

            if (WordSpacing != 0)
                width += WordSpacing * text.Count(char.IsWhiteSpace);

            return width;
        }

        private static string TrimOneWord(string text)
        {
            string trimmed = text.TrimEnd();
            int lastWhitespace = trimmed.LastIndexOfAny(new[] { ' ', '\t', '\r', '\n' });
            return lastWhitespace <= 0 ? string.Empty : trimmed[..lastWhitespace].TrimEnd();
        }

        private static SKFontStyleWeight ToSkFontStyleWeight(int weight)
        {
            return weight switch
            {
                <= 100 => SKFontStyleWeight.Thin,
                <= 200 => SKFontStyleWeight.ExtraLight,
                <= 300 => SKFontStyleWeight.Light,
                <= 350 => SKFontStyleWeight.Light,
                <= 400 => SKFontStyleWeight.Normal,
                <= 500 => SKFontStyleWeight.Medium,
                <= 600 => SKFontStyleWeight.SemiBold,
                <= 700 => SKFontStyleWeight.Bold,
                <= 800 => SKFontStyleWeight.ExtraBold,
                <= 900 => SKFontStyleWeight.Black,
                _ => SKFontStyleWeight.Black,
            };
        }

        public void Dispose()
        {
        }
    }
}
