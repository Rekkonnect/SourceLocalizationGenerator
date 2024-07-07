# SourceLocalizationGenerator

A Roslyn source generator for embedding localization strings and efficiently swapping dynamically.

## References

- [Usage](docs/usage/usage.md)
- [Diagnostics](docs/rules/index.md)

## Downloads

### NuGet Packages

- [SourceLocalizationGenerator](https://www.nuget.org/packages/SourceLocalizationGenerator) - includes the generator WITHOUT the core components.
- [SourceLocalizationGenerator.Core](https://www.nuget.org/packages/SourceLocalizationGenerator.Core) - ONLY includes the core components. You must also manually install this package.

### Consumption

Include a reference to the `SourceLocalizationGenerator` and `SourceLocalizationGenerator.Core` NuGet packages in the desired projects.

It is likely that the installation of the analyzers and the source generator in a project in Visual Studio may cause its behavior to be fuzzy, so it is recommended to restart Visual Studio.

Support for JetBrains Rider is not tested. Please consider opening compatibility issues [here](https://github.com/Rekkonnect/SourceLocalizationGenerator/issues).

## Notes

For Visual Studio users, this package is only intended to be used in Visual Studio 2022 onwards. The package makes use of incremental generators, which are only supported in Visual Studio 2022.

For JetBrains Rider users, the latest versions should theoretically have no issues running the generators and the analyzers included in the package.
