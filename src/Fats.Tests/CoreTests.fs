module CoreTests

open Xunit
open Fats.Core
open Fats.Model

[<Fact>]
let ``RangeOfPositions with length zero returns empty string from non-empty line`` () =
    let lines = [| "0123" |]

    let range =
        OfPositions(
            RangeOfPositions.Create "test.txt" (Pos.Create (Line 1) (Column 0)) (Pos.Create (Line 1) (Column 0))
        )

    let content = rangeContent lines range
    Assert.Equal("", Assert.Single(content))

[<Fact>]
let ``RangeOfPositions with length zero returns empty string from empty line`` () =
    let lines = [| "" |]

    let range =
        OfPositions(
            RangeOfPositions.Create "test.txt" (Pos.Create (Line 1) (Column 0)) (Pos.Create (Line 1) (Column 0))
        )

    let content = rangeContent lines range
    Assert.Equal("", Assert.Single(content))


[<Fact>]
let ``RangeOfPositions of first char returns correct string`` () =
    let lines = [| "0123" |]

    let range =
        OfPositions(
            RangeOfPositions.Create "test.txt" (Pos.Create (Line 1) (Column 0)) (Pos.Create (Line 1) (Column 1))
        )

    let content = rangeContent lines range
    Assert.Equal("0", Assert.Single(content))

[<Fact>]
let ``RangeOfPositions of last char returns correct string`` () =
    let lines = [| "0123" |]

    let range =
        OfPositions(
            RangeOfPositions.Create "test.txt" (Pos.Create (Line 1) (Column 3)) (Pos.Create (Line 1) (Column 4))
        )

    let content = rangeContent lines range
    Assert.Equal("3", Assert.Single(content))

[<Fact>]
let ``Multiline RangeOfPositions covering all lines returns correct strings`` () =
    let lines = [| "0123"; "4567" |]

    let range =
        OfPositions(
            RangeOfPositions.Create "test.txt" (Pos.Create (Line 1) (Column 0)) (Pos.Create (Line 2) (Column 4))
        )

    let content = rangeContent lines range
    Assert.Equal(2, content.Length)
    Assert.Equal(lines[0], content[0])
    Assert.Equal(lines[1], content[1])

[<Fact>]
let ``Multiline RangeOfPositions covering part of lines returns correct strings`` () =
    let lines = [| "0123"; "4567"; "890" |]

    let range =
        OfPositions(
            RangeOfPositions.Create "test.txt" (Pos.Create (Line 1) (Column 3)) (Pos.Create (Line 3) (Column 1))
        )

    let content = rangeContent lines range
    Assert.Equal(3, content.Length)
    Assert.Equal("3", content[0])
    Assert.Equal(lines[1], content[1])
    Assert.Equal("8", content[2])

[<Fact>]
let ``RangeOfLine with length zero returns single line from non-empty line`` () =
    let lines = [| "0123" |]

    let range = OfLines(RangeOfLines.Create "test.txt" (Line 1) (Line 1))

    let content = rangeContent lines range
    Assert.Equal(lines[0], Assert.Single(content))

[<Fact>]
let ``Multiline RangeOfLine returns correct strings`` () =
    let lines = [| "0123"; "45"; "6789" |]

    let range = OfLines(RangeOfLines.Create "test.txt" (Line 1) (Line 3))

    let content = rangeContent lines range
    Assert.Equal(3, content.Length)
    Assert.Equal(lines[0], content[0])
    Assert.Equal(lines[1], content[1])
    Assert.Equal(lines[2], content[2])
