$ErrorActionPreference = "Stop"

function exec($command) {
    write-host ([Environment]::NewLine + $command.ToString().Trim()) -fore CYAN

    & $command
    if ($lastexitcode -ne 0) { throw $lastexitcode }
}
