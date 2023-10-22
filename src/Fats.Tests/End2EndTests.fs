module Fats.End2EndTests

open System
open Xunit
open TestHelpers

[<Fact>]
let ``correct exit code is returned for usage`` () =
    let output, errors, exitCode = runFats "--help"
    Assert.True(output.StartsWith("USAGE: ", StringComparison.Ordinal))
    Assert.Equal("", errors)
    Assert.Equal(0, exitCode)

[<Fact>]
let ``test run with multiple files`` () =
    let fileContent1 = "01234"
    let fileContent2 = "567"
    use tmpFile1 = new TmpFile(fileContent1)
    use tmpFile2 = new TmpFile(fileContent2)

    let output, errors, exitCode =
        runFats $"--nomarkup \"{tmpFile1.Path}(1-1)\" \"{tmpFile2.Path}(1-1)\""

    let outputLines = output.Split("\n")
    Assert.Equal(fileContent1, outputLines[0])
    Assert.Equal(fileContent2, outputLines[1])
    Assert.Equal("", errors)
    Assert.Equal(0, exitCode)

[<Fact>]
let ``test run with bad args`` () =
    let output, errors, exitCode = runFats $"--nomarkup \"FooBar\" "
    Assert.Equal("invalid argument: \"FooBar\"", output.ReplaceLineEndings(""))
    Assert.Equal("", errors)
    Assert.Equal(0, exitCode)
