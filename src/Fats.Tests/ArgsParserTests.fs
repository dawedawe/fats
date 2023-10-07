module ArgsParserTests

open Xunit
open Fats.ArgsParser
open Fats.Model

[<Fact>]
let ``Single windows-style arg with double minus in range is parsed correctly`` () =
    let path = @".\src\Fats\Program.fs"
    let args = [| $"{path}:(1,0--2,0)" |]
    let ranges = parse args
    Assert.Single(ranges) |> ignore
    Assert.Equal(path, ranges.[0].File.Value)
    Assert.Equal((Pos.Create (Line 1) (Column 0)), ranges.[0].Start)
    Assert.Equal((Pos.Create (Line 2) (Column 0)), ranges.[0].End)

[<Fact>]
let ``Single unix-style arg with single minus in range is parsed correctly`` () =
    let path = @"./src/Fats/Program.fs"
    let args = [| $"{path}:(1,0-2,0)" |]
    let ranges = parse args
    Assert.Single(ranges) |> ignore
    Assert.Equal(path, ranges.[0].File.Value)
    Assert.Equal((Pos.Create (Line 1) (Column 0)), ranges.[0].Start)
    Assert.Equal((Pos.Create (Line 2) (Column 0)), ranges.[0].End)

[<Fact>]
let ``No range for arg without range`` () =
    let args = [| "foo" |]
    let ranges = Fats.ArgsParser.parse args
    Assert.Empty(ranges)

[<Fact>]
let ``No range for arg without file`` () =
    let args = [| ":(1,0--2,0)" |]
    let ranges = Fats.ArgsParser.parse args
    Assert.Empty(ranges)
