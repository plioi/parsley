Framework '4.0'

properties {
    $project = "Parsley"
    $birthYear = 2011
    $maintainers = "Patrick Lioi"
    $description = "A monadic parser combinator library inspired by Haskell's Parsec library."

    $configuration = 'Release'
    $src = resolve-path '.\src'
    $build = if ($env:build_number -ne $NULL) { $env:build_number } else { '0' }
    $version = [IO.File]::ReadAllText('.\VERSION.txt') + '.' + $build
}

task default -depends Test

task Package -depends Test {
    rd .\package -recurse -force -ErrorAction SilentlyContinue | out-null
    mkdir .\package -ErrorAction SilentlyContinue | out-null
    exec { & $src\.nuget\NuGet.exe pack $src\$project\$project.csproj -Symbols -Prop Configuration=$configuration -OutputDirectory .\package }

    write-host
    write-host "To publish these packages, issue the following command:"
    write-host "   nuget push .\package\$project.$version.nupkg"
}

task Test -depends Compile {
    $fixieRunner = join-path $src "packages\Fixie.0.0.1.133\lib\net45\Fixie.Console.exe"
    exec { & $fixieRunner $src\$project.Test\bin\$configuration\$project.Test.dll }
}

task Compile -depends CommonAssemblyInfo {
  exec { msbuild /t:clean /v:q /nologo /p:Configuration=$configuration $src\$project.sln }
  exec { msbuild /t:build /v:q /nologo /p:Configuration=$configuration $src\$project.sln }
}

task CommonAssemblyInfo {
    $date = Get-Date
    $year = $date.Year
    $copyrightSpan = if ($year -eq $birthYear) { $year } else { "$birthYear-$year" }
    $copyright = "Copyright (c) $copyrightSpan $maintainers"

"using System.Reflection;
using System.Runtime.InteropServices;

[assembly: ComVisible(false)]
[assembly: AssemblyProduct(""$project"")]
[assembly: AssemblyVersion(""$version"")]
[assembly: AssemblyFileVersion(""$version"")]
[assembly: AssemblyCopyright(""$copyright"")]
[assembly: AssemblyCompany(""$maintainers"")]
[assembly: AssemblyDescription(""$description"")]
[assembly: AssemblyConfiguration(""$configuration"")]" | out-file "$src\CommonAssemblyInfo.cs" -encoding "ASCII"
}