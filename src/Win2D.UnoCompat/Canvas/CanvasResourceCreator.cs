namespace Microsoft.Graphics.Canvas
{
    public interface ICanvasResourceCreator
    {
        CanvasDevice Device { get; }
    }

    public interface ICanvasResourceCreatorWithDpi : ICanvasResourceCreator
    {
        float Dpi { get; }
        int ConvertDipsToPixels(float dips, CanvasDpiRounding dpiRounding);
        float ConvertPixelsToDips(int pixels);
    }
}
