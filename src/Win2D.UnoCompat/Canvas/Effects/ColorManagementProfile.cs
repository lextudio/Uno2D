namespace Microsoft.Graphics.Canvas.Effects
{
    public enum CanvasColorSpace
    {
        Custom = 0,
        Srgb = 1,
        ScRgb = 2,
    }

    public sealed class ColorManagementProfile
    {
        public ColorManagementProfile(CanvasColorSpace colorSpace)
        {
            ColorSpace = colorSpace;
        }

        public CanvasColorSpace ColorSpace { get; }
    }
}
