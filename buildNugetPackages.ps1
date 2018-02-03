rm *.nupkg
nuget pack .\AudioBuddy.nuspec -IncludeReferencedProjects -Prop Configuration=Release
cp *.nupkg C:\Projects\Nugets\
nuget push -source https://www.nuget.org -NonInteractive *.nupkg