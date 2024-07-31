namespace SquiggleCop.Tasks.Tests;

internal sealed class TestData
{
    public string Sarif { get; init; }
    public string Baseline { get; init; }

    private TestData(string sarif, string baseline)
    {
        Sarif = sarif;
        Baseline = baseline;
    }

    public static TestData Sample1 { get; } = new(
        sarif:
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
        """,
        baseline:
        """
        - {Id: CA1000, Title: Do not declare static members on generic types, Category: Design, DefaultSeverity: Note, IsEnabledByDefault: true, EffectiveSeverities: [Note], IsEverSuppressed: true}
        """);
}
