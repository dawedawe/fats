namespace Fats

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
