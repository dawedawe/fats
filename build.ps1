rm -Recurse -Force -ErrorAction Ignore ./src/Fats/bin
rm -Recurse -Force -ErrorAction Ignore ./src/Fats/obj
rm -Recurse -Force -ErrorAction Ignore ./src/Fats.Tests/bin
rm -Recurse -Force -ErrorAction Ignore ./src/Fats.Tests/obj
rm -Recurse -Force -ErrorAction Ignore ./nupkg
dotnet tool restore
dotnet fantomas .
dotnet build -c Release
dotnet test -c Release
dotnet pack -c Release
