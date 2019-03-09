if (Test-Path .\artifacts) { Remove-Item .\artifacts -Force -Recurse }
dotnet build
dotnet test