nuget pack .\Raven.Bundles.NodaTime\Raven.Bundles.NodaTime.csproj
nuget pack .\Raven.Client.NodaTime\Raven.Client.NodaTime.csproj
nuget push *.nupkg
del *.nupkg