. (Join-Path $PSScriptRoot utilities)

exec { dotnet run -c Release --project src/Parsley.Benchmark }
