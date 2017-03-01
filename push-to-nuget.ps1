nuget pack .\Raven.Bundles.NodaTime\Raven.Bundles.NodaTime.csproj -Prop Configuration=Release -Symbols
nuget pack .\Raven.Client.NodaTime\Raven.Client.NodaTime.csproj -Prop Configuration=Release -Symbols

Get-ChildItem . -Filter *.nupkg | `
Foreach-Object{
  If($_.Name -notmatch '.symbols.'){
    nuget push $_.Name -Source https://www.nuget.org/api/v2/package
  }
}
del *.nupkg