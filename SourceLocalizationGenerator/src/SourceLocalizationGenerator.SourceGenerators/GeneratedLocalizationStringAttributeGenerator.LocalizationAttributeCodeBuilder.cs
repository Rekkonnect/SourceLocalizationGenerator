using Dentextist;

namespace SourceLocalizationGenerator.SourceGenerators;

public sealed partial class GeneratedLocalizationStringAttributeGenerator
{
    private sealed class LocalizationAttributeCodeBuilder(AttributeGenerationArgs args)
    {
        private readonly CSharpCodeBuilder _builder = new(' ', 4);
        private readonly AttributeGenerationArgs _args = args;

        public string GenerateSupportsLanguageArms()
        {
            _builder.NestingLevel = 3;

            bool hasAppended = false;
            foreach (var mapping in _args.Model.LocalizationParameters)
            {
                if (hasAppended)
                {
                    _builder.AppendLine();
                }
                AppendSupportsLanguageArm(mapping);
                hasAppended = true;
            }
            return _builder.ToString();
        }

        private void AppendSupportsLanguageArm(ParameterLocalizationInfo mapping)
        {
            _builder.Append('"');
            _builder.Append(mapping.Language);
            _builder.Append("\" => true,");
        }

        public string GenerateLocalizationArms()
        {
            _builder.NestingLevel = 3;

            bool hasAppended = false;
            foreach (var mapping in _args.Model.LocalizationParameters)
            {
                if (hasAppended)
                {
                    _builder.AppendLine();
                }
                AppendLocalizationArm(mapping);
                hasAppended = true;
            }
            return _builder.ToString();
        }

        private void AppendLocalizationArm(ParameterLocalizationInfo mapping)
        {
            _builder.Append('"');
            _builder.Append(mapping.Language);
            _builder.Append("\" => ");
            _builder.Append(mapping.AttachedPropertyName);
            _builder.Append(',');
        }

        public string GenerateLocalizationProperties()
        {
            _builder.NestingLevel = 1;

            bool hasAppended = false;
            foreach (var mapping in _args.Model.LocalizationParameters)
            {
                if (hasAppended)
                {
                    _builder.AppendLine();
                }
                AppendLocalizationProperty(mapping);
                hasAppended = true;
            }
            return _builder.ToString();
        }

        private void AppendLocalizationProperty(ParameterLocalizationInfo mapping)
        {
            _builder.Append("public string");
            if (!mapping.Default)
            {
                _builder.Append('?');
            }
            _builder.Append(' ');
            _builder.Append(mapping.AttachedPropertyName);
            _builder.Append(" { get; } = ");
            _builder.Append(mapping.Parameter.Name);
            _builder.Append(';');
        }

        public string GenerateSupportedLanguageList()
        {
            bool hasAppended = false;
            foreach (var language in _args.Model.Languages)
            {
                if (hasAppended)
                {
                    _builder.Append(", ");
                }

                _builder.Append('"');
                _builder.Append(language);
                _builder.Append('"');
                hasAppended = true;
            }
            return _builder.ToString();
        }

        public string GenerateLocalizationResourceRecordConstructorParameters()
        {
            _builder.NestingLevel = 1;

            bool hasAppended = false;
            foreach (var mapping in _args.Model.LocalizationParameters)
            {
                if (hasAppended)
                {
                    _builder.AppendLine(',');
                }
                AppendLocalizationResourceRecordConstructorParameter(mapping);
                hasAppended = true;
            }
            return _builder.ToString();
        }

        private void AppendLocalizationResourceRecordConstructorParameter(
            ParameterLocalizationInfo mapping)
        {
            _builder.Append("string");
            if (!mapping.Default)
            {
                _builder.Append('?');
            }
            _builder.Append(' ');
            _builder.Append(mapping.AttachedPropertyName);
        }
    }
}
