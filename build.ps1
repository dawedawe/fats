rm -Recurse -Force -ErrorAction Ignore ./src/Fats/bin/Release
rm -Recurse -Force -ErrorAction Ignore ./src/Fats/obj/Release
rm -Recurse -Force -ErrorAction Ignore ./src/Fats.Tests/bin/Release
rm -Recurse -Force -ErrorAction Ignore ./src/Fats.Tests/obj/Release
rm -Recurse -Force -ErrorAction Ignore ./nupkg
dotnet tool restore
dotnet fantomas .
dotnet build -c Release
dotnet test -c Release --no-build
dotnet pack -c Release --no-build
