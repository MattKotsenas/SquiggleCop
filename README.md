# SquiggleCop

[![NuGet Version](https://img.shields.io/nuget/v/SquiggleCop.MSBuild?style=flat&logo=nuget&color=blue&label=SquiggleCop.MSBuild)](https://www.nuget.org/packages/SquiggleCop.MSBuild)
[![NuGet Version](https://img.shields.io/nuget/v/SquiggleCop.Tool?style=flat&logo=nuget&color=blu&label=SquiggleCop.Tool)](https://www.nuget.org/packages/SquiggleCop.Tool)
[![Main build](https://github.com/mattkotsenas/squigglecop/actions/workflows/main.yml/badge.svg)](https://github.com/mattkotsenas/squigglecop/actions/workflows/main.yml)

Prevent unintended configuration changes to .NET (Roslyn) analyzers.

SquiggleCop parsers compiler output to create a baseline file of all .NET (Roslyn) analyzer rules and their configured
severity levels. This baseline file should be checked into source control. On subsequent runs the new baseline is
compared to the existing file. If baselines don't match, either an unexpected configuration change was made and should
be fixed, or the baseline should be updated to document the new, expected configuration.

SquiggleCop is available in both MSBuild task and CLI tool form to make it easy to integrate into your existing
development processes.

## Getting Started

### Enabling SARIF logs

SquiggleCop uses [SARIF](https://sarifweb.azurewebsites.net/) v2.1 files to work its magic. If you aren't already
producing SARIF files as part of your build, set the `ErrorLog` property, either in a
[Directory.Build.props](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory?view=vs-2022)
so it automatically applies to all projects:

```xml
<ErrorLog>$(MSBuildProjectFile).diagnostics.sarif,version=2.1</ErrorLog>
```

> NOTE: We recommend you either add `*.sarif` to your `.gitignore` file

or on the command-line for ad-hoc validation:

```powershell
dotnet build -p:ErrorLog=diagnostics.sarif%2cversion=2.1
```

> NOTE: The command and semi-colon characters in the log path must be XML-escaped

### CLI Tool

The CLI tool is designed for ad-hoc validation, or for interacting with baseline files. Install the CLI tool by running
this command:

```powershell
dotnet tool install SquiggleCop.Tool
```

then generate a baseline like this:

```powershell
dotnet squigglecop generate ./path/to/diagnostics.sarif --auto-baseline
```

### MSBuild

The MSBuild package is designed for generating baselines as part of a large build, or to continuously validate baselines
to prevent unintentional changes to build systems.

Add the MSBuild package like this:

```powershell
dotnet add package SquiggleCop.MSBuild
```

If you use Central Package Management (CPM), use a
[`GlobalPackageReference`](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management#global-package-references)
to add SquiggleCop to every project automatically.

```xml
<Project>
  <ItemGroup>
    <GlobalPackageReference Include="SquiggleCop.MSBuild" Version="*" />
  </ItemGroup>
</Project>
```

By default, SquiggleCop expects the baseline file to be named `SquiggleCop.Baseline.yaml` and placed next to the project
file. To specify a custom path to the baseline file, add an item to `AdditionalFiles` that points to the baseline file:

```xml
<Project>
  <ItemGroup>
    <AdditionalFiles Include="/path/to/SquiggleCop.Baseline.yaml" />
  </ItemGroup>
</Project>
```

If a new baseline doesn't match the existing file, SquiggleCop emits MSBuild warning `SQ2000: Baseline mismatch`.
Either use the SquiggleCop CLI to create a new baseline, or enable automatic baselining by setting:

```xml
<Project>
  <PropertyGroup>
    <SquiggleCop_AutoBaseline>true</SquiggleCop_AutoBaseline>
  </PropertyGroup>
</Project>
```

If autobaseline is on, be sure to review any changes to the baseline file before you commit your code.

## Anatomy of a baseline file

A baseline file is a YAML file with a repeating structure:

```yaml
- Id: CA1000
  Title: Do not declare static members on generic types
  Category: Design
  DefaultSeverity: Note
  IsEnabledByDefault: true
  EffectiveSeverities:
  - Note
  - None
  IsEverSuppressed: true
```

### ID

This is the ID of the diagnostic. Nothing special here.

### Title

The title of the diagnostic. Note that some diagnostics have multiple titles for a single ID. For instance, depending
on how it's configured, the title of `IDE0053` can be either "Use block body for lambda expression" or "Use expression
body for lambda expression".

### Category

The category of the diagnostic. See
[Analyzer Configuration](https://github.com/dotnet/roslyn-analyzers/blob/main/docs/Analyzer%20Configuration.md)
for more information.

### DefaultSeverity

The diagnostic's default severity.

### IsEnabledByDefault

If the diagnostic is enabled by default.

### EffectiveSeverities

The severity or severities of a diagnostic once all options have been considered (i.e. rulesets, .editorconfig,
.globalconfig, etc.). One common way to end up with multiple effective severities is to use different .editorconfig files
for different parts of the codebase. Note that inline suppressions will _not_ show up here, instead they show up in
`IsEverSuppressed`.

### IsEverSuppressed

`true` if the diagnostic is ever suppressed at a call site. Common ways to do this are `#pragma` and `[SuppressMessage]`.
