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

    let parse (args: string list) =
        let f (rangesAcc, invalidAcc) arg =
            match arg with
            | IsOfPositions r
            | IsOfPosition r
            | IsOfLines r -> Array.append rangesAcc [| r |], invalidAcc
            | _ -> rangesAcc, Array.append invalidAcc [| arg |]

        List.fold f (Array.empty, Array.empty) args

module Core =

    open Model

    let rangeOfPositionsContent (range: RangeOfPositions) (lines: string array) =
        if range.End.Line.Value <= lines.Length && range.IsValid then
            let linesInRange = lines[range.Start.Line.Value0 .. range.End.Line.Value0]

            if
                range.Start.Column.Value > (Array.head linesInRange).Length
                || range.End.Column.Value > (Array.last linesInRange).Length
            then
                Error $"Invalid range {range.ToString()}"
            else if range.Start.Line = range.End.Line then
                let pre = linesInRange[0].Substring(0, range.Start.Column.Value)

                let mid =
                    linesInRange[0]
                        .Substring(range.Start.Column.Value, range.End.Column.Value - range.Start.Column.Value)

                let post = linesInRange[0].Substring(range.End.Column.Value)

                let content =
                    { Source = OfPositions range
                      Pre = pre
                      Mid = mid
                      Post = post }

                Ok content

            else
                let linesUpperBound = linesInRange.Length - 1
                let pre = linesInRange[0].Substring(0, range.Start.Column.Value)

                let mid =
                    seq {
                        yield linesInRange[0].Substring(range.Start.Column.Value)

                        for i in range.Start.Line.Value0 + 1 .. range.End.Line.Value0 - 1 do
                            yield linesInRange[i]

                        yield linesInRange[linesUpperBound].Substring(0, range.End.Column.Value)
                    }
                    |> String.concat System.Environment.NewLine

                let post = linesInRange[linesUpperBound].Substring(range.End.Column.Value)

                let content =
                    { Source = OfPositions range
                      Pre = pre
                      Mid = mid
                      Post = post }

                Ok content
        else
            Error $"Invalid range {range.ToString()}"

    /// <summary>give back the whole line of the position</summary>
    /// <param name="range"></param>
    /// <param name="lines"></param>
    /// <returns></returns>
    let rangeOfPositionContent (range: RangeOfPosition) (lines: string array) =
        if range.Position.Line.Value <= lines.Length && range.IsValid then
            let lineInRange = lines[range.Position.Line.Value0]

            if range.Position.Column.Value > lineInRange.Length then
                Error $"Invalid range {range.ToString()}"
            else
                let pre = lineInRange.Substring(0, range.Position.Column.Value)
                let mid = lineInRange.Substring(range.Position.Column.Value, 1)
                let post = lineInRange.Substring(range.Position.Column.Value + 1)

                let content =
                    { Source = OfPosition range
                      Pre = pre
                      Mid = mid
                      Post = post }

                Ok content
        else
            Error $"Invalid range {range.ToString()}"

    let rangeOfLinesContent (range: RangeOfLines) (lines: string array) =
        if range.End.Value <= lines.Length && range.IsValid then
            let mid =
                lines[range.Start.Value0 .. range.End.Value0]
                |> String.concat System.Environment.NewLine

            let content =
                { Source = OfLines range
                  Pre = ""
                  Mid = mid
                  Post = "" }

            Ok content
        else
            Error $"Invalid range {range.ToString()}"

    let rangeContent (lines: string array) (range: Range) =
        match range with
        | OfPositions range -> rangeOfPositionsContent range lines
        | OfPosition range -> rangeOfPositionContent range lines
        | OfLines range -> rangeOfLinesContent range lines

    let fileContent (path: Path, ranges: Range array) =
        if System.IO.File.Exists path.Value then
            let lines = System.IO.File.ReadAllLines(path.Value)
            ranges |> Array.map (rangeContent lines)
        else
            $"File not found: %s{path.Value}" |> Error |> Array.singleton

module IO =

    open System
    open Spectre.Console
    open Model

    let printer nomarkup content =
        let nomarkup =
            match content.Source with
            | OfLines _ -> true // because marking up all content makes no sense
            | _ -> nomarkup

        if nomarkup then
            printfn $"%s{content.Pre}%s{content.Mid}%s{content.Post}"
        else
            AnsiConsole.Markup(
                "{0}[underline bold]{1}[/]{2}{3}",
                Markup.Escape(content.Pre),
                Markup.Escape(content.Mid),
                Markup.Escape(content.Post),
                Environment.NewLine
            )

    let output nomarkup (results: Result<Content, string> array) =

        results
        |> Array.iter (fun r ->
            match r with
            | Ok content -> printer nomarkup content
            | Error e -> printfn $"%s{e}"

        )

module SarifReport =

    open Microsoft.CodeAnalysis.Sarif
    open Core
    open Model
    open IO

    type SarifItem =
        { RuleId: string
          Message: string
          Range: Range }

    type SarifItemContent =
        { Item: SarifItem
          Content: Result<Content, string> }

    let fromPhysicalLocation (loc: PhysicalLocation) =
        let file = Path loc.ArtifactLocation.Uri.AbsolutePath
        let startLine = Line loc.Region.StartLine
        let startColumn = Column loc.Region.StartColumn
        OfPosition(RangeOfPosition.Create file (Pos.Create startLine startColumn))

    let readLog (rawText: unit -> string) =
        let text = rawText ()
        let sarifLog = Newtonsoft.Json.JsonConvert.DeserializeObject<SarifLog>(text)
        sarifLog

    let readLogFromDisk path =
        let rawText = fun () -> System.IO.File.ReadAllText(path)
        readLog rawText

    let itemsFromLog (log: SarifLog) =
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
                { Item = item; Content = content })
            |> Ok
        else
            $"File not found: %s{path.Value}" |> Error

    let output nomarkup (results: Result<SarifItemContent array, string>) =
        match results with
        | Ok results ->
            results
            |> Array.iter (fun itemContent ->
                printfn $"{itemContent.Item.RuleId}: {itemContent.Item.Message}\n{itemContent.Item.Range.ToString()}"

                match itemContent.Content with
                | Ok rangeContent -> printer nomarkup rangeContent
                | Error e -> printfn $"%s{e}"

                printfn "---")
        | Error e -> printfn $"%s{e}"

module Main =

    open System
    open System.IO
    open Argu
    open ArgsParser

    type CliArguments =
        | [<Unique>] NoMarkup
        | [<MainCommand>] Paths of paths: string list

        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | NoMarkup _ -> "don't use console markup like bold or underline"
                | Paths _ -> "the ranges to dump"

    let parser = ArgumentParser.Create<CliArguments>()

    let handleSarifArg nomarkup path =
        if File.Exists path then
            path
            |> SarifReport.readLogFromDisk
            |> SarifReport.itemsFromLog
            |> Array.groupBy (fun r -> r.Range.File)
            |> Array.iter (SarifReport.fileContent >> (SarifReport.output nomarkup))

            0
        else
            printfn $"file does not exist %s{path}"
            1

    let handlePathArgs nomarkup paths =
        parse paths
        |> fun (ranges, invalidArgs) ->
            invalidArgs |> Array.iter (fun a -> printfn $"invalid argument: \"{a}\"")

            ranges
            |> Array.groupBy (fun r -> r.File)
            |> Array.iter (Core.fileContent >> IO.output nomarkup)

        0

    let usage exitCode =
        printfn $"{parser.PrintUsage()}"
        exitCode

    [<EntryPoint>]
    let main argv =

        let results = parser.Parse(inputs = argv, raiseOnUsage = false)

        if results.IsUsageRequested then
            usage 0
        else
            let nomarkup = results.Contains <@ NoMarkup @>
            let paths = results.TryGetResult <@ Paths @> |> Option.defaultValue []

            match paths with
            | [] -> usage 1
            | [ path ] when path.EndsWith(".sarif", StringComparison.Ordinal) -> handleSarifArg nomarkup path
            | paths -> handlePathArgs nomarkup paths
