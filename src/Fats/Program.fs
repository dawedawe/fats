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

module ArgsParser =

    open System.Text.RegularExpressions
    open Model

    let ofPositionsRegex =
        Regex(@"(?<file>.+[^:])(:?)(\((?<sline>\d+),(?<scol>\d+)-+(?<eline>\d+),(?<ecol>\d+)\))")

    let ofPositionRegex = Regex(@"(?<file>.+[^:])(:?)(\((?<line>\d+),(?<col>\d+)\))")

    let ofLinesRegex = Regex(@"(?<file>.+[^:])(:?)(\((?<sline>\d+)-+(?<eline>\d+)\))")

    let (|IsOfPositions|_|) s =
        let ofPositionsMatch = ofPositionsRegex.Match(s)

        if ofPositionsMatch.Success then
            let file = ofPositionsMatch.Groups["file"].Value
            let startLine = int ofPositionsMatch.Groups["sline"].Value
            let startCol = int ofPositionsMatch.Groups["scol"].Value
            let endLine = int ofPositionsMatch.Groups["eline"].Value
            let endCol = int ofPositionsMatch.Groups["ecol"].Value

            Some(
                OfPositions(
                    (RangeOfPositions.Create
                        (Path file)
                        (Pos.Create (Line startLine) (Column startCol))
                        (Pos.Create (Line endLine) (Column endCol)))
                )
            )
        else
            None

    let (|IsOfPosition|_|) s =
        let ofPositionMatch = ofPositionRegex.Match(s)

        if ofPositionMatch.Success then
            let file = ofPositionMatch.Groups["file"].Value
            let line = int ofPositionMatch.Groups["line"].Value
            let col = int ofPositionMatch.Groups["col"].Value

            Some(OfPosition((RangeOfPosition.Create (Path file) (Pos.Create (Line line) (Column col)))))
        else
            None

    let (|IsOfLines|_|) s =
        let ofLinesMatch = ofLinesRegex.Match(s)

        if ofLinesMatch.Success then
            let file = ofLinesMatch.Groups["file"].Value
            let startLine = int ofLinesMatch.Groups["sline"].Value
            let endLine = int ofLinesMatch.Groups["eline"].Value

            Some(OfLines(RangeOfLines.Create (Path file) (Line startLine) (Line endLine)))
        else
            None

    let parse (args: string array) =
        let f (rangesAcc, invalidAcc) arg =
            match arg with
            | IsOfPositions r
            | IsOfPosition r
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

    /// <summary>give back the whole line of the position</summary>
    /// <param name="range"></param>
    /// <param name="lines"></param>
    /// <returns></returns>
    let rangeOfPositionContent (range: RangeOfPosition) (lines: string array) =
        if range.Position.Line.Value <= lines.Length && range.IsValid then
            let lineInRange = lines[range.Position.Line.Value0]

            if range.Position.Column.Value > lineInRange.Length then
                $"Invalid range {range.ToString()}" |> Array.singleton
            else
                lineInRange |> Array.singleton
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
        | OfPosition range -> rangeOfPositionContent range lines
        | OfLines range -> rangeOfLinesContent range lines

    let fileContent (path: Path, ranges: Range array) =
        if System.IO.File.Exists path.Value then
            let lines = System.IO.File.ReadAllLines(path.Value)
            ranges |> Array.map (rangeContent lines) |> Array.concat
        else
            sprintf "File not found: %s" path.Value |> Array.singleton

module SarifReport =

    open Microsoft.CodeAnalysis.Sarif
    open Core
    open Model

    type SarifItem =
        { RuleId: string
          Message: string
          Range: Range }

    let fromPhysicalLocation (loc: PhysicalLocation) =
        let file = Path loc.ArtifactLocation.Uri.AbsolutePath
        let startLine = Line loc.Region.StartLine
        let startColumn = Column loc.Region.StartColumn
        OfPosition(RangeOfPosition.Create file (Pos.Create startLine startColumn))

    let readLog path =
        let text = System.IO.File.ReadAllText(path)
        let sarifLog = Newtonsoft.Json.JsonConvert.DeserializeObject<SarifLog>(text)
        sarifLog

    let rangesFromLog (log: SarifLog) =
        [| for run in log.Runs do
               for result in run.Results do
                   for loc in result.Locations do
                       { Range = fromPhysicalLocation loc.PhysicalLocation
                         RuleId = result.RuleId
                         Message = result.Message.Text } |]


    let fileContent (path: Path, sarifItems: SarifItem array) =
        if System.IO.File.Exists path.Value then
            let lines = System.IO.File.ReadAllLines(path.Value)

            sarifItems
            |> Array.map (fun item ->
                let content = rangeContent lines item.Range
                Array.append [| $"{item.RuleId}: {item.Message}\n{item.Range.ToString()}" |] content)
            |> Array.map (fun c -> Array.append c [| "---" |])
            |> Array.concat
        else
            sprintf "File not found: %s" path.Value |> Array.singleton

module IO =

    let output lines =
        lines |> Array.iter (fun s -> printfn "%s" s)

module Main =

    open System
    open System.IO
    open ArgsParser

    [<EntryPoint>]
    let main argv =
        if Array.isEmpty argv then
            printfn "Usage: fats <path> [<path> ...]"
            1
        else if argv.Length = 1 && argv[0].EndsWith(".sarif", StringComparison.Ordinal) then
            if File.Exists argv[0] then
                argv[0]
                |> SarifReport.readLog
                |> SarifReport.rangesFromLog
                |> Array.groupBy (fun r -> r.Range.File)
                |> Array.iter (SarifReport.fileContent >> IO.output)

                0
            else
                printfn $"file does not exist %s{argv[0]}"
                1
        else
            parse argv
            |> fun (ranges, invalidArgs) ->
                invalidArgs |> Array.iter (fun a -> printfn $"invalid argument: \"{a}\"")

                ranges
                |> Array.groupBy (fun r -> r.File)
                |> Array.iter (Core.fileContent >> IO.output)

            0
