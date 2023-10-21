module Fats.ArgsParserTests

open Xunit
open Fats.ArgsParser
open Fats.Model
open Fats.TestHelpers

let validArgsAndExpectedRanges =
    SeqTheoryTupleData<string, Range>(

        [ @".\a\b\c.fs:(1,0--2,0)",
          OfPositions(
              RangeOfPositions.Create
                  (Path @".\a\b\c.fs")
                  (Pos.Create (Line 1) (Column 0))
                  (Pos.Create (Line 2) (Column 0))
          )
          @".\a\b\c.fs(1,0--2,0)",
          OfPositions(
              RangeOfPositions.Create
                  (Path @".\a\b\c.fs")
                  (Pos.Create (Line 1) (Column 0))
                  (Pos.Create (Line 2) (Column 0))
          )
          @"./a/b/c.fs:(1,0-2,0)",
          OfPositions(
              RangeOfPositions.Create
                  (Path @"./a/b/c.fs")
                  (Pos.Create (Line 1) (Column 0))
                  (Pos.Create (Line 2) (Column 0))
          )
          @".\a\b\c.fs:(1--2)", OfLines(RangeOfLines.Create (Path @".\a\b\c.fs") (Line 1) (Line 2))
          @"./a/b/c.fs:(1-2)", OfLines(RangeOfLines.Create (Path @"./a/b/c.fs") (Line 1) (Line 2))
          @"./a/b/c.fs(1-2)", OfLines(RangeOfLines.Create (Path @"./a/b/c.fs") (Line 1) (Line 2))
          @"./a/b/c.fs(1,0)", OfPosition(RangeOfPosition.Create (Path @"./a/b/c.fs") (Pos.Create (Line 1) (Column 0)))
          @"./a/b/c.fsi:(1,0)",
          OfPosition(RangeOfPosition.Create (Path @"./a/b/c.fsi") (Pos.Create (Line 1) (Column 0))) ]
    )

[<Theory>]
[<MemberData(nameof (validArgsAndExpectedRanges))>]
let ``Valid args produce valid ranges`` (arg: string, expectedRange: Range) =
    let ranges, invalidArgs = parse [ arg ]
    Assert.Empty(invalidArgs)
    Assert.Equal(expectedRange, Assert.Single(ranges))

let invalidArgs =
    SeqTheoryData<string>([ "foo"; "foo:"; ":(1,0--2,0)"; "(1,0--2,0)" ])

[<Theory>]
[<MemberData(nameof (invalidArgs))>]
let ``Invalid args produce invalid ranges`` (arg: string) =
    let ranges, invalidArgs = parse [ arg ]
    Assert.Empty(ranges)
    Assert.Single(invalidArgs)
