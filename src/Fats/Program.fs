namespace Fats

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
            let paths = results.GetResult(<@ Paths @>, [])

            match paths with
            | [] -> usage 1
            | [ path ] when path.EndsWith(".sarif", StringComparison.Ordinal) -> handleSarifArg nomarkup path
            | paths -> handlePathArgs nomarkup paths
