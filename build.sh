#!/bin/sh

rm -rf ./src/Fats/bin ./src/Fats/obj ./src/Fats.Tests/bin ./src/Fats.Tests/obj ./nupkg
dotnet tool restore
dotnet fantomas .
dotnet build
dotnet test
dotnet pack
