function main {
    step { Restore }
    step { Build }
    step { Test }

    if ($env:APPVEYOR_BUILD_NUMBER -ne $NULL) {
        step { Pack }
    }
}

function Restore {
    exec { & dotnet restore --verbosity Warning }
}

function Build() {
    exec { & dotnet build .\src\Parsley --configuration Release }
    exec { & dotnet build .\src\Parsley.Tests --configuration Release }
}

function Test {
    exec { & dotnet test .\src\Parsley.Tests --configuration Release }
}

function Pack {
    if ([System.IO.Directory]::Exists(".\artifacts")) {
        [System.IO.Directory]::Delete(".\artifacts", $true)
    }
    $revision = $env:APPVEYOR_BUILD_NUMBER
    $revision = "{0:D4}" -f [convert]::ToInt32($revision, 10)
    exec { & dotnet pack .\src\Parsley --configuration Release --output .\artifacts --version-suffix=$revision }
}

function step($block) {
    $name = $block.ToString().Trim().Split(' ')[0]
    write-host "Executing $name" -fore CYAN
    &$block
}

function exec($cmd) {
    $global:lastexitcode = 0
    & $cmd
    if ($lastexitcode -ne 0) {
        throw "Error executing command:$cmd"
    }
}

try {
    main
    write-host
    write-host "Build Succeeded!" -fore GREEN
    exit 0
} catch [Exception] {
    write-host
    write-host $_.Exception.Message -fore DARKRED
    write-host
    write-host "Build Failed!" -fore DARKRED
    exit 1
}