module Fats.TestHelpers

open System
open System.Diagnostics
open System.Collections.Generic
open System.IO
open Xunit

type SeqTheoryData<'t>(data: IEnumerable<'t>) =
    inherit TheoryData<'t>()

    do
        for d in data do
            base.Add(d)

type SeqTheoryTupleData<'t1, 't2>(data: IEnumerable<('t1 * 't2)>) =
    inherit TheoryData<'t1, 't2>()

    do
        for (d1, d2) in data do
            base.Add(d1, d2)

type TmpFile(content: string) =

    let path = Path.Join(Path.GetTempPath(), Guid.NewGuid().ToString("N"))

    do File.WriteAllText(path, content)

    member _.Path = path

    interface IDisposable with
        member _.Dispose() : unit = File.Delete(path)

let runFats args =
    let configuration =
#if DEBUG
        "Debug"
#else
        "Release"
#endif

    let assemblyPath =
        Path.Combine("..", "..", "..", "..", "Fats", "bin", configuration, "net8.0", "fats.dll")

    let info = ProcessStartInfo("dotnet", $"{assemblyPath} {args}")
    info.CreateNoWindow <- true
    info.RedirectStandardError <- true
    info.RedirectStandardOutput <- true
    use proc = new Process()
    proc.StartInfo <- info
    proc.Start() |> ignore

    if proc.WaitForExit(TimeSpan.FromMinutes(3.0)) then

        let stdOut = proc.StandardOutput.ReadToEnd().ReplaceLineEndings("\n")
        let stdError = proc.StandardError.ReadToEnd().ReplaceLineEndings("\n")
        stdOut, stdError, proc.ExitCode
    else
        failwith "fats process did not exit"
