.\.nuget\nuget.exe pack .\Raven.Bundles.NodaTime\Raven.Bundles.NodaTime.csproj
.\.nuget\nuget.exe pack .\Raven.Client.NodaTime\Raven.Client.NodaTime.csproj
.\.nuget\nuget.exe push *.nupkg
del *.nupkg