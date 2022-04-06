. (Join-Path $PSScriptRoot utilities)

if (test-path artifacts) { remove-item artifacts -Recurse }

exec { bash ./build }
exec { dotnet pack src -c Release -o artifacts --no-build --nologo }
