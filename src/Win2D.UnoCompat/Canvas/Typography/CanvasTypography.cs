namespace Microsoft.Graphics.Canvas.Typography
{
    public enum CanvasTypographyFeatureName
    {
        Kerning,
        StandardLigatures,
        DiscretionaryLigatures,
        StylisticSet1,
        StylisticSet2,
        StylisticSet3,
    }

    public sealed class CanvasTypography
    {
        private readonly Dictionary<CanvasTypographyFeatureName, int> _features = new();
        public IReadOnlyDictionary<CanvasTypographyFeatureName, int> Features => _features;
        public void AddFeature(CanvasTypographyFeatureName featureName, int value) => _features[featureName] = value;
        public bool TryGetFeature(CanvasTypographyFeatureName featureName, out int value) => _features.TryGetValue(featureName, out value);
    }
}
