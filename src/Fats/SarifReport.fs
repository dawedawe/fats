namespace Fats

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

    let output dumpConf (results: Result<SarifItemContent array, string>) =
        match results with
        | Ok results ->
            results
            |> Array.iter (fun itemContent ->
                printfn $"{itemContent.Item.RuleId}: {itemContent.Item.Message}\n{itemContent.Item.Range.ToString()}"

                match itemContent.Content with
                | Ok rangeContent -> printer dumpConf rangeContent
                | Error e -> printfn $"%s{e}"

                printfn "---")
        | Error e -> printfn $"%s{e}"
