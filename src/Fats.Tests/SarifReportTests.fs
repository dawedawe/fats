module Fats.SarifReportTests

open Xunit
open Fats.Model
open Fats.SarifReport

let rawText () =
    """
{
  "$schema": "https://schemastore.azurewebsites.net/schemas/json/sarif-2.1.0-rtm.6.json",
  "version": "2.1.0",
  "runs": [
    {
      "results": [
        {
          "ruleId": "GRA-STRING-002",
          "ruleIndex": 0,
          "message": {
            "text": "The usage of String.StartsWith with a single string argument is discouraged. Signal your intention explicitly by calling an overload."
          },
          "locations": [
            {
              "physicalLocation": {
                "artifactLocation": {
                  "uri": "src/testbed/Library.fs"
                },
                "region": {
                  "startLine": 6,
                  "startColumn": 11,
                  "endLine": 6,
                  "endColumn": 35
                }
              }
            }
          ]
        }
      ],
      "tool": {
        "driver": {
          "name": "Ionide.Analyzers.Cli",
          "version": "0.16.0.0",
          "informationUri": "https://ionide.io/FSharp.Analyzers.SDK/",
          "rules": [
            {
              "id": "GRA-STRING-002",
              "name": "The usage of String.StartsWith with a single string argument is discouraged. Signal your intention explicitly by calling an overload.",
              "shortDescription": {
                "text": "Verifies the correct usage of System.String.StartsWith",
                "markdown": "Verifies the correct usage of System.String.StartsWith"
              },
              "helpUri": "https://learn.microsoft.com/en-us/dotnet/standard/base-types/best-practices-strings"
            }
          ]
        }
      },
      "invocations": [
        {
          "startTimeUtc": "2023-10-17T21:45:47.865Z",
          "endTimeUtc": "2023-10-17T21:45:47.951Z",
          "executionSuccessful": true
        }
      ],
      "columnKind": "utf16CodeUnits"
    }
  ]
}
"""

[<Fact>]
let ``parsing sarif returns correct ranges`` () =
    let expected0 =
        OfPosition(RangeOfPosition.Create (Path @"src/testbed/Library.fs") (Pos.Create (Line 6) (Column 11)))

    let ranges = readLog rawText |> itemsFromLog

    Assert.Equal(1, ranges.Length)
    Assert.Equal(expected0, ranges[0].Range)
