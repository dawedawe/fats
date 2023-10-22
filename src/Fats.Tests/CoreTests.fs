module Fats.CoreTests

open System
open Xunit
open Fats.Core
open Fats.Model

[<Fact>]
let ``RangeOfPositions with length zero returns empty string from non-empty line`` () =
    let lines = [| "0123" |]

    let range =
        OfPositions(
            RangeOfPositions.Create (Path "test.txt") (Pos.Create (Line 1) (Column 0)) (Pos.Create (Line 1) (Column 0))
        )

    match rangeContent lines range with
    | Ok content ->
        Assert.Equal("", content.Pre)
        Assert.Equal("", content.Mid)
        Assert.Equal(lines[0], content.Post)
    | Error _ -> Assert.Fail("Error returned")

[<Fact>]
let ``RangeOfPositions with length zero returns empty string from empty line`` () =
    let lines = [| "" |]

    let range =
        OfPositions(
            RangeOfPositions.Create (Path "test.txt") (Pos.Create (Line 1) (Column 0)) (Pos.Create (Line 1) (Column 0))
        )

    match rangeContent lines range with
    | Ok content ->
        Assert.Equal("", content.Pre)
        Assert.Equal("", content.Mid)
        Assert.Equal("", content.Post)
    | Error _ -> Assert.Fail("Error returned")


[<Fact>]
let ``RangeOfPositions of first char returns correct string`` () =
    let lines = [| "0123" |]

    let range =
        OfPositions(
            RangeOfPositions.Create (Path "test.txt") (Pos.Create (Line 1) (Column 0)) (Pos.Create (Line 1) (Column 1))
        )

    match rangeContent lines range with
    | Ok content ->
        Assert.Equal("", content.Pre)
        Assert.Equal("0", content.Mid)
        Assert.Equal("123", content.Post)
    | Error _ -> Assert.Fail("Error returned")

[<Fact>]
let ``RangeOfPositions of last char returns correct string`` () =
    let lines = [| "0123" |]

    let range =
        OfPositions(
            RangeOfPositions.Create (Path "test.txt") (Pos.Create (Line 1) (Column 3)) (Pos.Create (Line 1) (Column 4))
        )

    match rangeContent lines range with
    | Ok content ->
        Assert.Equal("012", content.Pre)
        Assert.Equal("3", content.Mid)
        Assert.Equal("", content.Post)
    | Error _ -> Assert.Fail("Error returned")

[<Fact>]
let ``RangeOfPosition returns correct char`` () =
    let lines = [| "0123"; "45"; "678" |]

    let range =
        OfPosition(RangeOfPosition.Create (Path "test.txt") (Pos.Create (Line 2) (Column 1)))

    match rangeContent lines range with
    | Ok content ->
        Assert.Equal("4", content.Pre)
        Assert.Equal("5", content.Mid)
        Assert.Equal("", content.Post)
    | Error _ -> Assert.Fail("Error returned")

[<Fact>]
let ``Multiline RangeOfPositions covering all lines returns correct strings`` () =
    let lines = [| "0123"; "4567" |]

    let range =
        OfPositions(
            RangeOfPositions.Create (Path "test.txt") (Pos.Create (Line 1) (Column 0)) (Pos.Create (Line 2) (Column 4))
        )

    match rangeContent lines range with
    | Ok content ->
        Assert.Equal("", content.Pre)
        Assert.Equal(lines |> String.concat Environment.NewLine, content.Mid)
        Assert.Equal("", content.Post)
    | Error _ -> Assert.Fail("Error returned")

[<Fact>]
let ``Multiline RangeOfPositions covering part of lines returns correct strings`` () =
    let lines = [| "0123"; "4567"; "890" |]

    let range =
        OfPositions(
            RangeOfPositions.Create (Path "test.txt") (Pos.Create (Line 1) (Column 3)) (Pos.Create (Line 3) (Column 1))
        )

    match rangeContent lines range with
    | Ok content ->
        Assert.Equal("012", content.Pre)
        let expected = "3" + Environment.NewLine + lines[1] + Environment.NewLine + "8"
        Assert.Equal(expected, content.Mid)
        Assert.Equal("90", content.Post)
    | Error _ -> Assert.Fail("Error returned")

[<Fact>]
let ``RangeOfLine with length zero returns single line from non-empty line`` () =
    let lines = [| "0123" |]

    let range = OfLines(RangeOfLines.Create (Path "test.txt") (Line 1) (Line 1))

    match rangeContent lines range with
    | Ok content ->
        Assert.Equal("", content.Pre)
        Assert.Equal(lines[0], content.Mid)
        Assert.Equal("", content.Post)
    | Error _ -> Assert.True(false)

[<Fact>]
let ``Multiline RangeOfLine returns correct strings`` () =
    let lines = [| "0123"; "45"; "6789" |]

    let range = OfLines(RangeOfLines.Create (Path "test.txt") (Line 1) (Line 3))

    match rangeContent lines range with
    | Ok content ->
        Assert.Equal("", content.Pre)
        let expected = lines |> String.concat Environment.NewLine
        Assert.Equal(expected, content.Mid)
        Assert.Equal("", content.Post)
    | Error _ -> Assert.True(false)
