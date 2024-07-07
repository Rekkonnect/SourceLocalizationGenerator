namespace SourceLocalizationGenerator.SourceGenerators;

public static class WellKnownNames
{
    public static class Metadata
    {
        public const string BaseNamespace =
            "SourceLocalizationGenerator.Core";

        public const string
            GeneratedLocalizationStringAttribute
                = $"{BaseNamespace}.GeneratedLocalizationStringAttribute",
            LocalizationStringResourceNameAttribute
                = $"{BaseNamespace}.LocalizationStringResourceNameAttribute",
            LocalizationStringResourceValueAttribute
                = $"{BaseNamespace}.LocalizationStringResourceValueAttribute",
            LocalizationStringResourceContainerAttribute
                = $"{BaseNamespace}.LocalizationStringResourceContainerAttribute",
            ILocalizationStringAttribute
                = $"{BaseNamespace}.ILocalizationStringAttribute"
            ;
    }
}
