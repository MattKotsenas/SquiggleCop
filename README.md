![Icon](https://raw.githubusercontent.com/MattKotsenas/SquiggleCop/main/icon.png)

# SquiggleCop

Prevent unintended configuration changes to .NET (Roslyn) analyzers.

[![NuGet Version](https://img.shields.io/nuget/v/SquiggleCop.Tasks?style=flat&logo=nuget&color=blue&label=SquiggleCop.Tasks)](https://www.nuget.org/packages/SquiggleCop.Tasks)
[![NuGet Version](https://img.shields.io/nuget/v/SquiggleCop.Tool?style=flat&logo=nuget&color=blue&label=SquiggleCop.Tool)](https://www.nuget.org/packages/SquiggleCop.Tool)
[![Main build](https://github.com/mattkotsenas/squigglecop/actions/workflows/main.yml/badge.svg)](https://github.com/mattkotsenas/squigglecop/actions/workflows/main.yml)

There are many ways to configure diagnostic warning and error levels in a .NET build, and understanding how they all
interact can be tricky to get correct. .NET / MSBuild support all these mechanisms (and probably more!) to configure
what analyzers are enabled and at what severity level:

- Analyzers provided in the SDK ([docs](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/overview?tabs=net-8))
- Analyzer NuGet packages ([example](https://www.nuget.org/packages/roslynator.analyzers#readme-body-tab))
- `.editorconfig` ([docs](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files#editorconfig))
- `.globalconfig` ([docs](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/configuration-files#global-analyzerconfig))
- `.ruleset` ([docs](https://learn.microsoft.com/en-us/previous-versions/visualstudio/visual-studio-2019/code-quality/using-rule-sets-to-group-code-analysis-rules?view=vs-2019-archive))
- `WarningLevel` ([docs](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/errors-warnings#warninglevel))
- `AnalysisLevel` ([docs](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/errors-warnings#analysis-level))
- `TreatWarningsAsErrors` ([docs](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/errors-warnings#treatwarningsaserrors))
- `WarningsAsErrors` ([docs](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/errors-warnings#warningsaserrors-and-warningsnotaserrors))
- `WarningsNotAsErrors` ([docs](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/errors-warnings#warningsaserrors-and-warningsnotaserrors))
- `NoWarn` ([docs](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/errors-warnings#nowarn))

That's a lot to keep track of!

With SquiggleCop, any change to project files or build scripts produces an easy to understand (and diff!) baseline file
that shows the consequences of the change:

<table>
<tr>
<th>Code Change</th>
</tr>
<tr>
<td>

```diff
--- a/sample.csproj
+++ b/sample.csproj
@@ -3,7 +3,7 @@
   <PropertyGroup>
     <OutputType>Exe</OutputType>
     <TargetFramework>net8.0</TargetFramework>
-    <AnalysisLevel>5</AnalysisLevel>
+    <AnalysisLevel>6</AnalysisLevel>
     <ImplicitUsings>enable</ImplicitUsings>
     <Nullable>enable</Nullable>
```

</td>
<tr>
<th>Baseline File Diff</th>
</tr>
<tr>
<td>

```diff
--- a/SquiggleCop.Baseline.yaml
+++ b/SquiggleCop.Baseline.yaml
@@ -57,8 +57,8 @@
 - {Id: CA1401, Title: P/Invokes should not be visible, Category: Interoperability, DefaultSeverity: Note, IsEnabledByDefault: true, EffectiveSeverities: [Note], IsEverSuppressed: false}
 - {Id: CA1416, Title: Validate platform compatibility, Category: Interoperability, DefaultSeverity: Warning, IsEnabledByDefault: true, EffectiveSeverities: [Warning], IsEverSuppressed: false}
 - {Id: CA1417, Title: Do not use 'OutAttribute' on string parameters for P/Invokes, Category: Interoperability, DefaultSeverity: Warning, IsEnabledByDefault: true, EffectiveSeverities: [Warning], IsEverSuppressed: false}
-- {Id: CA1418, Title: Use valid platform string, Category: Interoperability, DefaultSeverity: Warning, IsEnabledByDefault: true, EffectiveSeverities: [None], IsEverSuppressed: true}
-- {Id: CA1419, Title: Provide a parameterless constructor that is as visible as the containing type for concrete types derived from 'System.Runtime.InteropServices.SafeHandle', Category: Interoperability, DefaultSeverity: Note, IsEnabledByDefault: true, EffectiveSeverities: [None], IsEverSuppressed: true}
+- {Id: CA1418, Title: Use valid platform string, Category: Interoperability, DefaultSeverity: Warning, IsEnabledByDefault: true, EffectiveSeverities: [Warning], IsEverSuppressed: false}
+- {Id: CA1419, Title: Provide a parameterless constructor that is as visible as the containing type for concrete types derived from 'System.Runtime.InteropServices.SafeHandle', Category: Interoperability, DefaultSeverity: Note, IsEnabledByDefault: true, EffectiveSeverities: [Note], IsEverSuppressed: false}
 - {Id: CA1420, Title: 'Property, type, or attribute requires runtime marshalling', Category: Interoperability, DefaultSeverity: Warning, IsEnabledByDefault: true, EffectiveSeverities: [None], IsEverSuppressed: true}
 - {Id: CA1421, Title: This method uses runtime marshalling even when the 'DisableRuntimeMarshallingAttribute' is applied, Category: Interoperability, DefaultSeverity: Note, IsEnabledByDefault: true, EffectiveSeverities: [None], IsEverSuppressed: true}
 - {Id: CA1422, Title: Validate platform compatibility, Category: Interoperability, DefaultSeverity: Warning, IsEnabledByDefault: true, EffectiveSeverities: [None], IsEverSuppressed: false}
```

</td>
</tr>
</table>

SquiggleCop parses compiler output to create a baseline file of all .NET (Roslyn) analyzer rules and their configured
severity levels. This baseline file should be checked into source control. On subsequent runs the new baseline is
compared to the existing file. If baselines don't match, either an unexpected configuration change was made and should
be fixed, or the baseline should be updated to document the new, expected configuration.

SquiggleCop is available in both MSBuild task and CLI tool form to make it easy to integrate into your existing
development processes.

## Getting Started

### Enabling SARIF logs

SquiggleCop uses [SARIF](https://sarifweb.azurewebsites.net/) v2.1 files to work its magic. If you aren't already
producing SARIF files as part of your build, set the
[`ErrorLog`](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/errors-warnings#errorlog)
property, either in a
[Directory.Build.props](https://learn.microsoft.com/en-us/visualstudio/msbuild/customize-by-directory?view=vs-2022)
so it automatically applies to all projects:

```xml
<ErrorLog>$(MSBuildProjectFile).diagnostics.sarif,version=2.1</ErrorLog>
```

> [!TIP]
> We recommend you add `*.sarif` to your `.gitignore` file

or on the command-line for ad-hoc validation:

```powershell
dotnet build -p:ErrorLog=diagnostics.sarif%2cversion=2.1
```

> [!IMPORTANT]
> The comma or semi-colon character in the log path must be XML-escaped

### CLI Tool

The CLI tool is designed for ad-hoc validation and interacting with baseline files. Install the CLI tool by running
this command:

```powershell
dotnet tool install SquiggleCop.Tool
```

then generate a baseline like this:

```powershell
dotnet squigglecop generate ./path/to/diagnostics.sarif --auto-baseline
```

### MSBuild Tasks

The Tasks package automatically integrates SquiggleCop into the build using MSBuild. This package is designed for
generating baselines as part of a large build and to continuously validate baselines and prevent unintentional build changes.

Add the Tasks package like this:

```powershell
dotnet add package SquiggleCop.Tasks
```

If you use Central Package Management (CPM), you can use a
[`GlobalPackageReference`](https://learn.microsoft.com/en-us/nuget/consume-packages/central-package-management#global-package-references)
to add SquiggleCop to every project automatically.

```xml
<Project>
  <ItemGroup>
    <GlobalPackageReference Include="SquiggleCop.Tasks" Version="{{version}}" />
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

If autobaseline is on, be sure to review any changes to the baseline file before committing your code.

> [!CAUTION]
> If you turn auto-baseline on, be sure to turn it off in CI. Otherwise SquiggleCop may not be able to warn about
> potential issues!

## Anatomy of a baseline file

A baseline file is a YAML file with a repeating structure. Here's a single rule entry:

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

This is the ID of the diagnostic.

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

`true` if the diagnostic is ever suppressed at a call site. Common ways to do this are:

- `#pragma`
- `[SuppressMessage]`
- `<AnalysisLevel>`

## Diagnosing baseline mismatches

The easiest way to debug a baseline mismatch that occurs in CI but doesn't occur locally is to:

1. Upload the SARIF files from the build
2. Use the SquiggleCop CLI to generate a new baseline from the CI SARIF file and compare it to the checked in baseline

### Uploading SARIF reports

Upload your SARIF reports as pipeline artifacts to help narrow down issues.

#### GitHub Actions

```yaml
- name: Upload SARIF logs
  uses: actions/upload-artifact@v4
  if: success() || failure() # Upload logs even if the build failed
  with:
    name: SARIF logs
    path: ./artifacts/**/*.sarif # Modify as necessary to point to your sarif files
```

#### Azure DevOps

```yaml
- task: CopyFiles@2
  displayName: 'Copy SARIF files to Artifact Staging'
  condition: succeededOrFailed() # Upload logs even if the build failed
  inputs:
    contents: 'artifacts\**\*.sarif' # Modify as necessary to point to your sarif files
    targetFolder: '$(Build.ArtifactStagingDirectory)\sarif'
    cleanTargetFolder: true
    overWrite: true

- task: PublishPipelineArtifact@1
  displayName: 'Publish SARIF files as Artifacts'
  condition: succeededOrFailed() # Upload logs even if the build failed
  inputs:
    targetPath: '$(Build.ArtifactStagingDirectory)\sarif'
    publishLocation: 'pipeline'
    artifact: 'sarif'
```

### Common sources of baseline mismatches

- Different MSBuild parameters locally vs CI
  - Also check if settings are based off the `$(ContinuousIntegrationBuild)` property, which some CI providers set
- Different SDK versions
  - Use a [global.json](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json) to set the same SDK version
  locally and in CI
  - New SDK feature versions can introduce new analyzers so we suggest limiting `rollForward` to patch updates, or disable entirely

## Advanced configuration

### Alternate baseline paths

By default, SquiggleCop expects the baseline file to be named `SquiggleCop.Baseline.yaml` and placed next to the project
file. To specify a custom path to the baseline file, add an item to `AdditionalFiles` that points to the baseline file:

```xml
<Project>
  <ItemGroup>
    <AdditionalFiles Include="/path/to/SquiggleCop.Baseline.yaml" />
  </ItemGroup>
</Project>
```

### TreatWarningsAsErrors

Often, projects use `TreatWarningsAsErrors` in CI builds to prevent warnings from entering the main branch.

However, toggling TreatWarningsAsErrors _also_ changes the effective severity of analyzer diagnostics, which can lead to
unnecessary churn in baseline files. If your project or development workflow toggles TreatWarningsAsErrors between CI
and local development, also toggle the `SquiggleCop_Enabled` property based on the same logic.

Here's a sample that toggles TreatWarningsAsErrors based on the
[`ContinuousIntegrationBuild`](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/msbuild-props#continuousintegrationbuild)
property:

```xml
<Project>
  <PropertyGroup>
    <PedanticMode Condition=" '$(PedanticMode)' == '' ">$([MSBuild]::ValueOrDefault('$(ContinuousIntegrationBuild)', 'false'))</PedanticMode>
    <TreatWarningsAsErrors>$(PedanticMode)</TreatWarningsAsErrors>
    <SquiggleCop_Enabled>$(PedanticMode)</SquiggleCop_Enabled>
  </PropertyGroup>
</Project>
```

### Encodings & line endings

Baseline files are written in UTF-8 encoding without a BOM. Baseline files use the `\n` line ending on all
platforms. SquiggleCop's own diffing algorithm ignores end of line differences to avoid unnecessary issues, however
depending on your `.gitattributes` settings line endings may be normalized to other values. If Git's line ending
normalization is causing issues, consider setting the following in your `.gitattributes` file:

```
# Store SquiggleCop baselines as lf regardless of platform
SquiggleCop.Baseline.yaml text eol=lf
```

And then run `git add --renormalize .` to update Git with the re-normalized files.

---

_Icon 'fractal' by Bohdan Burmich from [Noun Project](https://thenounproject.com/browse/icons/term/fractal/)
(CC BY 3.0)_
