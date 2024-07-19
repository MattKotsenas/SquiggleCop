using Microsoft.Build.Utilities.ProjectCreation;

namespace SquiggleCop.Tasks.Tests;

public class BaselineFileTests : TestBase
{
    private const string SampleSarif1 =
        """
        {
          "$schema": "http://json.schemastore.org/sarif-2.1.0",
          "version": "2.1.0",
          "runs": [
            {
              "properties": {
                "analyzerExecutionTime": "15.126"
              },
              "tool": {
                "driver": {
                  "name": "Microsoft (R) Visual C# Compiler",
                  "semanticVersion": "4.10.0",
                  "rules": [
                    {
                      "id": "CA1000",
                      "shortDescription": {
                        "text": "Do not declare static members on generic types"
                      },
                      "fullDescription": {
                        "text": "When a static member of a generic type is called, the type argument must be specified for the type. When a generic instance member that does not support inference is called, the type argument must be specified for the member. In these two cases, the syntax for specifying the type argument is different and easily confused."
                      },
                      "defaultConfiguration": {
                        "level": "note"
                      },
                      "helpUri": "https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1000",
                      "properties": {
                        "category": "Design",
                        "isEverSuppressed": "true",
                        "suppressionKinds": [
                          "external"
                        ],
                        "executionTimeInSeconds": "<0.001",
                        "executionTimeInPercentage": "<1",
                        "tags": [
                          "PortedFromFxCop",
                          "Telemetry",
                          "EnabledRuleInAggressiveMode"
                        ]
                      }
                    }
                  ]
                }
              },
              "invocations": [
                {
                  "executionSuccessful": true,
                  "ruleConfigurationOverrides": [
                    {
                      "descriptor": {
                        "id": "MA0026",
                        "index": 432
                      },
                      "configuration": {
                        "level": "warning"
                      }
                    },
                    {
                      "descriptor": {
                        "id": "MA0026",
                        "index": 432
                      },
                      "configuration": {
                        "level": "note"
                      }
                    },
                    {
                      "descriptor": {
                        "id": "S1135",
                        "index": 886
                      },
                      "configuration": {
                        "level": "warning"
                      }
                    },
                    {
                      "descriptor": {
                        "id": "S1135",
                        "index": 886
                      },
                      "configuration": {
                        "level": "note"
                      }
                    }
                  ]
                }
              ],
              "columnKind": "utf16CodeUnits"
            }
          ]
        }
        """;

    private const string SampleSarif2 =
    """
        {
          "$schema": "http://json.schemastore.org/sarif-2.1.0",
          "version": "2.1.0",
          "runs": [
            {
              "properties": {
                "analyzerExecutionTime": "15.126"
              },
              "tool": {
                "driver": {
                  "name": "Microsoft (R) Visual C# Compiler",
                  "semanticVersion": "4.10.0",
                  "rules": [
                    {
                      "id": "CA1000",
                      "shortDescription": {
                        "text": "Do not declare static members on generic types"
                      },
                      "fullDescription": {
                        "text": "When a static member of a generic type is called, the type argument must be specified for the type. When a generic instance member that does not support inference is called, the type argument must be specified for the member. In these two cases, the syntax for specifying the type argument is different and easily confused."
                      },
                      "defaultConfiguration": {
                        "level": "note"
                      },
                      "helpUri": "https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1000",
                      "properties": {
                        "category": "Design",
                        "isEverSuppressed": "true",
                        "suppressionKinds": [
                          "external"
                        ],
                        "executionTimeInSeconds": "<0.001",
                        "executionTimeInPercentage": "<1",
                        "tags": [
                          "PortedFromFxCop",
                          "Telemetry",
                          "EnabledRuleInAggressiveMode"
                        ]
                      }
                    },
                    {
                      "id": "CA1001",
                      "shortDescription": {
                        "text": "Types that own disposable fields should be disposable"
                      },
                      "fullDescription": {
                        "text": "A class declares and implements an instance field that is a System.IDisposable type, and the class does not implement IDisposable. A class that declares an IDisposable field indirectly owns an unmanaged resource and should implement the IDisposable interface."
                      },
                      "defaultConfiguration": {
                        "level": "note"
                      },
                      "helpUri": "https://learn.microsoft.com/dotnet/fundamentals/code-analysis/quality-rules/ca1001",
                      "properties": {
                        "category": "Design",
                        "isEverSuppressed": "true",
                        "suppressionKinds": [
                          "external"
                        ],
                        "executionTimeInSeconds": "<0.001",
                        "executionTimeInPercentage": "<1",
                        "tags": [
                          "PortedFromFxCop",
                          "Telemetry",
                          "EnabledRuleInAggressiveMode"
                        ]
                      }
                    }
                  ]
                }
              },
              "invocations": [
                {
                  "executionSuccessful": true,
                  "ruleConfigurationOverrides": [
                    {
                      "descriptor": {
                        "id": "MA0026",
                        "index": 432
                      },
                      "configuration": {
                        "level": "warning"
                      }
                    },
                    {
                      "descriptor": {
                        "id": "MA0026",
                        "index": 432
                      },
                      "configuration": {
                        "level": "note"
                      }
                    },
                    {
                      "descriptor": {
                        "id": "S1135",
                        "index": 886
                      },
                      "configuration": {
                        "level": "warning"
                      }
                    },
                    {
                      "descriptor": {
                        "id": "S1135",
                        "index": 886
                      },
                      "configuration": {
                        "level": "note"
                      }
                    }
                  ]
                }
              ],
              "columnKind": "utf16CodeUnits"
            }
          ]
        }
        """;

    public BaselineFileTests()
    {
        File.WriteAllText(Path.Combine(TestRootPath, "sample1.log"), SampleSarif1);
        File.WriteAllText(Path.Combine(TestRootPath, "sample2.log"), SampleSarif2);
    }

    // TODO: Implement explicit file support

    [Theory]
    [CombinatorialData]
    public async Task NoBaselineFile(bool? autoBaseline, bool explicitFile)
    {
        DateTime now = DateTime.UtcNow;
        FileInfo baselineFile = new(Path.Combine(TestRootPath, SquiggleCop.BaselineFile));

        if (autoBaseline.HasValue && !autoBaseline.Value)
        {
            // TODO: Implement; when auto is off, do what? Give message to run tool?
            return;
        }

        const string errorLog = "sarif.log";

        ProjectCreator.Templates.SimpleBuild()
            .PropertyGroup()
                .Property("ErrorLog", $"{errorLog},version=2.1")
                .Property("SquiggleCop_AutoBaseline", autoBaseline?.ToString().ToLowerInvariant())
            .Target(name: "_SetSarifLog", beforeTargets: "AfterCompile")
                .TaskMessage("Overwriting ErrorLog with sample to simulate compile...")
                .Task(name: "Copy", parameters: new Dictionary<string, string?>(StringComparer.Ordinal) { { "SourceFiles", Path.Combine(TestRootPath, "sample1.log") }, { "DestinationFiles", errorLog } })
            .Save(Path.Combine(TestRootPath, "project.csproj"))
            .TryBuild(restore: true, out bool result, out BuildOutput output);

        result.Should().BeTrue();
        baselineFile.LastWriteTimeUtc.Should().BeOnOrAfter(now);
        await Verify(
            new BaselineFileResults(
                output.ToBuildLogMessages(),
                await File.ReadAllTextAsync(baselineFile.FullName)
        )).UseParameters(autoBaseline, explicitFile);
    }

    [Theory]
    [CombinatorialData]
    public void BaselineUpToDate(bool? autoBaseline, bool explicitFile)
    {
        // TODO: Implement; in both cases, ensure to not set the modified date
    }

    [Theory]
    [CombinatorialData]
    public void BaselineOutOfDate(bool? autoBaseline, bool explicitFile)
    {
        // TODO: Implement; when auto is on, update the file
        // TODO: Implement; when auto is off, pop Verify? Give message to run tool?
    }
}
