namespace Fats

module Main =

    open System
    open System.IO
    open Argu
    open ArgsParser
    open Model

    type CliArguments =
        | [<Unique>] NoMarkup
        | [<Unique>] NoPrefix
        | [<Unique>] NoPostfix
        | [<MainCommand>] Paths of paths: string list

        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | NoMarkup -> "don't use console markup like bold or underline"
                | NoPrefix -> "don't dump line content before the range"
                | NoPostfix -> "don't dump line content after the range"
                | Paths _ -> "the ranges to dump"

    let parser = ArgumentParser.Create<CliArguments>()

    let handleSarifArg dumpConf path =
        if File.Exists path then
            path
            |> SarifReport.readLogFromDisk
            |> SarifReport.itemsFromLog
            |> Array.groupBy (fun r -> r.Range.File)
            |> Array.iter (SarifReport.fileContent >> (SarifReport.output dumpConf))

            0
        else
            printfn $"file does not exist %s{path}"
            1

    let handlePathArgs dumpConf paths =
        parse paths
        |> fun (ranges, invalidArgs) ->
            invalidArgs |> Array.iter (fun a -> printfn $"invalid argument: \"{a}\"")

            ranges
            |> Array.groupBy (fun r -> r.File)
            |> Array.iter (Core.fileContent >> IO.output dumpConf)

        0

    let handleSarifDirArg dumpConf path =
        Directory.EnumerateFiles(path, "*.sarif", SearchOption.AllDirectories)
        |> Seq.map (handleSarifArg dumpConf)
        |> Seq.max

    let usage exitCode =
        printfn $"{parser.PrintUsage()}"
        exitCode

    [<EntryPoint>]
    let main argv =

        let results = parser.Parse(inputs = argv, raiseOnUsage = false)

        if results.IsUsageRequested then
            usage 0
        else
            let dumpConf =
                { NoMarkup = results.Contains <@ NoMarkup @>
                  NoPrefix = results.Contains <@ NoPrefix @>
                  NoPostfix = results.Contains <@ NoPostfix @> }

            let paths = results.GetResult(<@ Paths @>, [])

            match paths with
            | [] -> usage 1
            | [ path ] when path.EndsWith(".sarif", StringComparison.Ordinal) -> handleSarifArg dumpConf path
            | [ path ] when Directory.Exists path -> handleSarifDirArg dumpConf path
            | paths -> handlePathArgs dumpConf paths
