namespace Fats

module Model =

    type Line =
        | Line of int

        member this.Value =
            match this with
            | Line p -> p

    type Column =
        | Column of int

        member this.Value =
            match this with
            | Column p -> p

    type Pos =
        { Line: Line
          Column: Column }

        static member Zero = { Line = Line 0; Column = Column 0 }
        static member Create line column = { Line = line; Column = column }

    type Path =
        | Path of string

        member this.Value =
            match this with
            | Path p -> p

    type Range =
        { File: Path
          Start: Pos
          End: Pos }

        static member Create path start ``end`` =
            { File = Path path
              Start = start
              End = ``end`` }

module ArgsParser =

    open System.Text.RegularExpressions
    open Model

    let parse (args: string array) =

        let regex = Regex(@"(.+):(\((\d+),(\d+)-+(\d+),(\d+)\))")

        [| for arg in args do
               let regMatch = regex.Match(arg)

               if regMatch.Success then
                   let file = regMatch.Groups.[1].Value
                   let startLine = int regMatch.Groups.[3].Value
                   let startCol = int regMatch.Groups.[4].Value
                   let endLine = int regMatch.Groups.[5].Value
                   let endCol = int regMatch.Groups.[6].Value

                   yield
                       (Range.Create
                           file
                           (Pos.Create (Line startLine) (Column startCol))
                           (Pos.Create (Line endLine) (Column endCol))) |]

module Core =

    open Model

    let getRangeContent (range: Range) (lines: string array) =
        if range.End.Line.Value <= lines.Length then
            let linesInRange = lines[range.Start.Line.Value - 1 .. range.End.Line.Value - 1]

            if
                range.Start.Column.Value > (Array.head linesInRange).Length
                || range.End.Column.Value > (Array.last linesInRange).Length
            then
                sprintf "Invalid range for: %s" range.File.Value |> Array.singleton
            else if range.Start.Line = range.End.Line then
                [| linesInRange[0]
                       .Substring(range.Start.Column.Value, range.End.Column.Value - range.Start.Column.Value) |]

            else
                linesInRange[0] <- linesInRange[0].Substring(range.Start.Column.Value)

                linesInRange[linesInRange.Length - 1] <-
                    linesInRange[linesInRange.Length - 1].Substring(0, range.End.Column.Value)

                linesInRange
        else
            sprintf "Invalid range for: %s" range.File.Value |> Array.singleton


    let getFileContent (range: Range) =
        let path = range.File.Value

        if System.IO.File.Exists path then
            System.IO.File.ReadAllLines(path) |> getRangeContent range
        else
            sprintf "File not found: %s" path |> Array.singleton

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
            |> Array.iter (fun p ->
                let content = Core.getFileContent p
                IO.output content)

            0
