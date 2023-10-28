namespace Fats

module IO =

    open System
    open Spectre.Console
    open Model

    let printer dumpConf content =
        let nomarkup =
            match content.Source with
            | OfLines _ -> true // because marking up all content makes no sense
            | _ -> dumpConf.NoMarkup

        let prefix = if dumpConf.NoPrefix then String.Empty else content.Pre
        let postfix = if dumpConf.NoPostfix then String.Empty else content.Post

        if nomarkup then
            printfn $"%s{prefix}%s{content.Mid}%s{postfix}"
        else
            AnsiConsole.Markup(
                "{0}[underline bold]{1}[/]{2}{3}",
                Markup.Escape(prefix),
                Markup.Escape(content.Mid),
                Markup.Escape(postfix),
                Environment.NewLine
            )

    let output nomarkup (results: Result<Content, string> array) =

        results
        |> Array.iter (fun r ->
            match r with
            | Ok content -> printer nomarkup content
            | Error e -> printfn $"%s{e}"

        )
