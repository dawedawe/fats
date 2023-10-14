fats
====

fats is a cat-like dotnet tool to quickly dump multiple F# ranges from multiple files to stdout.  
Please note: F# ranges use 1-based lines and 0-based columns.

# Installation
```shell
dotnet tool install --global fats
```

# Usage
```
fats <path> [<path> ...]
```

A single `path` argument consists of a file path followed by a colon and a range in parentheses.  
Two types of ranges are currently supported:
- `(startLine-endLine)` - a range of lines
- `(startLine,startColumn-endLine,endColumn)` - a range of characters  
- `(line,column)` - a range of a single line  

Between start and end line/column numbers there can be a single dash or multiple dashes.

Examples:
```shell
> fats "./src/Fats/Program.fs:(1,0-1,4)" "./src/Fats/Program.fs:(3,0--4,0)" "./src/Fats/Program.fs:(5,0--5,10)"
name
module Model =

    type L
```

```shell
> fats "./src/Fats/Program.fs:(1-1)"
namespace Fats
```

```shell
> fats "./src/Fats/Program.fs:(1,3)"
namespace Fats
```
