@echo off

set /p VERSION_VALUE=<..\assets\version.txt

dotnet tool install --global single-exe

cd ../src/StarEachOther.Desktop

dotnet publish -c Release --sc -r win-x64 -o bin/publish

single-exe -b %cd%/bin/publish -e GithubStarEachOther.exe -v %VERSION_VALUE%

move .\bin\GithubStarEachOther.exe .\bin\GithubStarEachOther_win_x64.exe