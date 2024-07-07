# Usage Documentation

## Disclaimer

This generator package is only targeted for C# projects. VB is not supported.

If you are using an IDE, Visual Studio 2022 or JetBrains Rider 2023.1 and above are recommended. Other versions may not behave correctly.

## Motivation

Localization on apps is often misimplemented, requiring complex structures and performance being sometimes questionable. Frameworks like MAUI, Avalonia, etc. often interact with the current locale via CurrentUICulture, which is an abstraction with additional complexity glued to it than necessary, when localization could be more easily variable and specific to individual parts of the app.

## Package Installation

To install the generator in your project (not the entire solution), you must install the following NuGet packages:

- [SourceLocalizationGenerator](https://www.nuget.org/packages/SourceLocalizationGenerator) - includes the generator WITHOUT the core components.
- [SourceLocalizationGenerator.Core](https://www.nuget.org/packages/SourceLocalizationGenerator.Core) - ONLY includes the core components. You must also manually install this package.

Ensure that `SourceLocalizationGenerator.Core` has no `PrivateAssets="true"` in the corresponding `PackageDependency` tag in your project files. This enables the consumption of the attributes that are provided from the package, upon which the generators are based.

## Disclaimer

Before going into the guide for this package, it's worth noting that this is a solution for a specific problem where localization of a limited and fixed subset of languages is required. Using this generator is not recommended for more than 4 languages, which could quickly turn into a giant mess of parameters.

With the above guideline in mind, it is possible to offer an alternative API for interacting with this generator where each language is specifically described via an attribute by binding to the same resource key, allowing breaking the localization values into multiple files, one per language for multiple languages. This future expansion will be noted in the issues.

## Guide

This guide assumes that the package has been correctly included in your project, following the steps shown above.

### Localization template

To use localization we must provide a template for defining our localization resources, which carries semantics about the activated language. This can be done by defining an attribute like the one below:

```csharp
using SourceLocalizationGenerator.Core;
using System;
using System.Collections.Immutable;

namespace X.Y.Z;

#nullable enable

[GeneratedLocalizationStringAttribute]
public partial class TestLocalizationAttribute(
    [LocalizationStringResourceName]
    string resourceName,
    [LocalizationStringResourceValue("ell", Default = true)]
    string greek,
    [LocalizationStringResourceValue("eng")]
    string? english)
    : Attribute, ILocalizationStringAttribute
    ;
```

> <div style="text-align: center; padding: 5px"><b>WARNING</b></div>
> <p>This partial declaration must <b>always</b> implement exactly <code>Attribute, ILocalizationStringAttribute</code>, so that the generator is aware of the attribute type being an attribute. While the one generator generates a partial declaration of this attribute type that also includes these base types, the compiler is not aware of the actual attribute type that is used in the declarations of localization resources that we will see below.</p>

`GeneratedLocalizationStringAttribute` is required to inform the generator about the annotated attribute being an attribute defining a template for localization strings.

`LocalizationStringResourceName` denotes the name of the resource that is being assigned in this attribute. It is required to give a name to the exposed property that holds the current localization value in the localization string container type (described below).

`LocalizationStringResourceValue` denotes that the parameter is bound to a resource value for the given language described by the code (`ell` or `eng` in this example). `Default = true` denotes that the language is the default language that is being used for this localization resource, therefore allowing assigning `null` values to other languages. This can be helpful to avoid unnecessary assignments and event invocations notifying a no-op change.

The generator will automatically generate two files for this attribute, one being another partial declaration of the attribute adding properties and methods about the attribute, and the other being a resource record type which holds the current localization value and its properties, shown below:

```csharp
using SourceLocalizationGenerator.Core;
using System;
using System.Collections.Immutable;

namespace X.Y.Z;

#nullable enable

public sealed record TestLocalizationResource(
    string Greek,
    string? English)
{
    // Directly settable from outside the class for improved performance
    public string Current { get; internal set; } = Greek;

    public string? GetResource(string language)
    {
        return language switch
        {
            "ell" => Greek,
            "eng" => English,
            _ => null,
        };
    }
}
```

This resource record is not intended to be manually created by the user, and is automatically handled by the generator package, described in the following sections.

### Localization containers

Localization containers are classes that contain localizable strings, which are automatically generated based on the applied attributes.

Given the above example localization attribute, a localization container class can be very simply declared like so:

```csharp
using SourceLocalizationGenerator.Core;
using System;
using System.Collections.Immutable;

namespace X.Y.Z;

#nullable enable

[LocalizationStringResourceContainer]
[TestLocalization(nameof(Current), "Ρεύμα", "Current")]
[TestLocalization(nameof(Amperes), "Amperes", null)]
public sealed partial class AmpereLocalizationResourceContainer;
```

The `LocalizationStringResourceContainer` attribute helps trigger the generator for this specific container class.

`TestLocalization` refers to the attribute we created earlier which declares the API we will use to denote a localization resource property in the container. Each such attribute creates one localization resource property.

For the above localization resource container class, the generator will generate this partial declaration:

```csharp
using SourceLocalizationGenerator.Core;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using X.Y.Z;

namespace X.Y.Z;

#nullable enable

sealed partial class AmpereLocalizationResourceContainer
    : INotifyPropertyChanged
{
    private static readonly PropertyChangedEventArgs _currentChanged = new(nameof(Current));

    private string _currentLanguage = "ell";

    public TestLocalizationResource Current { get; } = new("Ρεύμα", "Current");
    public TestLocalizationResource Amperes { get; } = new("Amperes", null);

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetLanguage(string language)
    {
        if (_currentLanguage == language)
            return;

        _currentLanguage = language;

        switch (language)
        {
            case "ell":
                Current.Current = Current.Greek;
                break;
            case "eng":
                Current.Current = Current.English!;
                break;
        }

        NotifyPropertyChanged(_currentChanged);
    }

    private void NotifyPropertyChanged(PropertyChangedEventArgs eventArgs)
    {
        PropertyChanged?.Invoke(this, eventArgs);
    }
}
```

Notice that since we declared `Amperes` as `null` in English, there is no code for changing the `Current` value of the `Amperes` resource.

Alternatively, assume that we also introduced the French language in our localization resource template. With the following container declaration:

```csharp
using SourceLocalizationGenerator.Core;
using System;
using System.Collections.Immutable;

namespace X.Y.Z;

#nullable enable

[LocalizationStringResourceContainer]
[TestLocalization(nameof(Current), "Ρεύμα", "Current", "Courant")]
[TestLocalization(nameof(Amperes), "Amperes", null, "Ampères")]
public sealed partial class AmpereLocalizationResourceContainer;
```

We get the following generated code:

```csharp
using SourceLocalizationGenerator.Core;
using System;
using System.Collections.Immutable;
using System.ComponentModel;
using X.Y.Z;

namespace X.Y.Z;

#nullable enable

sealed partial class AmpereLocalizationResourceContainer
    : INotifyPropertyChanged
{
    private static readonly PropertyChangedEventArgs _currentChanged = new(nameof(Current));
    private static readonly PropertyChangedEventArgs _amperesChanged = new(nameof(Amperes));

    private string _currentLanguage = "ell";

    public TestLocalizationResource Current { get; } = new("Ρεύμα", "Current", "Courant");
    public TestLocalizationResource Amperes { get; } = new("Amperes", null, "Ampères");

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetLanguage(string language)
    {
        if (_currentLanguage == language)
            return;

        _currentLanguage = language;

        switch (language)
        {
            case "ell":
                Current.Current = Current.Greek;
                Amperes.Current = Amperes.Greek;
                break;
            case "eng":
                Current.Current = Current.English!;
                Amperes.Current = Amperes.Greek;
                break;
            case "fra":
                Current.Current = Current.French!;
                Amperes.Current = Amperes.French!;
                break;
        }

        NotifyPropertyChanged(_currentChanged);
        NotifyPropertyChanged(_amperesChanged);
    }

    private void NotifyPropertyChanged(PropertyChangedEventArgs eventArgs)
    {
        PropertyChanged?.Invoke(this, eventArgs);
    }
}
```

In this case, since English has no localization for `Amperes`, but French does have one, changing its property value is not omitted, and we thus generate code for both changing its value and for its property change notifiers.

### Consumption

To make use of the generated localization resources, we need to store instances of the containers, and operate language changes on them. Each container is responsible for its current localization values.

As common practice, assume that we store the current localization values in a static class like this:

```csharp
namespace X.Y.Z;

public static class LocalizationContainerStorage
{
    private static AmpereLocalizationResourceContainer Amperes { get; } = new();

    public static void SetLanguage(string language)
    {
        Amperes.SetLanguage(language);
    }
}
```

Via the `SetLanguage` method on `LocalizationContainerStorage` we can centralize the behavior of setting the current language across all the localization containers that the storage may contain.

The localization values within the container can be retrieved by binding to the `Current` property of each of the localization resource in the container. For example, in MAUI this would be equal to the following (XAML):

```xml
<Label
  Text="{x:Static local:LocalizationContainerStorage.Amperes.Amperes.Current}"
  />
```

Because we implement `INotifyPropertyChanged`, the binding is valid and will automatically reflect the changes upon adjusting the language, as implemented in the generated boilerplate code with zero dictionary lookup overhead, as opposed to traditional app resource containers.
