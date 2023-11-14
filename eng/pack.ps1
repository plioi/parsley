. (Join-Path $PSScriptRoot utilities)

if (test-path packages) { remove-item packages -Recurse }

exec { bash ./build }
exec { dotnet pack src -c Release -o packages --no-build --nologo }
