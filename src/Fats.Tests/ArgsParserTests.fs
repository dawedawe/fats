module ArgsParserTests

open Xunit
open Fats.ArgsParser
open Fats.Model

[<Fact>]
let ``Single windows-style arg with double minus in RangeOfPositions is parsed correctly`` () =
    let path = @".\src\Fats\Program.fs"
    let args = [| $"{path}:(1,0--2,0)" |]
    let ranges = parse args
    Assert.Single(ranges) |> ignore
    Assert.Equal(path, ranges.[0].File.Value)

    match ranges[0] with
    | OfPositions r ->
        Assert.Equal((Pos.Create (Line 1) (Column 0)), r.Start)
        Assert.Equal((Pos.Create (Line 2) (Column 0)), r.End)
    | _ -> Assert.True(false)

[<Fact>]
let ``Single unix-style arg with single minus in RangeOfPositions is parsed correctly`` () =
    let path = @"./src/Fats/Program.fs"
    let args = [| $"{path}:(1,0-2,0)" |]
    let ranges = parse args
    Assert.Single(ranges) |> ignore
    Assert.Equal(path, ranges.[0].File.Value)

    match ranges[0] with
    | OfPositions r ->
        Assert.Equal((Pos.Create (Line 1) (Column 0)), r.Start)
        Assert.Equal((Pos.Create (Line 2) (Column 0)), r.End)
    | _ -> Assert.True(false)


[<Fact>]
let ``Single windows-style arg with double minus in RangeOfLines is parsed correctly`` () =
    let path = @".\src\Fats\Program.fs"
    let args = [| $"{path}:(1--2)" |]
    let ranges = parse args
    Assert.Single(ranges) |> ignore
    Assert.Equal(path, ranges.[0].File.Value)

    match ranges[0] with
    | OfLines r ->
        Assert.Equal(Line 1, r.Start)
        Assert.Equal(Line 2, r.End)
    | _ -> Assert.True(false)

[<Fact>]
let ``Single unix-style arg with single minus in RangeOfLines is parsed correctly`` () =
    let path = @"./src/Fats/Program.fs"
    let args = [| $"{path}:(1-2)" |]
    let ranges = parse args
    Assert.Single(ranges) |> ignore
    Assert.Equal(path, ranges.[0].File.Value)

    match ranges[0] with
    | OfLines r ->
        Assert.Equal(Line 1, r.Start)
        Assert.Equal(Line 2, r.End)
    | _ -> Assert.True(false)


[<Fact>]
let ``No range for arg without range`` () =
    let args = [| "foo" |]
    let ranges = parse args
    Assert.Empty(ranges)

[<Fact>]
let ``No range for arg without file`` () =
    let args = [| ":(1,0--2,0)" |]
    let ranges = parse args
    Assert.Empty(ranges)
