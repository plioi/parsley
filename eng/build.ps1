. (Join-Path $PSScriptRoot utilities)

exec { dotnet --info }
exec { dotnet tool restore }
exec { dotnet clean src -c Release --nologo -v minimal }
exec { dotnet build src -c Release --nologo }
exec { dotnet fixie *.Tests -c Release --no-build }
