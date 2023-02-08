$root = "..\Source"

Remove-Item -Path $root\bin -Recurse
Remove-Item -Path $root\obj -Recurse
Remove-Item -Path $root\idata -Recurse

$version = dotnet-gitversion /output json /showvariable MajorMinorPatch
$version
$tag = "imoutochan/imoutopics:" + $version
$tag

cd $root
docker build --tag=$tag -t imoutochan/imoutopics . -f Dockerfile
docker push imoutochan/imoutopics:$version
docker push imoutochan/imoutopics:latest

cd ..

pause