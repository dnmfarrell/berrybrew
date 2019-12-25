@echo off
mkdir build
mkdir build\data

copy dev\data\*.json build\data
copy bin\env.exe build
copy bin\libintl3.dll build
copy bin\libiconv2.dll build

call perl -i.bak -ne "s/berrybrew(?!\\\\build)/berrybrew\\\\build/; print" build/data/config.json

echo "compiling dll..."

call mcs^
    -lib:bin^
    -t:library^
    -r:Newtonsoft.Json.dll,ICSharpCode.SharpZipLib.dll^
    -out:build\bbapi.dll^
    src\berrybrew.cs

copy bin\berrybrew-refresh.bat build\
copy bin\ICSharpCode.SharpZipLib.dll build\
copy bin\Newtonsoft.Json.dll build\
