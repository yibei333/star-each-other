#! /bin/bash

cd ../src/StarEachOther.Desktop

dotnet publish -c Release --sc -r osx-x64 -o bin/GithubStarEachOther_x64

cd bin

sudo chmod +x ./GithubStarEachOther_x64/GithubStarEachOther

tar -czf GithubStarEachOther_osx_x64.tar.gz ./GithubStarEachOther_x64
