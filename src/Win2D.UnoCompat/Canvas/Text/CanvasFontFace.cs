using SkiaSharp;

namespace Microsoft.Graphics.Canvas.Text
{
    public sealed class CanvasFontFace
    {
        internal CanvasFontFace(SKTypeface typeface)
        {
            Typeface = typeface ?? SKTypeface.Default;
        }

        internal SKTypeface Typeface { get; }

        public string FamilyName => Typeface.FamilyName;

        public int Weight => (int)Typeface.FontWeight;

        public CanvasFontStretch Stretch
        {
            get
            {
                if ((int)Typeface.FontWidth == (int)SKFontStyleWidth.UltraCondensed) return CanvasFontStretch.UltraCondensed;
                if ((int)Typeface.FontWidth == (int)SKFontStyleWidth.ExtraCondensed) return CanvasFontStretch.ExtraCondensed;
                if ((int)Typeface.FontWidth == (int)SKFontStyleWidth.Condensed) return CanvasFontStretch.Condensed;
                if ((int)Typeface.FontWidth == (int)SKFontStyleWidth.SemiCondensed) return CanvasFontStretch.SemiCondensed;
                if ((int)Typeface.FontWidth == (int)SKFontStyleWidth.Normal) return CanvasFontStretch.Normal;
                if ((int)Typeface.FontWidth == (int)SKFontStyleWidth.SemiExpanded) return CanvasFontStretch.SemiExpanded;
                if ((int)Typeface.FontWidth == (int)SKFontStyleWidth.Expanded) return CanvasFontStretch.Expanded;
                if ((int)Typeface.FontWidth == (int)SKFontStyleWidth.ExtraExpanded) return CanvasFontStretch.ExtraExpanded;
                if ((int)Typeface.FontWidth == (int)SKFontStyleWidth.UltraExpanded) return CanvasFontStretch.UltraExpanded;
                return CanvasFontStretch.Normal;
            }
        }

        public CanvasFontStyle Style => Typeface.FontSlant switch
        {
            SKFontStyleSlant.Italic => CanvasFontStyle.Italic,
            SKFontStyleSlant.Oblique => CanvasFontStyle.Oblique,
            _ => CanvasFontStyle.Normal,
        };

        public static CanvasFontFace[] GetSystemFontFamilies()
        {
            return SKFontManager.Default
                .GetFontFamilies()
                .Select(f => new CanvasFontFace(SKTypeface.FromFamilyName(f) ?? SKTypeface.Default))
                .ToArray();
        }
    }
}
