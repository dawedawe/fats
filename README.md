fats
====

fats is a cat-like dotnet tool to quickly dump multiple F# ranges from multiple files to the console.

Example:
```shell
> fats.exe ".\src\Fats\Program.fs:(1,0--2,0)" ".\src\Fats\Program.fs:(2,0--3,0)" ".\src\Fats\Program.fs:(3,0--4,0)"
name
module Model =

        | Line of int
                   
```