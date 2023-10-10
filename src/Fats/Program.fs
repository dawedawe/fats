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

    type Column =
        | Column of int

        member this.Value =
            match this with
            | Column p -> p

    type Pos =
        { Line: Line
          Column: Column }

        override this.ToString() =
            $"{this.Line.Value},{this.Column.Value}"

        static member Zero = { Line = Line 0; Column = Column 0 }

        static member Create line column = { Line = line; Column = column }

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
            $"{this.File.Value}:({this.Start.ToString()}-{this.End.ToString()})"

        member this.IsValid =
            if this.Start.Line = this.End.Line then
                this.Start.Column.Value <= this.End.Column.Value
            else
                this.Start.Line < this.End.Line

        static member Create path start ``end`` =
            { File = Path path
              Start = start
              End = ``end`` }

    type RangeOfLines =
        { File: Path
          Start: Line
          End: Line }

        override this.ToString() =
            $"{this.File.Value}:({this.Start.Value}-{this.End.Value})"

        member this.IsValid =
            this.Start.Value <= this.End.Value && this.Start.Value > 0 && this.End.Value > 0

        static member Create path start ``end`` =
            { File = Path path
              Start = start
              End = ``end`` }

    type Range =
        | OfPositions of RangeOfPositions
        | OfLines of RangeOfLines

        member this.File =
            match this with
            | OfPositions r -> r.File
            | OfLines r -> r.File

module ArgsParser =

    open System.Text.RegularExpressions
    open Model

    let ofPositionsRegex = Regex(@"(.+):(\((\d+),(\d+)-+(\d+),(\d+)\))")
    let ofLinesRegex = Regex(@"(.+):(\((\d+)-+(\d+)\))")

    let (|IsOfPositions|_|) s =
        let ofPositionsMatch = ofPositionsRegex.Match(s)

        if ofPositionsMatch.Success then
            let file = ofPositionsMatch.Groups[1].Value
            let startLine = int ofPositionsMatch.Groups[3].Value
            let startCol = int ofPositionsMatch.Groups[4].Value
            let endLine = int ofPositionsMatch.Groups[5].Value
            let endCol = int ofPositionsMatch.Groups[6].Value

            Some(
                OfPositions(
                    (RangeOfPositions.Create
                        file
                        (Pos.Create (Line startLine) (Column startCol))
                        (Pos.Create (Line endLine) (Column endCol)))
                )
            )
        else
            None

    let (|IsOfLines|_|) s =
        let ofLinesMatch = ofLinesRegex.Match(s)

        if ofLinesMatch.Success then
            let file = ofLinesMatch.Groups[1].Value
            let startLine = int ofLinesMatch.Groups[3].Value
            let endLine = int ofLinesMatch.Groups[4].Value

            Some(OfLines(RangeOfLines.Create file (Line startLine) (Line endLine)))
        else
            None

    let parse (args: string array) =
        let f (rangesAcc, invalidAcc) arg =
            match arg with
            | IsOfPositions r
            | IsOfLines r -> Array.append rangesAcc [| r |], invalidAcc
            | _ -> rangesAcc, Array.append invalidAcc [| arg |]

        Array.fold f (Array.empty, Array.empty) args

module Core =

    open Model

    let rangeOfPositionsContent (range: RangeOfPositions) (lines: string array) =
        if range.End.Line.Value <= lines.Length && range.IsValid then
            let linesInRange = lines[range.Start.Line.Value0 .. range.End.Line.Value0]

            if
                range.Start.Column.Value > (Array.head linesInRange).Length
                || range.End.Column.Value > (Array.last linesInRange).Length
            then
                $"Invalid range {range.ToString()}" |> Array.singleton
            else if range.Start.Line = range.End.Line then
                [| linesInRange[0]
                       .Substring(range.Start.Column.Value, range.End.Column.Value - range.Start.Column.Value) |]

            else
                linesInRange[0] <- linesInRange[0].Substring(range.Start.Column.Value)

                linesInRange[linesInRange.Length - 1] <-
                    linesInRange[linesInRange.Length - 1].Substring(0, range.End.Column.Value)

                linesInRange
        else
            $"Invalid range {range.ToString()}" |> Array.singleton

    let rangeOfLinesContent (range: RangeOfLines) (lines: string array) =
        if range.End.Value <= lines.Length && range.IsValid then
            lines[range.Start.Value0 .. range.End.Value0]
        else
            $"Invalid range {range.ToString()}" |> Array.singleton

    let rangeContent (lines: string array) (range: Range) =
        match range with
        | OfPositions range -> rangeOfPositionsContent range lines
        | OfLines range -> rangeOfLinesContent range lines

    let fileContent (path: Path, ranges: Range array) =
        if System.IO.File.Exists path.Value then
            let lines = System.IO.File.ReadAllLines(path.Value)
            ranges |> Array.map (rangeContent lines) |> Array.concat
        else
            sprintf "File not found: %s" path.Value |> Array.singleton

module IO =

    let output lines =
        lines |> Array.iter (fun s -> printfn "%s" s)

module Main =

    open ArgsParser

    [<EntryPoint>]
    let main argv =
        if Array.isEmpty argv then
            printfn "Usage: fats <path> [<path> ...]"
            1
        else
            parse argv
            |> fun (ranges, invalidArgs) ->
                invalidArgs |> Array.iter (fun a -> printfn $"invalid argument: \"{a}\"")

                ranges
                |> Array.groupBy (fun r -> r.File)
                |> Array.iter (Core.fileContent >> IO.output)

            0
