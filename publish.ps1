
$packages = gci "_build\*.nupkg"
nuget push $packages