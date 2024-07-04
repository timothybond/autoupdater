dotnet build ..\autoupdater.sln -c Release

$version = [System.Diagnostics.FileVersionInfo]::GetVersionInfo((Get-Location).Path +"\bin\Release\net8.0\Main.dll").FileVersion

$runtimes = 'win-x86', 'win-x64', 'win-arm64', 'linux-x64', 'linux-arm', 'linux-arm64', 'osx-x64', 'osx-arm64'

foreach ($runtime in $runtimes)
{
    dotnet publish -c Release -r $runtime
    ..\AutoUpdater.PostBuild\bin\Release\net8.0\AutoUpdater.PostBuild.exe -p $version -r $runtime -d ../AutoUpdater.Main/bin/Release/net8.0
}