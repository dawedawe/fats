#!/bin/sh

rm -rf ./src/Fats/bin/Release ./src/Fats/obj/Release ./src/Fats.Tests/bin/Release ./src/Fats.Tests/obj/Release ./nupkg
dotnet tool restore
dotnet fantomas .
dotnet build -c Release
dotnet test -c Release
dotnet pack -c Release
