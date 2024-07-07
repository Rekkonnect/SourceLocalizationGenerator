using Dentextist;
using Microsoft.CodeAnalysis;
using System.Linq;

namespace SourceLocalizationGenerator.SourceGenerators;

public sealed partial class LocalizationStringResourceContainerGenerator
{
    private class LocalizationContainerCodeBuilder(AttributeGenerationArgs args)
    {
        private readonly CSharpCodeBuilder _builder = new(' ', 4);
        private readonly AttributeGenerationArgs _args = args;

        public string GeneratePropertyChangedFields()
        {
            _builder.NestingLevel = 1;

            bool hasAppended = false;
            foreach (var resource in _args.Model.LocalizationModels)
            {
                if (resource.HasNullNonDefaultResources)
                {
                    continue;
                }

                if (hasAppended)
                {
                    _builder.AppendLine();
                }
                AppendPropertyChangedField(resource);
                hasAppended = true;
            }
            return _builder.ToString();
        }

        private void AppendPropertyChangedField(LocalizationResourceAttributeInstanceModel resource)
        {
            _builder.Append("private static readonly PropertyChangedEventArgs _");
            _builder.Append(resource.ResourceName.CamelCase());
            _builder.Append("Changed = new(nameof(");
            _builder.Append(resource.ResourceName);
            _builder.Append("));");
        }

        public string GenerateResourceFields()
        {
            _builder.NestingLevel = 1;

            bool hasAppended = false;
            foreach (var resource in _args.Model.LocalizationModels)
            {
                if (hasAppended)
                {
                    _builder.AppendLine();
                }
                AppendResourceProperty(resource);
                hasAppended = true;
            }
            return _builder.ToString();
        }

        private void AppendResourceProperty(LocalizationResourceAttributeInstanceModel resource)
        {
            _builder.Append("public ");
            _builder.Append(resource.AttributeModel.AssociatedResourceTypeName);
            _builder.Append(' ');
            _builder.Append(resource.ResourceName);
            _builder.Append(" { get; } = new(");

            var values = resource.Values;
            bool hasAppended = false;
            for (int i = 0; i < values.Length; i++)
            {
                if (hasAppended)
                {
                    _builder.Append(", ");
                }
                AppendValue(values[i].Value);
                hasAppended = true;
            }

            _builder.Append(");");
        }

        private void AppendValue(string? value)
        {
            if (value is null)
            {
                _builder.Append("null");
            }
            else
            {
                var stringLiteral = LiteralConversion.QuoteSnippetStringCStyle(value);
                _builder.Append(stringLiteral);
            }
        }

        public string GenerateLocalizationCases()
        {
            _builder.NestingLevel = 3;

            bool hasAppended = false;
            foreach (var language in _args.Model.Languages)
            {
                if (hasAppended)
                {
                    _builder.AppendLine();
                }
                AppendLocalizationCase(language);
                hasAppended = true;
            }
            return _builder.ToString();
        }

        private void AppendLocalizationCase(string language)
        {
            _builder.Append("case \"");
            _builder.Append(language);
            _builder.AppendLine("\":");

            using (_builder.IncrementNestingLevel())
            {
                foreach (var model in _args.Model.LocalizationModels)
                {
                    AppendLocalizationAssignment(model, language);
                }
                _builder.Append("break;");
            }
        }

        private void AppendLocalizationAssignment(
            LocalizationResourceAttributeInstanceModel resource,
            string language)
        {
            if (resource.HasNullNonDefaultResources)
            {
                return;
            }

            var resourceValue = resource.Values
                .FirstOrDefault(s => s.Language == language);
            if (resourceValue.Value is null)
            {
                var defaultLanguage = resource.AttributeModel.DefaultLocalizationParameter?.Language;
                // since we have no default value to resort to, avoid assigning to the resource
                if (defaultLanguage is null)
                {
                    return;
                }

                language = defaultLanguage;
            }

            _builder.Append(resource.ResourceName);
            _builder.Append(".Current = ");
            _builder.Append(resource.ResourceName);
            _builder.Append('.');

            var parameter = resource.AttributeModel.LocalizationParameters
                .First(p => p.Language == language)
                ;
            var propertyName = parameter.AttachedPropertyName;

            _builder.Append(propertyName);

            if (parameter.Parameter.Type.NullableAnnotation is NullableAnnotation.Annotated)
            {
                _builder.Append('!');
            }

            _builder.AppendLine(';');
        }

        public string GenerateNotifyPropertyLines()
        {
            _builder.NestingLevel = 2;

            bool hasAppended = false;
            foreach (var resource in _args.Model.LocalizationModels)
            {
                if (resource.HasNullNonDefaultResources)
                {
                    continue;
                }

                if (hasAppended)
                {
                    _builder.AppendLine();
                }
                AppendNotifyPropertyLine(resource);
                hasAppended = true;
            }
            return _builder.ToString();
        }

        private void AppendNotifyPropertyLine(LocalizationResourceAttributeInstanceModel resource)
        {
            _builder.Append("NotifyPropertyChanged(_");
            _builder.Append(resource.ResourceName.CamelCase());
            _builder.Append("Changed);");
        }
    }
}
