namespace Microsoft.Graphics.Canvas.Text
{
    // Win2D's CanvasFontWeight is a struct with a single Weight property.
    public struct CanvasFontWeight
    {
        public int Weight { get; set; }

        public static CanvasFontWeight Thin => new() { Weight = 100 };
        public static CanvasFontWeight ExtraLight => new() { Weight = 200 };
        public static CanvasFontWeight Light => new() { Weight = 300 };
        public static CanvasFontWeight SemiLight => new() { Weight = 350 };
        public static CanvasFontWeight Normal => new() { Weight = 400 };
        public static CanvasFontWeight Medium => new() { Weight = 500 };
        public static CanvasFontWeight SemiBold => new() { Weight = 600 };
        public static CanvasFontWeight Bold => new() { Weight = 700 };
        public static CanvasFontWeight ExtraBold => new() { Weight = 800 };
        public static CanvasFontWeight Black => new() { Weight = 900 };
        public static CanvasFontWeight ExtraBlack => new() { Weight = 950 };
    }
}
