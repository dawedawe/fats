namespace Fats

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
