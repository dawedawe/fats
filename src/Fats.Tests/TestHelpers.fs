module Fats.TestHelpers

open System.Collections.Generic
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
