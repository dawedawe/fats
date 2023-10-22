namespace Fats

module Model =

    type Line =
        | Line of int

        member this.Value =
            match this with
            | Line p -> p

        member this.Value0 =
            match this with
            | Line p -> p - 1

        member this.IsValid = this.Value >= 1

    type Column =
        | Column of int

        member this.Value =
            match this with
            | Column p -> p

        member this.IsValid = this.Value >= 0

    type Pos =
        { Line: Line
          Column: Column }

        override this.ToString() =
            $"{this.Line.Value},{this.Column.Value}"

        static member Create line column = { Line = line; Column = column }

        member this.IsValid = this.Line.IsValid && this.Column.IsValid

    type Path =
        | Path of string

        member this.Value =
            match this with
            | Path p -> p

    type RangeOfPositions =
        { File: Path
          Start: Pos
          End: Pos }

        override this.ToString() =
            $"{this.File.Value}({this.Start.ToString()}-{this.End.ToString()})"

        member this.IsValid =
            this.Start.IsValid
            && this.End.IsValid
            && if this.Start.Line = this.End.Line then
                   this.Start.Column.Value <= this.End.Column.Value
               else
                   this.Start.Line < this.End.Line

        static member Create path start ``end`` =
            { File = path
              Start = start
              End = ``end`` }

    type RangeOfPosition =
        { File: Path
          Position: Pos }

        override this.ToString() =
            $"{this.File.Value}({this.Position.ToString()})"

        member this.IsValid = this.Position.IsValid

        static member Create path pos = { File = path; Position = pos }

    type RangeOfLines =
        { File: Path
          Start: Line
          End: Line }

        override this.ToString() =
            $"{this.File.Value}({this.Start.Value}-{this.End.Value})"

        member this.IsValid =
            this.Start.Value <= this.End.Value && this.Start.Value > 0 && this.End.Value > 0

        static member Create path start ``end`` =
            { File = path
              Start = start
              End = ``end`` }

    type Range =
        | OfPositions of RangeOfPositions
        | OfPosition of RangeOfPosition
        | OfLines of RangeOfLines

        member this.File =
            match this with
            | OfPositions r -> r.File
            | OfPosition r -> r.File
            | OfLines r -> r.File

        member this.IsValid =
            match this with
            | OfPositions r -> r.IsValid
            | OfPosition r -> r.IsValid
            | OfLines r -> r.IsValid

        override this.ToString() =
            match this with
            | OfPositions r -> r.ToString()
            | OfPosition r -> r.ToString()
            | OfLines r -> r.ToString()

    type Content =
        { Source: Range
          Pre: string
          Mid: string
          Post: string }
