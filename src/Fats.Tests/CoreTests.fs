module CoreTests

open Xunit
open Fats.Core
open Fats.Model

[<Fact>]
let ``Range with length zero returns empty string from non-empty line`` () =
    let lines = [| "0123" |]

    let range =
        Range.Create "test.txt" (Pos.Create (Line 1) (Column 0)) (Pos.Create (Line 1) (Column 0))

    let content = getRangeContent range lines
    Assert.Equal(Assert.Single(content), "")

[<Fact>]
let ``Range with length zero returns empty string from empty line`` () =
    let lines = [| "" |]

    let range =
        Range.Create "test.txt" (Pos.Create (Line 1) (Column 0)) (Pos.Create (Line 1) (Column 0))

    let content = getRangeContent range lines
    Assert.Equal(Assert.Single(content), "")


[<Fact>]
let ``Range of first char returns correct string`` () =
    let lines = [| "0123" |]

    let range =
        Range.Create "test.txt" (Pos.Create (Line 1) (Column 0)) (Pos.Create (Line 1) (Column 1))

    let content = getRangeContent range lines
    Assert.Equal("0", Assert.Single(content))

[<Fact>]
let ``Range of last char returns correct string`` () =
    let lines = [| "0123" |]

    let range =
        Range.Create "test.txt" (Pos.Create (Line 1) (Column 3)) (Pos.Create (Line 1) (Column 4))

    let content = getRangeContent range lines
    Assert.Equal("3", Assert.Single(content))

[<Fact>]
let ``Multiline range covering all lines returns correct strings`` () =
    let lines = [| "0123"; "4567" |]

    let range =
        Range.Create "test.txt" (Pos.Create (Line 1) (Column 0)) (Pos.Create (Line 2) (Column 4))

    let content = getRangeContent range lines
    Assert.Equal(2, content.Length)
    Assert.Equal(lines[0], content[0])
    Assert.Equal(lines[1], content[1])

[<Fact>]
let ``Multiline range covering part of lines returns correct strings`` () =
    let lines = [| "0123"; "4567"; "890" |]

    let range =
        Range.Create "test.txt" (Pos.Create (Line 1) (Column 3)) (Pos.Create (Line 3) (Column 1))

    let content = getRangeContent range lines
    Assert.Equal(3, content.Length)
    Assert.Equal("3", content[0])
    Assert.Equal(lines[1], content[1])
    Assert.Equal("8", content[2])
