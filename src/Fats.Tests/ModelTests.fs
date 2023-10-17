module Fats.ModelTests

open Xunit
open Fats.Model
open Fats.TestHelpers

let validRanges =
    SeqTheoryData<Range>(
        [ OfPositions(
              RangeOfPositions.Create
                  (Path "test.txt")
                  (Pos.Create (Line 1) (Column 0))
                  (Pos.Create (Line 1) (Column 0))
          )
          OfPositions(
              RangeOfPositions.Create
                  (Path "test.txt")
                  (Pos.Create (Line 1) (Column 0))
                  (Pos.Create (Line 2) (Column 0))
          )
          OfPosition(RangeOfPosition.Create (Path "test.txt") (Pos.Create (Line 1) (Column 0)))
          OfLines(RangeOfLines.Create (Path "test.txt") (Line 1) (Line 1))
          OfLines(RangeOfLines.Create (Path "test.txt") (Line 1) (Line 2)) ]
    )

[<Theory>]
[<MemberData(nameof (validRanges))>]
let ``Valid ranges are valid`` (r: Range) = Assert.True(r.IsValid)

let invalidRanges =
    SeqTheoryData<Range>(
        [ OfPositions(
              RangeOfPositions.Create
                  (Path "test.txt")
                  (Pos.Create (Line 1) (Column 1))
                  (Pos.Create (Line 1) (Column 0))
          )
          OfPositions(
              RangeOfPositions.Create
                  (Path "test.txt")
                  (Pos.Create (Line 2) (Column 0))
                  (Pos.Create (Line 1) (Column 0))
          )
          OfPositions(
              RangeOfPositions.Create
                  (Path "test.txt")
                  (Pos.Create (Line 0) (Column 0))
                  (Pos.Create (Line 2) (Column 0))
          )
          OfPositions(
              RangeOfPositions.Create
                  (Path "test.txt")
                  (Pos.Create (Line 1) (Column 0))
                  (Pos.Create (Line 0) (Column 0))
          )
          OfPosition(RangeOfPosition.Create (Path "test.txt") (Pos.Create (Line 0) (Column 0)))
          OfPosition(RangeOfPosition.Create (Path "test.txt") (Pos.Create (Line 1) (Column -1)))
          OfLines(RangeOfLines.Create (Path "test.txt") (Line 1) (Line 0))
          OfLines(RangeOfLines.Create (Path "test.txt") (Line 0) (Line 1))
          OfLines(RangeOfLines.Create (Path "test.txt") (Line 2) (Line 1)) ]
    )

[<Theory>]
[<MemberData(nameof (invalidRanges))>]
let ``Invalid ranges are invalid`` (r: Range) = Assert.False(r.IsValid)
