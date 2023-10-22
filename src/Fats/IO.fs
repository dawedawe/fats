namespace Fats

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
