Framework '4.0'

properties {
    $project = 'Parsley'
    $birthYear = 2011
    $maintainers = "Patrick Lioi"

    $configuration = 'Release'
    $src = resolve-path '.\src'
    $build = if ($env:build_number -ne $NULL) { $env:build_number } else { '0' }
    $version = [IO.File]::ReadAllText('.\VERSION.txt') + '.' + $build

    $basedir = resolve-path '.\'
    $package_dir = "$basedir\package"
}

task default -depends Test

task Test -depends Compile {
    $xunitRunner = join-path $src "packages\xunit.runners.1.9.1\tools\xunit.console.clr4.exe"
    exec { & $xunitRunner $src\$project.Test\bin\$configuration\$project.Test.dll }
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
[assembly: AssemblyConfiguration(""$configuration"")]" | out-file "$src\CommonAssemblyInfo.cs" -encoding "ASCII"
}

task Package -depends Test {
    delete_directory $package_dir
    copy_files "$src\$project\bin\$configuration\" $package_dir
    
    $nuspec_file = "$package_dir\$project.nuspec"
    create-nuspec "$version" "$nuspec_file"
    exec { & $src\.nuget\NuGet.exe pack $nuspec_file -Symbols -OutputDirectory $package_dir }
}

function global:copy_files($source,$destination,$exclude=@()) {    
    create_directory $destination
    Get-ChildItem $source -Recurse -Exclude $exclude | Copy-Item -Destination {Join-Path $destination $_.FullName.Substring($source.length)} 
}

function global:delete_directory($directory_name) {
    rd $directory_name -recurse -force  -ErrorAction SilentlyContinue | out-null
}

function global:create_directory($directory_name) {
    mkdir $directory_name  -ErrorAction SilentlyContinue  | out-null
}

function global:create-nuspec($version,$filename) {
    "<?xml version=""1.0""?>
<package xmlns=""http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd"">
  <metadata>
    <id>Parsley</id>
    <version>$version</version>
    <authors>Patrick Lioi</authors>
    <owners>Patrick Lioi</owners>
    <licenseUrl>https://github.com/plioi/parsley/blob/master/LICENSE.txt</licenseUrl>
    <projectUrl>https://github.com/plioi/parsley</projectUrl>
    <iconUrl>https://github.com/plioi/parsley/raw/master/parsley.png</iconUrl>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <summary>A text parsing library.</summary>
    <description>Parsley is a monadic parser combinator library inspired by Haskell's Parsec library.  It can parse context-sensitive, infinite look-ahead grammars but it performs best on predictive LL[1] grammars.</description>
  </metadata>
  <files>
    <file src=""$package_dir\*.dll"" target=""lib\net40"" />
    <file src=""$package_dir\*.pdb"" target=""lib\net40"" />
    <file src=""$src\**\*.cs"" target=""src"" />
  </files>
</package>" | out-file $filename -encoding "ASCII"
}