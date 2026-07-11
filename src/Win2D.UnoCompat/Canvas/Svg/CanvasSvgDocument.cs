using System.Globalization;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.UI;

namespace Microsoft.Graphics.Canvas.Svg
{
    public sealed class CanvasSvgDocument
    {
        private readonly XDocument _document;
        private readonly Size _size;

        private CanvasSvgDocument(XDocument document, Size size)
        {
            _document = document;
            _size = size;
        }

        public static async Task<CanvasSvgDocument> LoadAsync(Stream stream)
        {
            ArgumentNullException.ThrowIfNull(stream);
            XDocument document = await XDocument.LoadAsync(stream, LoadOptions.None, CancellationToken.None).ConfigureAwait(false);
            XElement root = document.Root ?? throw new InvalidOperationException("SVG document has no root element.");
            double width = ParseDouble(root.Attribute("width")?.Value, 0);
            double height = ParseDouble(root.Attribute("height")?.Value, 0);
            if ((width <= 0 || height <= 0) && root.Attribute("viewBox")?.Value is { } viewBox)
            {
                string[] parts = viewBox.Split([' ', ','], StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 4)
                {
                    width = ParseDouble(parts[2], width);
                    height = ParseDouble(parts[3], height);
                }
            }

            return new CanvasSvgDocument(document, new Size(width, height));
        }

        public Size GetSize() => _size;

        public void Draw(CanvasDrawingSession drawingSession, Rect destinationRect)
        {
            ArgumentNullException.ThrowIfNull(drawingSession);
            XElement root = _document.Root ?? throw new InvalidOperationException("SVG document has no root element.");
            foreach (XElement element in root.Elements())
            {
                string name = element.Name.LocalName;
                if (name == "rect")
                {
                    var rect = new Rect(
                        ParseDouble(element.Attribute("x")?.Value, 0) + destinationRect.X,
                        ParseDouble(element.Attribute("y")?.Value, 0) + destinationRect.Y,
                        ParseDouble(element.Attribute("width")?.Value, 0),
                        ParseDouble(element.Attribute("height")?.Value, 0));
                    drawingSession.FillRectangle(rect.XFloat(), rect.YFloat(), rect.WidthFloat(), rect.HeightFloat(), ParseColor(element.Attribute("fill")?.Value));
                }
                else if (name == "circle")
                {
                    drawingSession.FillCircle(
                        (float)(destinationRect.X + ParseDouble(element.Attribute("cx")?.Value, 0)),
                        (float)(destinationRect.Y + ParseDouble(element.Attribute("cy")?.Value, 0)),
                        (float)ParseDouble(element.Attribute("r")?.Value, 0),
                        ParseColor(element.Attribute("fill")?.Value));
                }
            }
        }

        private static double ParseDouble(string? value, double fallback)
        {
            if (string.IsNullOrWhiteSpace(value))
                return fallback;
            value = value.Trim().TrimEnd('p', 'x');
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result) ? result : fallback;
        }

        private static Color ParseColor(string? value)
        {
            if (string.IsNullOrWhiteSpace(value) || value == "none")
                return Color.FromArgb(0, 0, 0, 0);
            if (value.StartsWith("#", StringComparison.Ordinal) && value.Length == 7)
                return Color.FromArgb(255, Convert.ToByte(value[1..3], 16), Convert.ToByte(value[3..5], 16), Convert.ToByte(value[5..7], 16));
            return value.ToLowerInvariant() switch
            {
                "red" => Color.FromArgb(255, 255, 0, 0),
                "green" => Color.FromArgb(255, 0, 128, 0),
                "blue" => Color.FromArgb(255, 0, 0, 255),
                "black" => Color.FromArgb(255, 0, 0, 0),
                "white" => Color.FromArgb(255, 255, 255, 255),
                _ => Color.FromArgb(255, 0, 0, 0),
            };
        }
    }

    internal static class RectExtensions
    {
        public static float XFloat(this Rect rect) => (float)rect.X;
        public static float YFloat(this Rect rect) => (float)rect.Y;
        public static float WidthFloat(this Rect rect) => (float)rect.Width;
        public static float HeightFloat(this Rect rect) => (float)rect.Height;
    }
}
