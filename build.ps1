Framework '4.5.2'

properties {
    $birthYear = 2011
    $maintainers = "Patrick Lioi"

    $configuration = 'Release'
    $projects = @(gci src -rec -filter *.csproj)
    $version = '0.0.7'
}

task default -depends Test

task Package -depends Test {
    rd package -recurse -force -ErrorAction SilentlyContinue | out-null
    mkdir package -ErrorAction SilentlyContinue | out-null
    exec { & tools\NuGet.exe pack src\Parsley\Parsley.csproj -Symbols -Prop Configuration=$configuration -OutputDirectory package }

    write-host
    write-host "To publish these packages, issue the following command:"
    write-host "   tools\NuGet push package\Parsley.$version.nupkg"
}

task Test {
    generate-assembly-info
    generate-license
    compile-solution

    $testRunners = @(gci src\packages -rec -filter xunit.console.exe)

    if ($testRunners.Length -ne 1)
    {
        throw "Expected to find 1 xunit.console.exe, but found $($testRunners.Length)."
    }

    $testRunner = $testRunners[0].FullName

    foreach ($project in $projects)
    {
        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project)

        if ($projectName.EndsWith("Test"))
        {
            $testAssembly = "$($project.Directory)\bin\$configuration\$projectName.dll"
            exec { & $testRunner $testAssembly }
        }
    }
}

function compile-solution {
    Set-Alias msbuild (get-msbuild-path)
    exec { msbuild /t:clean /v:q /nologo /p:Configuration=$configuration src\Parsley.sln }
    exec { msbuild /t:build /v:q /nologo /p:Configuration=$configuration src\Parsley.sln }
}

function generate-assembly-info {
    $assemblyVersion = $version
    if ($assemblyVersion.Contains("-")) {
        $assemblyVersion = $assemblyVersion.Substring(0, $assemblyVersion.IndexOf("-"))
    }

    $copyright = get-copyright

    foreach ($project in $projects) {
        $projectName = [System.IO.Path]::GetFileNameWithoutExtension($project)

        if ($projectName -eq "Build") {
            continue;
        }

        regenerate-file "$($project.DirectoryName)\Properties\AssemblyInfo.cs" @"
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: AssemblyProduct("Parsley")]
[assembly: AssemblyTitle("$projectName")]
[assembly: AssemblyVersion("$assemblyVersion")]
[assembly: AssemblyFileVersion("$assemblyVersion")]
[assembly: AssemblyInformationalVersion("$version")]
[assembly: AssemblyCopyright("$copyright")]
[assembly: AssemblyCompany("$maintainers")]
[assembly: AssemblyConfiguration("$configuration")]
"@
    }
}

function generate-license {
    $copyright = get-copyright

    regenerate-file "LICENSE.txt" @"
The MIT License (MIT)
$copyright

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
"@
}

function get-copyright {
    $date = Get-Date
    $year = $date.Year
    $copyrightSpan = if ($year -eq $birthYear) { $year } else { "$birthYear-$year" }
    return "Copyright © $copyrightSpan $maintainers"
}

function regenerate-file($path, $newContent) {
    $oldContent = [IO.File]::ReadAllText($path)

    if ($newContent -ne $oldContent) {
        $relativePath = Resolve-Path -Relative $path
        write-host "Generating $relativePath"
        [System.IO.File]::WriteAllText($path, $newContent, [System.Text.Encoding]::UTF8)
    }
}

function get-msbuild-path {
    [cmdletbinding()]
    param(
        [Parameter(Position=0)]
        [ValidateSet('32bit','64bit')]
        [string]$bitness = '32bit'
    )
    process{

        # Find the highest installed version of msbuild.exe.

        $regLocalKey = $null

        if($bitness -eq '32bit'){
            $regLocalKey = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine,[Microsoft.Win32.RegistryView]::Registry32)
        } else {
            $regLocalKey = [Microsoft.Win32.RegistryKey]::OpenBaseKey([Microsoft.Win32.RegistryHive]::LocalMachine,[Microsoft.Win32.RegistryView]::Registry64)
        }

        $versionKeyName = $regLocalKey.OpenSubKey('SOFTWARE\Microsoft\MSBuild\ToolsVersions\').GetSubKeyNames() | Sort-Object {[double]$_} -Descending

        $keyToReturn = ('SOFTWARE\Microsoft\MSBuild\ToolsVersions\{0}' -f $versionKeyName)

        $path = ( '{0}msbuild.exe' -f $regLocalKey.OpenSubKey($keyToReturn).GetValue('MSBuildToolsPath'))

        return $path
    }
}