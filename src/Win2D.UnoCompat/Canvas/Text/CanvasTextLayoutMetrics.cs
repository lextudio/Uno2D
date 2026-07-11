using Windows.Foundation;

namespace Microsoft.Graphics.Canvas.Text
{
    /// <summary>
    /// Describes the region occupied by a range of characters in a <see cref="CanvasTextLayout"/>.
    /// Mirrors the Win2D <c>CanvasTextLayoutRegion</c> struct so shared code is source-compatible
    /// between the SkiaSharp shim (Uno desktop) and real Win2D (WinUI 3).
    /// </summary>
    public struct CanvasTextLayoutRegion
    {
        /// <summary>Index of the first character in the region.</summary>
        public int CharacterIndex { get; set; }

        /// <summary>Number of characters in the region.</summary>
        public int CharacterCount { get; set; }

        /// <summary>The bounds, in DIPs, of the region within the layout.</summary>
        public Rect LayoutBounds { get; set; }
    }

    /// <summary>
    /// Per-line metrics for a <see cref="CanvasTextLayout"/>. Mirrors the subset of the Win2D
    /// <c>CanvasLineMetrics</c> struct that the editor relies on.
    /// </summary>
    public struct CanvasLineMetrics
    {
        /// <summary>Number of characters on the line (excluding trailing whitespace/newline accounting).</summary>
        public int CharacterCount { get; set; }

        /// <summary>Number of trailing whitespace characters on the line.</summary>
        public int TrailingWhitespaceCount { get; set; }

        /// <summary>Number of trailing newline characters on the line.</summary>
        public int TerminalNewlineCount { get; set; }

        /// <summary>The height, in DIPs, of the line.</summary>
        public float Height { get; set; }

        /// <summary>The distance, in DIPs, from the top of the line to its baseline.</summary>
        public float Baseline { get; set; }
    }

    /// <summary>
    /// Per-cluster metrics for a <see cref="CanvasTextLayout"/>.
    /// </summary>
    public struct CanvasClusterMetrics
    {
        public int CharacterIndex { get; set; }
        public int CharacterCount { get; set; }
        public int GlyphCount { get; set; }
        public float Width { get; set; }
        public bool CanWrapLineAfter { get; set; }
        public bool IsWhitespace { get; set; }
        public bool IsNewline { get; set; }
        public bool IsSoftHyphen { get; set; }
        public bool IsRightToLeft { get; set; }
    }
}
